using Mailer.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mailer.Recorders.Sql
{
    public class MailMessage
    {
        public string MailMessageID { get; set; }

        public string MailSubject { get; set; }
        public string MailBody { get; set; }

        public string Priority { get; set; }

        public bool IsBodyHtml { get; set; }

        public bool IsSent { get; set; }

        public DateTimeOffset? MailSentDate { get; set; }

        public string Template { get; set; }

        public string EntityType { get; set; }

        public string EntityID { get; set; }

        public string ErrorMessage { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public string CreatedBy { get; set; }

        public DateTimeOffset ModifiedOn { get; set; }

        public string ModifiedBy { get; set; }

        public List<MailMessageAddress> Addresses { get; set; }
        public List<MailMessageAttachment> Attachments { get; set; }

        internal static MailMessage Populate(MailMessage message, EmailMessage email)
        {

            message.MailMessageID = email.Id;

            message.MailSubject = email.Subject;
            message.MailBody = email.Body;
            message.Priority = email.Priority.ToString();
            message.IsBodyHtml = email.IsBodyHtml;

            message.Template = email.Template;
            message.EntityType = email.EntityType;
            message.EntityID = email.Id;
            message.CreatedOn = email.CreatedOn;
            message.CreatedBy = email.From.DisplayName;
            message.ModifiedOn = email.CreatedOn;
            message.ModifiedBy = email.From.DisplayName;


            return message;
        }
    }
}
