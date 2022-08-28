
using Mailer.Abstractions;
using Mailer.Attachments.Sql;
using Mailer.Recorders.Sql;
using Mailer.SG;
using Mailer.Smtp;
using Mailer.Sql;
using Mailer.Worker.WindowsService;
using ServiceHelper;
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
        readonly MailerServiceHelper Service;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MailerService()
        {
            InitializeComponent();

            var queueConfiguration = LoadQueueConfiguration();
            Service = new MailerServiceHelper(queueConfiguration);
        }

        

        public  List<QueueHolder> LoadQueueConfiguration()
        {
            QueueConfigurationSection queueSetup = (QueueConfigurationSection)ConfigurationManager.GetSection("queues");

            List<QueueHolder> queueHolders = SetupQueueHolders(queueSetup);

            return queueHolders;
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

        private  QueueHolder SetupQueueHolder(QueueConfigurationElement config)
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
                    retVal.AttachmentProvider = new SqlAttachmentProvider($"name={config.AttachmentConnectionStringKey}", config.AttachmentSql);
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

        protected override void OnStart(string[] args)
        {
            try
            {
                Service.OnStart(args);
            }
            catch (Exception ex)
            {
                log.Error("Error during OnStart", ex);
            }
            
        }

        protected override void OnStop()
        {
            try
            {
                Service.OnStop();
            }
            catch (Exception ex)
            {
                log.Error("Error during OnStop", ex);
            }
        }

        protected override void OnContinue()
        {
            try
            {
                Service.OnContinue();
            }
            catch (Exception ex)
            {
                log.Error("Error during OnContinue", ex);
            }
        }

        protected override void OnPause()
        {
            try
            {
                Service.OnPause();
            }
            catch (Exception ex)
            {
                log.Error("Error during OnPause", ex);
            }
        }

    }
}
