using Mailer.Abstractions;
using Mailer.Azure.StorageEmailQueue;
using Mailer.Sql;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Mailer.TestHarness
{
    class Program
    {
        static IConfiguration config;
        static IEmailQueue azureQueue;
        static IEmailQueue sqlQueue;

        static async Task Main(string[] args)
        {
            // Set up configuration with user secrets
            config = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();

            // Initialize queue implementations
            InitializeQueues();

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Mailer Implementation Test Harness ===\n");
                Console.WriteLine("1. Test Azure Storage Queue Implementation");
                Console.WriteLine("2. Test SQL Queue Implementation");
                Console.WriteLine("3. Check Configuration");
                Console.WriteLine("4. Create Test Email with Attachment");
                Console.WriteLine("5. Exit");
                Console.Write("\nSelect option: ");

                var key = Console.ReadKey();
                Console.WriteLine();

                switch (key.KeyChar)
                {
                    case '1':
                        await TestAzureQueueImplementation();
                        break;
                    case '2':
                        await TestSqlQueueImplementation();
                        break;
                    case '3':
                        CheckConfiguration();
                        break;
                    case '4':
                        await TestEmailWithAttachment();
                        break;
                    case '5':
                        return;
                    default:
                        Console.WriteLine("Invalid option. Press any key to continue...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        static void InitializeQueues()
        {
            try 
            {
                var azureStorageConn = config["AzureStorageConnectionString"];
                if (!string.IsNullOrEmpty(azureStorageConn))
                {
                    azureQueue = new AzureStorageEmailQueue(azureStorageConn);
                    Console.WriteLine("Azure Storage Queue initialized successfully.");
                }
                
                var sqlConn = config["SqlConnectionString"];
                if (!string.IsNullOrEmpty(sqlConn))
                {
                    sqlQueue = new SqlQueue(sqlConn);
                    Console.WriteLine("SQL Queue initialized successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing queues: {ex.Message}");
            }
        }

        static async Task TestAzureQueueImplementation()
        {
            Console.Clear();
            Console.WriteLine("=== Testing Azure Storage Queue Implementation ===\n");
            
            if (azureQueue == null)
            {
                Console.WriteLine("Azure Storage Queue not initialized. Check configuration.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }
            
            try
            {
                // Create test message
                var message = CreateTestEmailMessage();
                
                // Queue the message
                Console.WriteLine("Queueing test message...");
                await azureQueue.QueueMessage(message);
                Console.WriteLine($"Message queued successfully. ID: {message.Id}");
                
                // Optionally retrieve the message
                Console.WriteLine("\nWould you like to retrieve the message from the queue? (y/n)");
                var retrieve = Console.ReadKey().KeyChar == 'y';
                Console.WriteLine();
                
                if (retrieve)
                {
                    Console.WriteLine("Retrieving message from queue...");
                    var retrievedMessage = await azureQueue.GetMessage();
                    
                    if (retrievedMessage != null)
                    {
                        Console.WriteLine("Message retrieved successfully:");
                        Console.WriteLine($"  ID: {retrievedMessage.Id}");
                        Console.WriteLine($"  Subject: {retrievedMessage.Subject}");
                        Console.WriteLine($"  From: {retrievedMessage.From.Address}");
                        Console.WriteLine($"  To: {string.Join(", ", retrievedMessage.To.ConvertAll(t => t.Address))}");
                    }
                    else
                    {
                        Console.WriteLine("No message found in queue.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error testing Azure Queue: {ex.Message}");
                Console.WriteLine(ex.ToString());
            }
            
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        static async Task TestSqlQueueImplementation()
        {
            Console.Clear();
            Console.WriteLine("=== Testing SQL Queue Implementation ===\n");
            
            if (sqlQueue == null)
            {
                Console.WriteLine("SQL Queue not initialized. Check configuration.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }
            
            try
            {
                // Create test message
                var message = CreateTestEmailMessage();
                
                // Queue the message
                Console.WriteLine("Queueing test message...");
                await sqlQueue.QueueMessage(message);
                Console.WriteLine($"Message queued successfully. ID: {message.Id}");
                
                // Optionally retrieve the message
                Console.WriteLine("\nWould you like to retrieve the message from the queue? (y/n)");
                var retrieve = Console.ReadKey().KeyChar == 'y';
                Console.WriteLine();
                
                if (retrieve)
                {
                    Console.WriteLine("Retrieving message from queue...");
                    var retrievedMessage = await sqlQueue.GetMessage();
                    
                    if (retrievedMessage != null)
                    {
                        Console.WriteLine("Message retrieved successfully:");
                        Console.WriteLine($"  ID: {retrievedMessage.Id}");
                        Console.WriteLine($"  Subject: {retrievedMessage.Subject}");
                        Console.WriteLine($"  From: {retrievedMessage.From.Address}");
                        Console.WriteLine($"  To: {string.Join(", ", retrievedMessage.To.ConvertAll(t => t.Address))}");
                    }
                    else
                    {
                        Console.WriteLine("No message found in queue.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error testing SQL Queue: {ex.Message}");
                Console.WriteLine(ex.ToString());
            }
            
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        static async Task TestEmailWithAttachment()
        {
            Console.Clear();
            Console.WriteLine("=== Create Test Email with Attachment ===\n");

            try
            {
                // Choose queue implementation
                Console.WriteLine("Select queue implementation:");
                Console.WriteLine("1. Azure Storage Queue");
                Console.WriteLine("2. SQL Queue");
                Console.Write("Select (1/2): ");
                var queueChoice = Console.ReadKey().KeyChar;
                Console.WriteLine();

                IEmailQueue queue = null;
                switch (queueChoice)
                {
                    case '1':
                        queue = azureQueue;
                        break;
                    case '2':
                        queue = sqlQueue;
                        break;
                    default:
                        Console.WriteLine("Invalid choice.");
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        return;
                }

                if (queue == null)
                {
                    Console.WriteLine("Selected queue is not initialized. Check configuration.");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    return;
                }

                // Create test message with attachment
                var message = CreateTestEmailMessage();

                // Create a temporary file for attachment
                string tempFile = Path.GetTempFileName();
                string attachmentContent = "This is a test attachment created by Mailer.TestHarness.\n";
                attachmentContent += $"Created on: {DateTime.Now}\n";
                attachmentContent += "This file can be safely deleted.";

                File.WriteAllText(tempFile, attachmentContent);

                // Add attachment to message
                EmailAttachment attachment = new EmailAttachment
                {
                    AttachmentId = Guid.NewGuid().ToString(),
                    FileName = "TestAttachment.txt",
                    ContentType = "text/plain",
                    Disposition = "attachment"
                };

                message.Attachments.Add(attachment);

                Console.WriteLine("Created test email with attachment.");
                Console.WriteLine($"Temporary file location: {tempFile}");
                Console.WriteLine("NOTE: In a real scenario, the attachment bytes would be stored ");
                Console.WriteLine("      separately and retrieved by IEmailAttachmentProvider.");

                // Queue the message
                Console.WriteLine("\nQueueing message with attachment reference...");
                await queue.QueueMessage(message);
                Console.WriteLine($"Message queued successfully. ID: {message.Id}");

                Console.WriteLine("\nNOTE: The actual attachment contents would need to be stored separately");
                Console.WriteLine("      through your attachment provider implementation before it can be sent.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating email with attachment: {ex.Message}");
                Console.WriteLine(ex.ToString());
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        static EmailMessage CreateTestEmailMessage()
        {
            Console.WriteLine("Creating test email message...");
            
            var message = new EmailMessage();
            message.Subject = $"Test Email {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            message.Body = "<h1>Test Email</h1><p>This is a test email message sent from the Mailer.TestHarness application.</p>";
            message.IsBodyHtml = true;
            message.From.Address = "test@example.com";
            message.From.DisplayName = "Test Sender";
            message.To.Add(new EmailAddress("recipient@example.com", "Test Recipient"));
            
            return message;
        }

        static void CheckConfiguration()
        {
            Console.Clear();
            Console.WriteLine("=== Configuration Status ===\n");
            
            var azureConn = config["AzureStorageConnectionString"];
            var sqlConn = config["SqlConnectionString"];
            var sendGridKey = config["SendGridApiKey"];
            
            Console.WriteLine($"Azure Storage Connection: {(string.IsNullOrEmpty(azureConn) ? "Not configured" : "Configured")}");
            Console.WriteLine($"SQL Connection: {(string.IsNullOrEmpty(sqlConn) ? "Not configured" : "Configured")}");
            Console.WriteLine($"SendGrid API Key: {(string.IsNullOrEmpty(sendGridKey) ? "Not configured" : "Configured")}");
            
            Console.WriteLine("\nTo configure, use the following commands in a terminal:");
            Console.WriteLine("dotnet user-secrets set \"AzureStorageConnectionString\" \"your-connection-string\" --project tests\\Mailer.TestHarness");
            Console.WriteLine("dotnet user-secrets set \"SqlConnectionString\" \"your-sql-connection\" --project tests\\Mailer.TestHarness");
            Console.WriteLine("dotnet user-secrets set \"SendGridApiKey\" \"your-api-key\" --project tests\\Mailer.TestHarness");
            
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
    }
}
