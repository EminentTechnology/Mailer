using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mailer.Sql
{
    class GetMessageResponse
    {
        public long Id { get; set; }
        public string Payload { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

    }
}
