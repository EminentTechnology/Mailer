using Mailer.Abstractions;
using Mailer.Sql;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Mailer.Tests
{
    [TestFixture]
    public class QueueTests
    {
        [Test]
        [Ignore("Integration")]
        public async Task Queue_QueueingEmail_ReturnsNewMessageId()
        {
            EmailMessage msg = 
                new EmailMessage(
                    "support@eminenttechnology.com", 
                    "pochu@eminenttechnology.com", 
                    $"test @ {DateTime.Now}",
                    "body");

            msg.Attachments.Add(new EmailAttachment {AttachmentId= "1B263DC3-6241-43CE-9D2D-021487C73C5C", ContentType= "application/pdf", FileName="my.pdf" });

            Assert.That(msg.Id, Is.Null);

            SqlQueue q = new SqlQueue();
            await q.QueueMessage(msg);

            Assert.That(msg.Id, Is.Not.Null);
        }

        [Test]
        [Ignore("Integration")]
        public async Task Queue_QueueingEmailRepeatedly_CreatesSeveralMessagesInQueue()
        {
            EmailMessage msg =
                new EmailMessage(
                    "support@eminenttechnology.com",
                    "pochu@eminenttechnology.com",
                    $"test @ {DateTime.Now}",
                    "body");

            Assert.That(msg.Id, Is.Null);

            SqlQueue q = new SqlQueue();
            await q.QueueMessage(msg);

            Assert.That(msg.Id, Is.Not.Null);

            for (int i = 0; i < 5; i++)
            {
                await q.QueueMessage(msg);
            }
        }
    }
}
