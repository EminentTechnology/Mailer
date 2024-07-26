using Mailer.Abstractions;

namespace Mailer.Worker.WebJob
{
    public class QueueHolder
    {
        public IEmailQueue Queue { get; set; }
        public IEmailSender Sender { get; set; }

        public IEmailRecorder Recorder { get; set; }

        public IEmailAttachmentProvider AttachmentProvider { get; set; }

        public QueueConfigurationElement Config { get; set; }

        public object Settings { get; set; }
    }
}
