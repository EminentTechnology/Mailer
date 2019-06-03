using Dapper;
using Dapper.Contrib.Extensions;
using Mailer.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Xml.Linq;

namespace Mailer.Sql
{
    public class SqlQueue : IEmailQueue
    {
        private readonly string NameOrConnectionString = null;
        readonly System.Xml.Serialization.XmlSerializer serializer = null;
        public SqlQueue()
        {
            serializer = new System.Xml.Serialization.XmlSerializer(typeof(Mailer.Abstractions.EmailMessage));
        }

        public SqlQueue(string nameOrConnectionString)
        {
            NameOrConnectionString = nameOrConnectionString;
            serializer = new System.Xml.Serialization.XmlSerializer(typeof(Mailer.Abstractions.EmailMessage));
        }

        public async Task<EmailMessage> GetMessage()
        {
            EmailMessage retVal = null;

            using (var conn = SqlConnectionHelper.GetConnection(NameOrConnectionString))
            {
                CommandDefinition command = new CommandDefinition
                (
                    @"delete top(1) from mailmessagequeue
                        output deleted.Id, deleted.Payload, deleted.CreatedOn
                        where Id = (
                        select top(1) Id
                          from mailmessagequeue with (rowlock, updlock, readpast)
                        order by Id)"
                );

                conn.Open();

                var item = await conn.QueryFirstAsync<GetMessageResponse>(command);

                if (item != null)
                {
                    retVal = ConvertStringPayloadToEmailMessage(item.Id, item.Payload);
                    retVal.CreatedOn = item.CreatedOn;
                }

            }
            

            return retVal;
        }

       

        public async Task<List<EmailMessage>> GetMessages(int messageCount=2)
        {
            List<EmailMessage> emails = new List<EmailMessage>();

            using (var conn = SqlConnectionHelper.GetConnection(NameOrConnectionString))
            {
                CommandDefinition command = new CommandDefinition
                (
                    $@"delete top({messageCount}) from mailmessagequeue
                        output deleted.Id, deleted.Payload, deleted.CreatedOn
                        where Id in (
                        select top({messageCount}) Id
                          from mailmessagequeue with (rowlock, updlock, readpast)
                        order by Id)"
                );

                conn.Open();

                var items = await conn.QueryAsync<GetMessageResponse>(command);

                foreach (var item in items)
                {
                    EmailMessage m = ConvertStringPayloadToEmailMessage(item.Id, item.Payload);
                    m.CreatedOn = item.CreatedOn;
                    emails.Add(m);
                }

            }
            //using (var db = GetDbContext())
            //{
            //    db.Database.Log = Console.WriteLine;


            //    var items = await db.Database.SqlQuery<GetMessageResponse>(
            //           $@"delete top({messageCount}) from mailmessagequeue
            //            output deleted.Id, deleted.Payload, deleted.CreatedOn
            //            where Id in (
            //            select top({messageCount}) Id
            //              from mailmessagequeue with (rowlock, updlock, readpast)
            //            order by Id)").ToListAsync();

            //    //S retVal.Body = payload;
            //    foreach (var item in items)
            //    {
            //        EmailMessage m = ConvertStringPayloadToEmailMessage(item.Id, item.Payload);
            //        m.CreatedOn = item.CreatedOn;
            //        emails.Add(m);
            //    }

            //}

            return emails;
        }

        public async Task QueueMessage(EmailMessage message)
        {
            MailMessageQueue queuedMessage = new MailMessageQueue();

            queuedMessage.Subject = message.Subject;
            queuedMessage.Template = message.Template;
            queuedMessage.EntityType = message.EntityType;
            queuedMessage.EntityID = message.EntityId;
            queuedMessage.CreatedOn = message.CreatedOn;
            queuedMessage.CreatedBy = message.From.DisplayName;
            queuedMessage.Payload = ConvertToStringPayload(message);

            using (var conn = SqlConnectionHelper.GetConnection(NameOrConnectionString))
            {
                conn.Open();
                conn.Insert(queuedMessage);

                message.Id = queuedMessage.Id.ToString();
            }
        }

        private  string ConvertToStringPayload(EmailMessage message)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.CloseOutput = true;
            settings.Encoding = Encoding.Unicode;
            settings.OmitXmlDeclaration = true;

            StringBuilder sb = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                serializer.Serialize(writer, message);
                writer.Close();
            }

            return sb.ToString();
        }

        private EmailMessage ConvertStringPayloadToEmailMessage(long id, string payload)
        {
            EmailMessage item = null;

            XDocument doc = XDocument.Parse(payload);

            XmlReader reader = doc.CreateReader();
            reader.MoveToContent();

            try
            {
                item = (EmailMessage)serializer.Deserialize(reader);
                item.Id = id.ToString();
            }
            catch (Exception ex)
            {
                throw new ApplicationException(String.Format("Error occurred while processing EmailMessage - {0}", doc.Root.FirstNode.ToString()), ex);
            }
            return item;
        }
    }
}
