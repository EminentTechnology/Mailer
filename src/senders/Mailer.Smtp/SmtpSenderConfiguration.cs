using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mailer.Smtp
{
    public class SmtpSenderConfiguration
    {
        public string Host { get; set; }
        public string PickupDirectoryLocation { get; set; }
        public int Port { get; set; }
        public int Timeout { get; set; }

        public string TargetName { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }
        public bool EnableSsl { get; set; }

        public bool UseDefaultCredentials { get; set; }
    }
}
