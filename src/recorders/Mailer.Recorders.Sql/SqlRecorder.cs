﻿using Dapper;
using Mailer.Abstractions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mailer.Recorders.Sql
{
    public class SqlRecorder : IEmailRecorder
    {
        private readonly string NameOrConnectionString = null;

        const string DEFAULT_MESSAGE_INSERT_SQL = @"INSERT INTO MailMessage 
        (
	        MailMessageID,
	        MailSubject,
	        MailBody,
	        Priority,
	        IsBodyHtml,
	        IsSent,
	        MailSentDate,
	        Template,
	        EntityType,
	        EntityID,
	        ErrorMessage,
	        CreatedOn,
	        CreatedBy,
	        ModifiedOn,
	        ModifiedBy
        )
        VALUES
        (
	        @MailMessageID,
	        @MailSubject,
	        @MailBody,
	        @Priority,
	        @IsBodyHtml,
	        @IsSent,
	        @MailSentDate,
	        @Template,
	        @EntityType,
	        @EntityID,
	        @ErrorMessage,
	        GetUtcDate(),
	        @CreatedBy,
	        GetUtcDate(),
	        @CreatedBy
        )";

        const string DEFAULT_MESSAGE_ADDRESS_INSERT_SQL = @"INSERT INTO MailMessageAddress 
        (
	        MailMessageAddressID,
	        MailMessageID,
	        AddressType,
	        EmailAddress,
	        DisplayName,
	        CreatedOn
        )
        VALUES
        (
	        @MailMessageAddressID,
	        @MailMessageID,
	        @AddressType,
	        @EmailAddress,
	        @DisplayName,
	        GetUtcDate()
        )";

        const string DEFAULT_MESSAGE_ATTACHMENT_INSERT_SQL = @"INSERT INTO MailMessageAttachment 
        (
            MailMessageAttachmentID,
            MailMessageID,
            DocumentID,
            CreatedOn
        )
        VALUES
        (
            @MailMessageAttachmentID,
            @MailMessageID,
            @DocumentID,
            GetDate()
        )";

        string MESSAGE_INSERT_SQL = "";
        string MESSAGE_ADDRESS_INSERT_SQL = "";
        string MESSAGE_ATTACHMENT_INSERT_SQL = "";

        public SqlRecorder()
        {
            MESSAGE_INSERT_SQL = DEFAULT_MESSAGE_INSERT_SQL;
            MESSAGE_ADDRESS_INSERT_SQL = DEFAULT_MESSAGE_ADDRESS_INSERT_SQL;
            MESSAGE_ATTACHMENT_INSERT_SQL = DEFAULT_MESSAGE_ATTACHMENT_INSERT_SQL;
        }

        public SqlRecorder(string nameOrConnectionString)
        {
            NameOrConnectionString = nameOrConnectionString;

            MESSAGE_INSERT_SQL = DEFAULT_MESSAGE_INSERT_SQL;
            MESSAGE_ADDRESS_INSERT_SQL = DEFAULT_MESSAGE_ADDRESS_INSERT_SQL;
            MESSAGE_ATTACHMENT_INSERT_SQL = DEFAULT_MESSAGE_ATTACHMENT_INSERT_SQL;
        }

        public SqlRecorder(string nameOrConnectionString, string messageSql, string addressSql, string attachmentSql)
        {
            NameOrConnectionString = nameOrConnectionString;

            MESSAGE_INSERT_SQL = messageSql;
            MESSAGE_ADDRESS_INSERT_SQL = addressSql;
            MESSAGE_ATTACHMENT_INSERT_SQL = attachmentSql;
        }

        public async Task RecordFailure(EmailMessage message, Exception error)
        {
            message.Id = Guid.NewGuid().ToString();

            var mailMessage = AddNewMailMessage(message);

            mailMessage.IsSent = false;
            mailMessage.ErrorMessage = error.Message;

            await InsertEmail(mailMessage);
            
        }

        

        public async Task RecordMessage(EmailMessage message)
        {
            message.Id = Guid.NewGuid().ToString();
            var mailMessage = AddNewMailMessage(message);

            await InsertEmail( mailMessage);
        }

        private static MailMessage AddNewMailMessage(EmailMessage message)
        {
            //insert message
            MailMessage mailMessage = new MailMessage();
            mailMessage.Addresses = new List<MailMessageAddress>();
            mailMessage.Attachments = new List<MailMessageAttachment>();
            mailMessage = MailMessage.Populate(mailMessage, message);

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

            var mailMessage = AddNewMailMessage(message);

            mailMessage.IsSent = true;
            mailMessage.ModifiedOn = DateTime.UtcNow;
            mailMessage.MailSentDate = mailMessage.ModifiedOn;

            await InsertEmail(mailMessage);
        }

        #region GetConnection
        private IDbConnection GetConnection()
        {
            IDbConnection connection = null;
            ConnectionStringSettings connectionStringSettings = null;
            string connectionString = null;

            var connectionStrings = ConfigurationManager.ConnectionStrings;

            //if name or connectionstring is provided
            if (!String.IsNullOrEmpty(NameOrConnectionString))
            {

                string[] segments = NameOrConnectionString.Split('=');
                
                //see how many segments we have in the provided name or connectionstring
                if (segments.Length > 0)
                {
                    var firstKey = segments[0].Trim();
                    //if only 2 segment and it is called name 
                    if ((segments.Length == 2) && (firstKey.ToLower() == "name"))
                    {
                        //then get the connectionstring from the connectionstrings node in config
                       connectionStringSettings = connectionStrings[segments[1]];
                    }
                    else
                    {
                        //otherwise this is a connectionstring
                        connectionString = NameOrConnectionString;
                    }
                }
                else
                {
                    //if not segment (ie - no equal sign) then retrieve from config by name
                    connectionStringSettings = connectionStrings[NameOrConnectionString];
                }

            }
            else
            {
                //nothing was specified - so use the first connection in connectionstrings
                if(connectionStrings.Count > 0)
                {
                    //take the first connection as default
                    connectionStringSettings = connectionStrings[0];

                }
            }

            if (connectionStringSettings != null)
            {
                connectionString = connectionStringSettings.ConnectionString;
            }

            //build connection
            connection = new SqlConnection(connectionString);

            return connection;
        }
        #endregion GetConnection
        private async Task InsertEmail(MailMessage mailMessage)
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                using (var transaction = conn.BeginTransaction())
                {
                    CommandDefinition mailMessageDefinition = GetMailMessageDefinition(mailMessage, transaction);
                    await conn.ExecuteAsync(mailMessageDefinition);

                    if (mailMessage.Addresses.Count > 0)
                    {
                        CommandDefinition addressDefinition = GetMailMessageAddressDefinition(mailMessage, transaction);
                        await conn.ExecuteAsync(addressDefinition);
                    }


                    if (mailMessage.Attachments.Count > 0)
                    {
                        CommandDefinition attachmentDefinition = GetMailMessageAttachmentDefinition(mailMessage, transaction);
                        await conn.ExecuteAsync(attachmentDefinition);
                    }

                    transaction.Commit();
                }
            }
        }

        private CommandDefinition GetMailMessageDefinition(MailMessage mailMessage, IDbTransaction transaction)
        {
            CommandDefinition retVal = new CommandDefinition
            (
                MESSAGE_INSERT_SQL,
                mailMessage,
                transaction
            );

            return retVal;
        }

        private CommandDefinition GetMailMessageAddressDefinition(MailMessage mailMessage, IDbTransaction transaction)
        {
            CommandDefinition retVal = new CommandDefinition
            (
                MESSAGE_ADDRESS_INSERT_SQL,
                mailMessage.Addresses,
                transaction
            );

            return retVal;
        }

        private CommandDefinition GetMailMessageAttachmentDefinition(MailMessage mailMessage, IDbTransaction transaction)
        {
            CommandDefinition retVal = new CommandDefinition
            (
                MESSAGE_ATTACHMENT_INSERT_SQL,
                mailMessage.Attachments,
                transaction
            );

            return retVal;
        }
    }
}
