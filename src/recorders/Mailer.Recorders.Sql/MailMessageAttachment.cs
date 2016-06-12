using Mailer.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mailer.Recorders.Sql
{
    public class MailMessageAttachment
    {
        public string MailMessageAttachmentID { get; set; }

        public string MailMessageID { get; set; }

        public MailMessage MailMessage { get; set; }

        public string DocumentID { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        internal static MailMessageAttachment Populate(MailMessageAttachment attachment, EmailMessage email, EmailAttachment emailAttachment)
        {
            attachment.MailMessageAttachmentID = emailAttachment.AttachmentId;
            attachment.MailMessageID = email.Id;
            attachment.DocumentID = emailAttachment.AttachmentId;
            attachment.CreatedOn = email.CreatedOn;

            return attachment;
        }
    }
}
