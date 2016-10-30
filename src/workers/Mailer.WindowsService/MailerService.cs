using Eminent.Service.Helper;
using Mailer.Abstractions;
using Mailer.Attachments.Sql;
using Mailer.Recorders.Sql;
using Mailer.Smtp;
using Mailer.Sql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Mailer.WindowsService
{
    partial class MailerService : DebuggableService
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Timer timer = new Timer();

        private readonly List<QueueHolder> queueHolders;

        public MailerService()
        {
            log4net.Config.XmlConfigurator.Configure();

            InitializeComponent();

            QueueConfigurationSection queueSetup = (QueueConfigurationSection)ConfigurationManager.GetSection("queues");

            queueHolders = SetupQueueHolders(queueSetup);
            

            timer.Enabled = false;
            timer.AutoReset = false;
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
        }

        private List<QueueHolder> SetupQueueHolders(QueueConfigurationSection queueSetup)
        {
            List<QueueHolder> retVal = new List<QueueHolder>();

            foreach (QueueConfigurationElement queue in queueSetup.Queues)
            {
                QueueHolder q = SetupQueueHolder(queue);
                retVal.Add(q);
            }

            return retVal;
        }

        private QueueHolder SetupQueueHolder(QueueConfigurationElement config)
        {
            QueueHolder retVal = new QueueHolder();

            retVal.Config = config;

            switch (config.Queue.ToLower())
            {
                case "mailer.sql.sqlqueue":
                    retVal.Queue = new SqlQueue($"name={config.QueueConnectionStringKey}");
                    break;
            }

            switch (config.Recorder.ToLower())
            {
                case "mailer.recorders.sql.sqlrecorder":
                    if (String.IsNullOrEmpty(config.RecorderMailMessageSql))
                    {
                        //use default sql
                        retVal.Recorder = new SqlRecorder($"name={config.RecorderConnectionStringKey}");
                    }
                    else
                    {
                        //use custom sql
                        retVal.Recorder = 
                            new SqlRecorder
                            (
                                $"name={config.RecorderConnectionStringKey}",
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
                    retVal.AttachmentProvider = new SqlAttachmentProvider($"name={config.RecorderConnectionStringKey}");
                    break;
            }

            switch (config.Sender.ToLower())
            {
                case "mailer.smtp.smtpsender":
                    SmtpSenderConfiguration smtpConfig = GetSmtpSenderConfiguration(config);
                    SmtpSender smtpSender = new SmtpSender(retVal.AttachmentProvider, smtpConfig, retVal.Recorder);
                    retVal.Sender = smtpSender;
                    break;

            }
       

            return retVal;
        }

        private SmtpSenderConfiguration GetSmtpSenderConfiguration(QueueConfigurationElement config)
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

        protected override void OnStart(string[] args)
        {
            int interval = 60000; //every minute

            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["POLLING_INTERVAL_SECONDS"]))
            {
                try
                {
                    interval = Convert.ToInt32(ConfigurationManager.AppSettings["POLLING_INTERVAL_SECONDS"]) * 1000;
                    timer.Interval = interval;
                }
                catch { }
            }

            log.Debug($"Checking for outbound email messages every {interval} ms");

            ProcessEmailMessages();

        }

        protected override void OnStop()
        {
            log.Info("stopped");
            timer.Stop();

        }

        protected override void OnContinue()
        {
            log.Info("continuing - checking for outbound email messages");

            ProcessEmailMessages();

        }

        protected override void OnPause()
        {
            log.Info("pausing");
            timer.Stop();
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ProcessEmailMessages();
        }

        private void ProcessEmailMessages()
        {
            try
            {
                log.Debug("Checking for outbound email messages");

                foreach (var queue in queueHolders)
                {
                    try
                    {
                        SendMessagesInQueue(queue);
                    }
                    catch (Exception ex)
                    {
                        Program.LogException($"Error encountered while processing queue - {queue.Config.Name}", ex, false);
                    }
                    
                }


            }
            catch (Exception ex)
            {

                Program.LogException(ex);
            }
            finally
            {
                timer.Start();
            }

        }



        private async void SendMessagesInQueue(QueueHolder holder)
        {
            if (holder.Config.QueueReceiveBatchSize > 1)
            {
                List<EmailMessage> messages = await holder.Queue.GetMessages(holder.Config.QueueReceiveBatchSize);

                log.Debug($"{messages.Count} emails in {holder.Config.Name} queue");


                foreach (var message in messages)
                {
                    try
                    {
                        await holder.Sender.Send(message);
                        //Program.LogDebug($"should have sent message {message.Id} ");
                    }
                    catch (Exception ex)
                    {
                        log.Error($"Error processing message in {holder.Config.Name} queue", ex);
                    }
                }
            }
            else
            {
                EmailMessage message = await holder.Queue.GetMessage();

                if (message != null)
                {
                    log.Debug($"1 email received from  {holder.Config.Name} queue");


                    try
                    {
                        await holder.Sender.Send(message);
                        //Program.LogDebug($"should have sent message {message.Id} ");
                    }
                    catch (Exception ex)
                    {
                        log.Error($"Error processing message in {holder.Config.Name} queue", ex);
                        await holder.Recorder.RecordFailure(message, ex);
                    }
                }
                else
                {
                    log.Debug($"0 emails received from  {holder.Config.Name} queue");
                }
                
            }
            
        }

       

    }
}
