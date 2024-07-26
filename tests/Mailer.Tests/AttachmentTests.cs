using System;

using System.Threading.Tasks;
using Mailer.Attachments.Sql;
using NUnit.Framework;

namespace Mailer.Tests
{
    [TestFixture]
    public class AttachmentTests
    {


        [Test]
        [Ignore("Integration")]
        public async Task Attachment_RetrievingDocumentByID_ReturnsDocument()
        {
            byte[] data = null;

            Assert.That(data, Is.Null);

            SqlAttachmentProvider q = new SqlAttachmentProvider("name=ConnectionString");
            data = await q.GetAttachmentSource("1B263DC3-6241-43CE-9D2D-021487C73C5C");

            Assert.That(data, Is.Null);
            Console.WriteLine("byte[] size = {0}", data.Length);
        }
    }
}
