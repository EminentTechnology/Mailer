using Azure.Storage.Queues;
using Mailer.Abstractions;
using Mailer.Azure.StorageEmailQueue;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Mailer.Worker.AzureFunctionWorker
{
    public class EmailProcessorFunction
    {
        private readonly IEmailSender emailSender;
        private readonly IEmailRecorder emailRecorder;
        private readonly IEmailAttachmentProvider attachmentProvider;
        private readonly BlobEmailStorage blobStorage;
        private readonly ILogger<EmailProcessorFunction> logger;

        public EmailProcessorFunction(
            IEmailSender emailSender,
            IEmailRecorder emailRecorder,
            IEmailAttachmentProvider attachmentProvider,
            BlobEmailStorage blobStorage,
            ILogger<EmailProcessorFunction> logger)
        {
            this.emailSender = emailSender;
            this.emailRecorder = emailRecorder;
            this.attachmentProvider = attachmentProvider;
            this.blobStorage = blobStorage;
            this.logger = logger;
        }

        [Function("ProcessEmailQueue")]
        public async Task Run([QueueTrigger("email-queue", Connection = "AzureWebJobsStorage")] string queueMessage)
        {
            EmailMessage? emailMessage = null;
            
            try
            {
                // Try to deserialize as BlobReferencePayload first
                var blobRef = JsonConvert.DeserializeObject<BlobReferencePayload>(queueMessage);
                
                if (!string.IsNullOrEmpty(blobRef?.BlobRef))
                {
                    // This is a blob reference, fetch from blob storage
                    logger.LogInformation($"Processing email from blob: {blobRef.BlobRef}");
                    emailMessage = await blobStorage.GetEmailMessage(blobRef.BlobRef);
                }
                else
                {
                    // This is a direct message
                    emailMessage = JsonConvert.DeserializeObject<EmailMessage>(queueMessage);
                }
                
                if (emailMessage == null)
                {
                    logger.LogError("Failed to deserialize email message");
                    return;
                }

                logger.LogInformation($"Processing email {emailMessage.Id}: {emailMessage.Subject}");
                
                // Send the email
                await emailSender.Send(emailMessage);
                
                // Record success
                await emailRecorder.RecordSuccess(emailMessage);
                
                logger.LogInformation($"Successfully processed email {emailMessage.Id}: {emailMessage.Subject}");
            }
            catch (FileNotFoundException ex)
            {
                // Specific handling for missing blob
                logger.LogError(ex, $"Email blob not found: {ex.Message}");
                
                if (emailMessage != null)
                {
                    await emailRecorder.RecordFailure(emailMessage, 
                        new Exception($"Email content was stored in blob but could not be retrieved: {ex.Message}", ex));
                }
            }
            catch (Exception ex)
            {
                // General error handling
                logger.LogError(ex, $"Error processing email message");
                
                if (emailMessage != null)
                {
                    await emailRecorder.RecordFailure(emailMessage, ex);
                }
            }
        }
    }
}
