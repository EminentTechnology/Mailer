using Mailer.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mailer.Attachments.Sql
{
    public class SqlAttachmentProvider : IEmailAttachmentProvider
    {
        private readonly string NameOrConnectionString = null;

        public SqlAttachmentProvider()
        {

        }

        public SqlAttachmentProvider(string nameOrConnectionString)
        {
            NameOrConnectionString = nameOrConnectionString;
            
        }

        public async Task<byte[]> GetAttachmentSource(string documentId)
        {
            byte[] retVal = null;

            using (var db = GetDbContext())
            {
                db.Database.Log = Console.WriteLine;
                var document = await db.Documents.FindAsync(new Guid(documentId));

                retVal = document.File;


            }

            return retVal;
        }

        private AttachmentContext GetDbContext()
        {
            if (String.IsNullOrEmpty(NameOrConnectionString))
                return new AttachmentContext();
            else
                return new AttachmentContext(NameOrConnectionString);
        }
    }
}
