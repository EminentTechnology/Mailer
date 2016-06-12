using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mailer.Abstractions
{
    public class EmailAddress
    {
        public EmailAddress()
        {

        }

        public EmailAddress(string address)
        {
            this.Address = address;
        }

        public EmailAddress(string address, string displayName)
        {
            this.Address = address;
            this.DisplayName = displayName;
        }

        public string Address { get; set; }

        string _DisplayName = null;
        public string DisplayName
        {
            get
            {
                return _DisplayName ?? Address;
            }

            set
            {
                _DisplayName = value;
            }
        }

        
    }
}
