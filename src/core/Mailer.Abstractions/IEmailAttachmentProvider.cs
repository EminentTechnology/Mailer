using System.Threading.Tasks;

namespace Mailer.Abstractions
{
    public interface IEmailAttachmentProvider
    {
        Task<byte[]> GetAttachmentSource(string documentId);
    }
}
