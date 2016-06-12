using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mailer.Abstractions
{
    public interface IEmailQueue
    {
        Task QueueMessage(EmailMessage message);
        Task<EmailMessage> GetMessage();
        Task<List<EmailMessage>> GetMessages(int messageCount);
    }
}
