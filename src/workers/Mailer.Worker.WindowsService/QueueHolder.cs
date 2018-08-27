using Mailer.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mailer.Worker.WindowsService
{
    public class QueueHolder
    {
        public IEmailQueue Queue { get; set; }
        public IEmailSender Sender { get; set; }
        
        public IEmailRecorder Recorder { get; set; }

        public IEmailAttachmentProvider AttachmentProvider { get; set; }

        public QueueConfigurationElement Config { get; set; }

        public Object Settings { get; set; }
    }
}
