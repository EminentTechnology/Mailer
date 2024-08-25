namespace Mailer.Abstractions
{
    public class EmailAttachment
    {
        public string AttachmentId { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public string Disposition { get; set; } 
        public string ContentId { get; set; }
    }
}
