using System.Threading.Tasks;

namespace Mailer.Abstractions
{
    public interface IEmailSender
    {
        Task Send(EmailMessage message);
    }
}
