namespace Mailer.Worker.WebJob
{
    public class QueueConfigurationElement
    {
        public string Name { get; set; }

        public string ConnectionString { get; set; }

        public string Queue { get; set; } = "Mailer.Sql.SqlQueue";
        public int QueueReceiveBatchSize { get; set; } = 100;

        public string Recorder { get; set; } = "Mailer.Recorders.Sql.SqlRecorder";
        public string RecorderMailMessageSql { get; set; }
        public string RecorderMailMessageAddressSql { get; set; }
        public string RecorderMailMessageAttachmentSql { get; set; }

        public string Sender { get; set; } = "Mailer.Smtp.SmtpSender";
        public string SenderUserName { get; set; }
        public string SenderPassword { get; set; }
        public string SenderHost { get; set; } = "smtp.sendgrid.net";
        public int SenderPort { get; set; } = 587;
        public bool SenderUseDefaultCredentials { get; set; } = false;
        public bool SenderEnableSsl { get; set; } = false;
        public string SenderPickupDirectoryLocation { get; set; }
        public int SenderTimeout { get; set; } = 100000;
        public string SenderTargetName { get; set; }

        public string AttachmentProvider { get; set; } = "Mailer.Attachments.Sql.SqlAttachmentProvider";
        public string AttachmentSql { get; set; }

        public System.Net.Mail.SmtpDeliveryMethod DeliveryMethod { get; set; } = System.Net.Mail.SmtpDeliveryMethod.Network;

    }
}