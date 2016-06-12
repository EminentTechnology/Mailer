using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mailer.Abstractions
{
    public class EmailAttachment
    {
        public string AttachmentId { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
    }
}
