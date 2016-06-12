using Mailer.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mailer.SparkPost
{
    public class SparkPostSender : IEmailSender
    {
        public Task Send(EmailMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
