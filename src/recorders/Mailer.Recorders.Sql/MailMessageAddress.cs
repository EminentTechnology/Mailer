using Mailer.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mailer.Recorders.Sql
{
    public class MailMessageAddress
    {
        public string MailMessageAddressID { get; set; }

        public string MailMessageID { get; set; }

        public MailMessage MailMessage { get; set; }

        public string AddressType { get; set; }
        public string EmailAddress { get; set; }

        public string DisplayName { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        internal static MailMessageAddress Populate(MailMessageAddress address, EmailMessage email, string addressType, EmailAddress emailAddress)
        {

            address.MailMessageAddressID = Guid.NewGuid().ToString();
            address.MailMessageID = email.Id;
            address.AddressType = addressType;
            address.EmailAddress = emailAddress.Address;
            address.DisplayName = emailAddress.DisplayName;
            address.CreatedOn = email.CreatedOn;

            return address;
        }
    }
}
