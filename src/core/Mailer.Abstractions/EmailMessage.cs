using System;
using System.Collections.Generic;

namespace Mailer.Abstractions
{
    public class EmailMessage
    {
        public EmailMessage()
        {
            CreatedOn = DateTime.UtcNow;
        }

        public EmailMessage(string from, string recipients, string subject, string body)
        {
            CreatedOn = DateTime.UtcNow;
            BuildMessage(from, recipients, subject, body, null, null, null);
        }

        public EmailMessage(string from, string recipients, string subject, string body, string template, string entityType, string entityId)
        {
            CreatedOn = DateTime.UtcNow;
            BuildMessage(from, recipients, subject, body, template, entityType, entityId);
        }

        private void BuildMessage(string from, string recipients, string subject, string body, string template, string entityType, string entityId)
        {
            this.From.Address = from;
            this.Subject = subject;
            this.Body = body;
            this.Template = template;
            this.EntityType = entityType;
            this.EntityId = entityId;

            if (!String.IsNullOrEmpty(recipients))
            {
                string[] list = recipients.Split(';');
                for (int i = 0; i < list.Length; i++)
                {
                    To.Add(new EmailAddress(list[i]));
                }
            }
        }

        public string Id { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public MailPriority Priority { get; set; }
        public bool IsBodyHtml { get; set; }
        public string Template { get; set; }
        public string EntityType { get; set; }
        public string EntityId { get; set; }

        EmailAddress _From = new EmailAddress();
        List<EmailAddress> _To = new List<EmailAddress>();
        List<EmailAddress> _ReplyTo = new List<EmailAddress>();
        List<EmailAddress> _CC = new List<EmailAddress>();
        List<EmailAddress> _BCC = new List<EmailAddress>();
        List<EmailAttachment> _Attachments = new List<EmailAttachment>();

        public EmailAddress From
        {
            get { return _From; }
            set { _From = value; }
        }

        public List<EmailAddress> ReplyTo
        {
            get { return _ReplyTo; }
            set { _ReplyTo = value; }
        }


        public List<EmailAddress> To
        {
            get { return _To; }
            set { _To = value; }
        }

        public List<EmailAddress> CC
        {
            get { return _CC; }
            set { _CC = value; }
        }

        public List<EmailAddress> BCC
        {
            get { return _BCC; }
            set { _BCC = value; }
        }

        public List<EmailAttachment> Attachments
        {
            get { return _Attachments; }
            set { _Attachments = value; }
        }

        public DateTimeOffset CreatedOn { get; set; }

    }
}
