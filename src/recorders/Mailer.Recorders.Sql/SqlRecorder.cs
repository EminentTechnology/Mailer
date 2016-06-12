using Mailer.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mailer.Recorders.Sql
{
    public class SqlRecorder : IEmailRecorder
    {
        private readonly string NameOrConnectionString = null;
        
        public SqlRecorder()
        {

            
        }

        public SqlRecorder(string nameOrConnectionString)
        {
            NameOrConnectionString = nameOrConnectionString;
            
        }

        public async Task RecordFailure(EmailMessage message, Exception error)
        {

            message.Id = Guid.NewGuid().ToString();

            using (var db = GetDbContext())
            {
                db.Database.Log = Console.WriteLine;
                var mailMessage = AddNewMailMessage(message, db);

                mailMessage.IsSent = false;
                mailMessage.ErrorMessage = error.Message;

                await db.SaveChangesAsync();


            }
        }

        public async Task RecordMessage(EmailMessage message)
        {
            message.Id = Guid.NewGuid().ToString();

            using (var db = GetDbContext())
            {
                db.Database.Log = Console.WriteLine;
                var mailMessage = AddNewMailMessage(message, db);

       

                await db.SaveChangesAsync();


            }

            //using (var db = GetDbContext())
            //{
            //    db.Database.Log = Console.WriteLine;
            //    var mailMessage = await db.Messages.FindAsync(message.Id);

            //    if (mailMessage == null)
            //    {
            //        mailMessage = AddNewMailMessage(message, db);
            //    }
            //    else
            //    {
            //        //update

            //        mailMessage = MailMessage.Populate(mailMessage, message);
            //        //db.Messages.(mailMessage);
            //    }

            //    await db.SaveChangesAsync();


            //}
        }

        private static MailMessage AddNewMailMessage(EmailMessage message, MailMessageContext db)
        {
            //insert message
            MailMessage mailMessage = new MailMessage();
            mailMessage.Addresses = new List<MailMessageAddress>();
            mailMessage.Attachments = new List<MailMessageAttachment>();

            mailMessage = MailMessage.Populate(mailMessage, message);

            db.Messages.Add(mailMessage);

            MailMessageAddress from = new MailMessageAddress();
            from = MailMessageAddress.Populate(from, message, "FROM", message.From);
            mailMessage.Addresses.Add(from);

            foreach (var item in message.To)
            {
                MailMessageAddress address = new MailMessageAddress();
                address = MailMessageAddress.Populate(address, message, "TO", item);
                mailMessage.Addresses.Add(address);
            }

            foreach (var item in message.BCC)
            {
                MailMessageAddress address = new MailMessageAddress();
                address = MailMessageAddress.Populate(address, message, "BCC", item);
                mailMessage.Addresses.Add(address);
            }

            foreach (var item in message.CC)
            {
                MailMessageAddress address = new MailMessageAddress();
                address = MailMessageAddress.Populate(address, message, "CC", item);
                mailMessage.Addresses.Add(address);
            }

            foreach (var item in message.ReplyTo)
            {
                MailMessageAddress address = new MailMessageAddress();
                address = MailMessageAddress.Populate(address, message, "ReplyTo", item);
                mailMessage.Addresses.Add(address);
            }

            foreach (var item in message.Attachments)
            {
                MailMessageAttachment attachment = new MailMessageAttachment();
                attachment = MailMessageAttachment.Populate(attachment, message, item);
                mailMessage.Attachments.Add(attachment);
            }

            return mailMessage;
        }

        public async Task RecordSuccess(EmailMessage message)
        {

            message.Id = Guid.NewGuid().ToString();

            using (var db = GetDbContext())
            {
                db.Database.Log = Console.WriteLine;
                var mailMessage = AddNewMailMessage(message, db);

                mailMessage.IsSent = true;
                mailMessage.ModifiedOn = DateTime.UtcNow;
                mailMessage.MailSentDate = mailMessage.ModifiedOn;



                await db.SaveChangesAsync();


            }

          
        }

        private MailMessageContext GetDbContext()
        {
            if (String.IsNullOrEmpty(NameOrConnectionString))
                return new MailMessageContext();
            else
                return new MailMessageContext(NameOrConnectionString);
        }
    }
}
