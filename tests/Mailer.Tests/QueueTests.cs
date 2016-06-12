using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mailer.Sql;
using Mailer.Abstractions;
using System.Threading.Tasks;
using Mailer.Smtp;

namespace Mailer.Tests
{
    [TestClass]
    public class QueueTests
    {
        [TestMethod]
        public async Task Queue_QueueingEmail_ReturnsNewMessageId()
        {
            EmailMessage msg = 
                new EmailMessage(
                    "support@eminenttechnology.com", 
                    "pochu@eminenttechnology.com", 
                    $"test @ {DateTime.Now}",
                    "body");

            msg.Attachments.Add(new EmailAttachment {AttachmentId= "1B263DC3-6241-43CE-9D2D-021487C73C5C", ContentType= "application/pdf", FileName="my.pdf" });

            Assert.IsNull(msg.Id);
            
            SqlQueue q = new SqlQueue();
            await q.QueueMessage(msg);

            Assert.IsNotNull(msg.Id);
        }

        [TestMethod]
        public async Task Queue_QueueingEmailRepeatedly_CreatesSeveralMessagesInQueue()
        {
            EmailMessage msg =
                new EmailMessage(
                    "support@eminenttechnology.com",
                    "pochu@eminenttechnology.com",
                    $"test @ {DateTime.Now}",
                    "body");

            Assert.IsNull(msg.Id);

            SqlQueue q = new SqlQueue();
            await q.QueueMessage(msg);

            Assert.IsNotNull(msg.Id);

            for (int i = 0; i < 5; i++)
            {
                await q.QueueMessage(msg);
            }

            
        }


        SmtpSenderConfiguration GetConfiguration()
        {
            SmtpSenderConfiguration retVal = new SmtpSenderConfiguration();

            //retVal.Host = "";
            //retVal.Port = "";
            //retVal.EnableSsl = "";

            //retVal.UseDefaultCredentials = "";
            //retVal.UserName = "";
            //retVal.Password = "";

            //retVal.PickupDirectoryLocation = "";

            //retVal.TargetName = "";
            //retVal.Timeout = "";


            return retVal;
        }

    }



}
