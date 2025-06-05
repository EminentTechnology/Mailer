using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Mailer.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Mailer.EmailQueue.AzureStorage
{
    public class AzureStorageEmailQueue : IEmailQueue
    {
        private readonly QueueClient queueClient;
        private readonly BlobEmailStorage blobStorage;
        private readonly int maxInlineSize;

        public AzureStorageEmailQueue(string connectionString, string queueName = "email-queue", 
            string blobContainerName = "email-messages", int maxInlineSize = 48 * 1024)
        {
            this.queueClient = new QueueClient(connectionString, queueName, new QueueClientOptions
            {
                MessageEncoding = QueueMessageEncoding.Base64
            });
            this.queueClient.CreateIfNotExists();
            
            this.blobStorage = new BlobEmailStorage(connectionString, blobContainerName);
            this.maxInlineSize = maxInlineSize;
        }

        public async Task QueueMessage(EmailMessage message)
        {
            // Assign ID if not already set
            if (string.IsNullOrEmpty(message.Id))
                message.Id = Guid.NewGuid().ToString();

            // Convert to JSON
            string jsonMessage = JsonConvert.SerializeObject(message);
            byte[] messageBytes = Encoding.UTF8.GetBytes(jsonMessage);
            
            // Check if message is too large for queue
            if (messageBytes.Length > maxInlineSize)
            {
                // Store in blob and queue reference
                string blobPath = await blobStorage.StoreEmailMessage(message);
                
                // Create reference payload
                var referencePayload = new BlobReferencePayload
                {
                    BlobRef = blobPath,
                    MessageId = message.Id,
                    Subject = message.Subject, // Include for logging/diagnostics
                    CreatedOn = message.CreatedOn
                };
                
                string jsonReference = JsonConvert.SerializeObject(referencePayload);
                await queueClient.SendMessageAsync(jsonReference);
            }
            else
            {
                // Queue the full message directly
                await queueClient.SendMessageAsync(jsonMessage);
            }
        }

        public async Task<EmailMessage> GetMessage()
        {
            QueueMessage message = await queueClient.ReceiveMessageAsync();
            
            if (message == null)
                return null;
                
            try
            {
                EmailMessage emailMessage = await DeserializeQueueMessage(message);
                
                // Delete the message from the queue after successful processing
                await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
                
                return emailMessage;
            }
            catch (Exception)
            {
                // Message will become visible again after visibility timeout
                throw;
            }
        }

        public async Task<List<EmailMessage>> GetMessages(int messageCount)
        {
            var messages = await queueClient.ReceiveMessagesAsync(messageCount);
            List<EmailMessage> result = new List<EmailMessage>();
            
            foreach (var message in messages.Value)
            {
                try
                {
                    EmailMessage emailMessage = await DeserializeQueueMessage(message);
                    result.Add(emailMessage);
                    
                    // Delete the message from the queue
                    await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
                }
                catch (Exception)
                {
                    // Skip this message and continue with others
                    // It will become visible again after visibility timeout
                }
            }
            
            return result;
        }

        private async Task<EmailMessage> DeserializeQueueMessage(QueueMessage message)
        {
            string messageContent = message.MessageText;
            
            // Try to deserialize as BlobReferencePayload first
            try
            {
                var blobRef = JsonConvert.DeserializeObject<BlobReferencePayload>(messageContent);
                
                if (!string.IsNullOrEmpty(blobRef?.BlobRef))
                {
                    // This is a blob reference, fetch from blob storage
                    return await blobStorage.GetEmailMessage(blobRef.BlobRef);
                }
            }
            catch (Exception ex)
            {
                // Log the exception for diagnostic purposes
                Console.WriteLine($"Failed to deserialize as BlobReferencePayload: {ex.Message}");
            }
            
            // Try to deserialize as EmailMessage
            return JsonConvert.DeserializeObject<EmailMessage>(messageContent);
        }
    }
}
