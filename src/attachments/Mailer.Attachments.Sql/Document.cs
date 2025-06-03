namespace Mailer.Attachments.Sql
{
    public class Document
    {
        public string DocumentId { get; set; }
        public string StorageType { get; set; }
        public string DocumentUrl { get; set; }
        public byte[] File { get; set; }
    }
}
