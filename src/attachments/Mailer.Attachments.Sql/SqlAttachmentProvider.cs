
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Mailer.Abstractions;
using Mailer.Sql;

namespace Mailer.Attachments.Sql
{
    public class SqlAttachmentProvider : IEmailAttachmentProvider
    {
        private readonly string NameOrConnectionString = null;


        private string AttachmentSQL = @"SELECT DocumentId, StorageType, DocumentUrl, [File] FROM Document WHERE DocumentId = @DocumentId";

        public SqlAttachmentProvider()
        {

        }

        public SqlAttachmentProvider(string nameOrConnectionString, string attachmentSQL=null)
        {
            NameOrConnectionString = nameOrConnectionString;

            if (String.IsNullOrWhiteSpace(attachmentSQL))
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
                        using (var client = new System.Net.WebClient())
                        {
                            retVal =  client.DownloadData(document.DocumentUrl);
                        }
                    }
                }
                
            }

            return retVal;
        }

  
        
    }
}
