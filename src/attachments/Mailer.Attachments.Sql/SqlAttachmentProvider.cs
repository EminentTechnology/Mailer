using Dapper;
using Mailer.Abstractions;
using Mailer.Sql;
using System;
using System.Threading.Tasks;

namespace Mailer.Attachments.Sql
{
    public class SqlAttachmentProvider : IEmailAttachmentProvider
    {
        private readonly string NameOrConnectionString = null;
        private string AttachmentSQL = @"SELECT CAST(DocumentId as NVARCHAR(50)) DocumentId, StorageType, DocumentUrl, [File] FROM Document WHERE DocumentId = @DocumentId";

        public SqlAttachmentProvider()
        {

        }

        public SqlAttachmentProvider(string nameOrConnectionString, string attachmentSQL=null)
        {
            NameOrConnectionString = nameOrConnectionString;

            if (!String.IsNullOrWhiteSpace(attachmentSQL))
            {
                AttachmentSQL = attachmentSQL;
            }

        }

        public async Task<byte[]> GetAttachmentSource(string DocumentId)
        {
            byte[] retVal = null;

            using (var conn = SqlConnectionHelper.GetConnection(NameOrConnectionString))
            {
                CommandDefinition command = new CommandDefinition
                (
                    AttachmentSQL,
                    new { DocumentId }
                );

                conn.Open();

                var document = await conn.QueryFirstAsync<Document>(command);

                if (String.IsNullOrWhiteSpace(document.StorageType))
                {
                    //assume DB - if no storage type provided
                    document.StorageType = "DB";
                }

                if (document.StorageType.ToLower() == "db")
                {
                    retVal = document.File;
                }
                else
                {

                    if (!String.IsNullOrWhiteSpace(document.DocumentUrl))
                    {
                        string url = document.DocumentUrl;

                        // Regular expression to check if the URL starts with a known protocol
                        if (!System.Text.RegularExpressions.Regex.IsMatch(url, @"^[a-zA-Z][a-zA-Z0-9+-.]*://"))
                        {
                            // If it doesn't match a known protocol, prepend "https://"
                            url = "https://" + url;
                        }

                        using (var client = new System.Net.WebClient())
                        {
                            retVal = await client.DownloadDataTaskAsync(new Uri(url));
                        }
                    }
                }
                
            }

            return retVal;
        } 
    }
}
