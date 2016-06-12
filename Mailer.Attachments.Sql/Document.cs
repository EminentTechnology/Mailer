using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mailer.Attachments.Sql
{
    public class Document
    {
        public Guid DocumentID { get; set; }
        public byte[] File { get; set; }

    }
}
