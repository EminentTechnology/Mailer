using System;

namespace Mailer.Sql
{
    class GetMessageResponse
    {
        public long Id { get; set; }
        public string Payload { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
    }
}
