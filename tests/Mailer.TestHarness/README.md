# Mailer Implementation Test Harness

A simple console application for testing the different email queue implementations in the Mailer project.

## Overview

This test harness allows you to:

1. Test Azure Storage Queue implementation
2. Test SQL Queue implementation
3. Create test emails with attachments
4. Manage configuration securely using .NET User Secrets

## Setup Instructions

### Configuration

This application uses .NET User Secrets to securely store configuration values outside of source control.

```powershell
# Initialize user secrets (only needed once)
dotnet user-secrets init --project tests\Mailer.TestHarness

# Set Azure Storage connection string
dotnet user-secrets set "AzureStorageConnectionString" "your-connection-string" --project tests\Mailer.TestHarness

# Set SQL connection string
dotnet user-secrets set "SqlConnectionString" "your-sql-connection-string" --project tests\Mailer.TestHarness

# Set SendGrid API key (optional for queue testing)
dotnet user-secrets set "SendGridApiKey" "your-sendgrid-api-key" --project tests\Mailer.TestHarness
```

### Running the Test Harness

```powershell
# Navigate to the test harness directory
cd tests\Mailer.TestHarness

# Build and run the application
dotnet run
```

## Features

### Azure Storage Queue Testing

Tests the Azure Storage Queue implementation by:
- Creating a test email message
- Queueing it to Azure Storage Queue
- Optionally retrieving it from the queue

This will help verify:
- Connectivity to Azure Storage
- Serialization/deserialization of messages
- Storage of large messages in blob storage

### SQL Queue Testing

Tests the SQL Queue implementation by:
- Creating a test email message
- Queueing it to SQL database
- Optionally retrieving it from the queue

### Email with Attachment Testing

Tests creating an email with an attachment and queueing it using either implementation.

## Notes

- All configuration values are stored securely using .NET User Secrets
- No sensitive information is committed to source control
- Configuration values persist between runs
- For Azure Functions testing, see the Azure Functions project documentation
