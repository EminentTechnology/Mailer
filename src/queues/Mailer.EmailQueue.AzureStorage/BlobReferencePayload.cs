using System;

namespace Mailer.EmailQueue.AzureStorage
{
    public class BlobReferencePayload
    {
        /// <summary>
        /// Reference to the blob path where the full email message is stored
        /// </summary>
        public string BlobRef { get; set; }
        
        /// <summary>
        /// ID of the original email message
        /// </summary>
        public string MessageId { get; set; }
        
        /// <summary>
        /// Subject of the email (included for easier diagnostics)
        /// </summary>
        public string Subject { get; set; }
        
        /// <summary>
        /// When the email message was created
        /// </summary>
        public DateTimeOffset CreatedOn { get; set; }
    }
}
