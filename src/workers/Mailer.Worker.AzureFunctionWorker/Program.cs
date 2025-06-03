using Mailer.Abstractions;
using Mailer.Azure.StorageEmailQueue;
using Mailer.Attachments.Sql;
using Mailer.Recorders.Sql;
using Mailer.SG;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Add services to the container
builder.Services.AddApplicationInsightsTelemetryWorkerService();

// Configuration settings
var config = builder.Configuration;
var connectionString = config.GetValue<string>("AzureWebJobsStorage");
var sqlConnectionString = config.GetValue<string>("SqlConnectionString");
var sendGridApiKey = config.GetValue<string>("SendGridApiKey");
var emailQueueName = config.GetValue<string>("EmailQueueName", "email-queue");
var emailBlobContainer = config.GetValue<string>("EmailBlobContainer", "email-messages");

// Register blob storage helper
builder.Services.AddSingleton(sp => 
    new BlobEmailStorage(connectionString, emailBlobContainer));

// Register attachment provider
builder.Services.AddSingleton<IEmailAttachmentProvider>(sp => 
    new SqlAttachmentProvider(sqlConnectionString));

// Register email recorder
builder.Services.AddSingleton<IEmailRecorder>(sp => 
    new SqlRecorder(sqlConnectionString));

// Register email sender (using SendGrid implementation)
builder.Services.AddSingleton<IEmailSender>(sp => 
    new SendGridSender(
        sp.GetRequiredService<IEmailAttachmentProvider>(),
        new SendGridSenderConfiguration { ApiKey = sendGridApiKey },
        sp.GetRequiredService<IEmailRecorder>()));

var app = builder.Build();
app.Run();
