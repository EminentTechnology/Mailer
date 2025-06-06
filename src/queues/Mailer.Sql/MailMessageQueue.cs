﻿using Dapper.Contrib.Extensions;
using System;

namespace Mailer.Sql
{
    [Table("MailMessageQueue")]
    public class MailMessageQueue
    {
        public long Id { get; set; }
        
        public string EntityType { get; set; }
        public string EntityID { get; set; }

        public string Template { get; set; }

        public string Subject { get; set; }

        public string Payload { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public string CreatedBy { get; set; }

    }
}
