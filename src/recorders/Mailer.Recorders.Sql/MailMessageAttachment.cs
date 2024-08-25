using Mailer.Abstractions;
using System;

namespace Mailer.Recorders.Sql
{
    public class MailMessageAttachment
    {
        public string MailMessageAttachmentID { get; set; }

        public string MailMessageID { get; set; }

        public MailMessage MailMessage { get; set; }

        public string DocumentID { get; set; }
        public string Disposition { get; set; }
        public string ContentId { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        internal static MailMessageAttachment Populate(MailMessageAttachment attachment, EmailMessage email, EmailAttachment emailAttachment)
        {
            attachment.MailMessageAttachmentID = Guid.NewGuid().ToString();
            attachment.MailMessageID = email.Id;
            attachment.DocumentID = emailAttachment.AttachmentId;
            attachment.Disposition = emailAttachment.Disposition;
            attachment.ContentId = emailAttachment.ContentId;
            attachment.CreatedOn = email.CreatedOn;

            return attachment;
        }
    }
}
