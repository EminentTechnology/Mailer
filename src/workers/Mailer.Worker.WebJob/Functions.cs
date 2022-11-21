using Mailer.Abstractions;
using Mailer.Attachments.Sql;
using Mailer.Recorders.Sql;
using Mailer.SG;
using Mailer.Smtp;
using Mailer.Sql;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mailer.Worker.WebJob
{
    public class Functions
    {
        public IConfiguration _config;
        private List<QueueHolder> queues = null;
        ILogger log = null;

        public Functions(IConfiguration configuration)
        {
            _config = configuration;
        }

        [Singleton]
        public async Task SendMessagesInQueue([TimerTrigger("0/30 * * * * *", RunOnStartup = true)] TimerInfo myTimer, ILogger log)
        {
            this.log = log;

            try
            {
                log.LogDebug("Checking for outbound email messages");

                foreach (var queue in GetQueues())
                {
                    try
                    {
                        await SendMessagesInQueue(queue);
                    }
                    catch (Exception ex)
                    {
                        log.LogError($"Error encountered while processing queue - {queue.Config.Name}", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
            }
        }

        private async Task SendMessagesInQueue(QueueHolder holder)
        {
            log.LogDebug($"{holder.Config.Name} queue batch size is set to {holder.Config.QueueReceiveBatchSize} emails");

            if (holder.Config.QueueReceiveBatchSize > 1)
            {
                List<EmailMessage> messages = await holder.Queue.GetMessages(holder.Config.QueueReceiveBatchSize);

                log.LogInformation($"{messages.Count} emails received from {holder.Config.Name} queue");


                foreach (var message in messages)
                {
                    var messageId = message.Id;

                    try
                    {
                        log.LogDebug($"Sending message {messageId}");
                        await holder.Sender.Send(message);
                        log.LogDebug($"Recorded message {messageId} - {message.Id}");
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, $"Error processing message {messageId} ({message.Id}) in {holder.Config.Name} queue");
                        await holder.Recorder.RecordFailure(message, ex);
                    }
                }
            }
            else
            {
                EmailMessage message = await holder.Queue.GetMessage();

                if (message != null)
                {
                    log.LogInformation($"1 email received from  {holder.Config.Name} queue");

                    var messageId = message.Id;

                    try
                    {
                        log.LogDebug($"Sending message {messageId}");
                        await holder.Sender.Send(message);
                        log.LogDebug($"Recorded message {messageId} - {message.Id}");
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, $"Error processing message {message.Id} in {holder.Config.Name} queue");
                        await holder.Recorder.RecordFailure(message, ex);
                    }
                }
                else
                {
                    log.LogDebug($"0 emails received from  {holder.Config.Name} queue");
                }
            }
        }

        private List<QueueHolder> GetQueues()
        {
            if (queues == null)
            {
                LoadQueues();
            }

            return queues;
        }

        private void LoadQueues()
        {
            var queues = new List<QueueHolder>();   
            var configElements = _config.GetSection("Queues").Get<List<QueueConfigurationElement>>();

            if (!configElements?.Any() ?? true)
            {
                log.LogDebug("Queues configuration not found.");
                return;
            }

            foreach (var item in configElements)
            {
                QueueHolder q = SetupQueueHolder(item);
                queues.Add(q);
            }

            this.queues = queues;
        }

        private QueueHolder SetupQueueHolder(QueueConfigurationElement config)
        {
            QueueHolder retVal = new QueueHolder();

            retVal.Config = config;

            switch (config.Queue.ToLower())
            {
                case "mailer.sql.sqlqueue":
                    retVal.Queue = new SqlQueue($"{config.ConnectionString}");
                    break;
            }

            switch (config.Recorder.ToLower())
            {
                case "mailer.recorders.sql.sqlrecorder":
                    if (String.IsNullOrEmpty(config.RecorderMailMessageSql))
                    {
                        //use default sql
                        retVal.Recorder = new SqlRecorder($"{config.ConnectionString}");
                    }
                    else
                    {
                        //use custom sql
                        retVal.Recorder =
                            new SqlRecorder
                            (
                                $"{config.ConnectionString}",
                                $"{config.RecorderMailMessageSql}",
                                $"{config.RecorderMailMessageAddressSql}",
                                $"{config.RecorderMailMessageAttachmentSql}"
                            );
                    }

                    break;
            }

            switch (config.AttachmentProvider.ToLower())
            {
                case "mailer.attachments.sql.sqlattachmentprovider":
                    retVal.AttachmentProvider = new SqlAttachmentProvider($"name={config.ConnectionString}", config.AttachmentSql);
                    break;
            }

            switch (config.Sender.ToLower())
            {
                case "mailer.smtp.smtpsender":
                    SmtpSenderConfiguration smtpConfig = GetSmtpSenderConfiguration(config);
                    SmtpSender smtpSender = new SmtpSender(retVal.AttachmentProvider, smtpConfig, retVal.Recorder);
                    retVal.Sender = smtpSender;
                    break;
                case "mailer.sg.sendgridsender":
                    SendGridSenderConfiguration sgConfig = GetSendGridSenderConfiguration(config);
                    SendGridSender sgSender = new SendGridSender(retVal.AttachmentProvider, sgConfig, retVal.Recorder);
                    retVal.Sender = sgSender;
                    break;
            }

            if (retVal.Queue == null)
            {
                throw new Exception($"Queue {config.Name} is not setup correctly - check queue configuration");
            }

            if (retVal.Sender == null)
            {
                throw new Exception($"Sender for Queue {config.Name} is not setup correctly - check queue configuration");
            }

            if (retVal.AttachmentProvider == null)
            {
                throw new Exception($"AttachmentProvider for Queue {config.Name} is not setup correctly - check queue configuration");
            }

            if (retVal.AttachmentProvider == null)
            {
                throw new Exception($"Recorder for Queue {config.Name} is not setup correctly - check queue configuration");
            }

            return retVal;
        }

        private static SmtpSenderConfiguration GetSmtpSenderConfiguration(QueueConfigurationElement config)
        {
            SmtpSenderConfiguration retVal = new SmtpSenderConfiguration();

            retVal.EnableSsl = config.SenderEnableSsl;
            retVal.Host = config.SenderHost;
            retVal.Password = config.SenderPassword;
            retVal.PickupDirectoryLocation = config.SenderPickupDirectoryLocation;
            retVal.Port = config.SenderPort;
            retVal.TargetName = config.SenderTargetName;
            retVal.Timeout = config.SenderTimeout;
            retVal.UseDefaultCredentials = config.SenderUseDefaultCredentials;
            retVal.UserName = config.SenderUserName;

            return retVal;
        }

        private static SendGridSenderConfiguration GetSendGridSenderConfiguration(QueueConfigurationElement config)
        {
            SendGridSenderConfiguration retVal = new SendGridSenderConfiguration();
            retVal.ApiKey = config.SenderUserName;

            return retVal;
        }
    }
}
