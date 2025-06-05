using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Mailer.Abstractions;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Mailer.EmailQueue.AzureStorage
{
    public class BlobEmailStorage
    {
        private readonly BlobContainerClient containerClient;

        public BlobEmailStorage(string connectionString, string containerName = "email-messages")
        {
            this.containerClient = new BlobContainerClient(connectionString, containerName);
            this.containerClient.CreateIfNotExists(PublicAccessType.None);
        }

        /// <summary>
        /// Stores an email message in blob storage
        /// </summary>
        /// <param name="message">The email message to store</param>
        /// <returns>The blob path where the message was stored</returns>
        public async Task<string> StoreEmailMessage(EmailMessage message)
        {
            string blobName = $"emails/{message.Id}-{DateTime.UtcNow:yyyyMMddHHmmss}.json";
            BlobClient blobClient = containerClient.GetBlobClient(blobName);
            
            string jsonMessage = JsonConvert.SerializeObject(message);
            byte[] messageBytes = Encoding.UTF8.GetBytes(jsonMessage);
            
            using (MemoryStream stream = new MemoryStream(messageBytes))
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }
            
            return blobName;
        }

        /// <summary>
        /// Retrieves an email message from blob storage
        /// </summary>
        /// <param name="blobPath">The blob path where the message is stored</param>
        /// <returns>The deserialized email message</returns>
        /// <exception cref="FileNotFoundException">Thrown when the blob cannot be found</exception>
        public async Task<EmailMessage> GetEmailMessage(string blobPath)
        {
            BlobClient blobClient = containerClient.GetBlobClient(blobPath);
            
            if (!await blobClient.ExistsAsync())
                throw new FileNotFoundException($"Email blob not found: {blobPath}");
                
            using (MemoryStream stream = new MemoryStream())
            {
                await blobClient.DownloadToAsync(stream);
                stream.Position = 0;
                
                using (StreamReader reader = new StreamReader(stream))
                {
                    string json = reader.ReadToEnd();
                    return JsonConvert.DeserializeObject<EmailMessage>(json);
                }
            }
        }

        /// <summary>
        /// Determines if an email message would be considered "large" based on its serialized size
        /// </summary>
        /// <param name="message">The email message to check</param>
        /// <param name="maxSize">Maximum size in bytes (default: 48KB)</param>
        /// <returns>True if the message exceeds the maximum size</returns>
        public bool IsLarge(EmailMessage message, int maxSize = 48 * 1024)
        {
            string jsonMessage = JsonConvert.SerializeObject(message);
            byte[] messageBytes = Encoding.UTF8.GetBytes(jsonMessage);
            return messageBytes.Length > maxSize;
        }
    }
}
