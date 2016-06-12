using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Mailer.Attachments.Sql;

namespace Mailer.Tests
{
    [TestClass]
    public class AttachmentTests
    {


        [TestMethod]
        public async Task Attachment_RetrievingDocumentByID_ReturnsDocument()
        {
            byte[] data = null;

            Assert.IsNull(data);

            SqlAttachmentProvider q = new SqlAttachmentProvider("name=ontrack");
            data = await q.GetAttachmentSource("1B263DC3-6241-43CE-9D2D-021487C73C5C");

            Assert.IsNotNull(data);
            Console.WriteLine("byte[] size = {0}", data.Length);
        }
    }
}
