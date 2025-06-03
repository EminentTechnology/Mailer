using System;
using System.Threading.Tasks;

namespace Mailer.Abstractions
{
    public interface IEmailRecorder
    {
        Task RecordSuccess(EmailMessage message);
        Task RecordFailure(EmailMessage message, Exception error);
    }
}
