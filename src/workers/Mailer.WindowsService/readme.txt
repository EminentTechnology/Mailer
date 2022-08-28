The following files are excluded intentionally from being comitted into source control:

Configuration\db.config
Configuration\queues.config

Both files contain secrets.

The following are samples to show how to configure the files:

Configuration\db.config
--------------------------
<?xml version="1.0" encoding="utf-8" ?>
<connectionStrings>
  <add name="DefaultConnectionString"
	   connectionString="{enter actual database connection string}" 
	   providerName="System.Data.SqlClient"/>
</connectionStrings>

Configuration\queues.config - via Smtp
--------------------------
<?xml version="1.0" encoding="utf-8" ?>
<queues>
  <queue name="{unique_name}"
           
    queue="Mailer.Sql.SqlQueue"
    queueConnectionStringKey="DefaultConnectionString"
    queueReceiveBatchSize="1" <!-- How many emails to process -->
           
    recorder="Mailer.Recorders.Sql.SqlRecorder"
    recorderConnectionStringKey="DefaultConnectionString"
		   
    sender="Mailer.Smtp.SmtpSender"
    senderEnableSsl="true"
    senderHost="somehost.com"
    senderUserName="{username}"
    senderPassword="{password}"
    senderPickupDirectoryLocation="{optional}"
    senderPort="465"
    senderTargetName="{optional}"
    senderTimeout="{optional}"
    senderUseDefaultCredentials="false"
        
           
    attachmentProvider="Mailer.Attachments.Sql.SqlAttachmentProvider"
    attachmentConnectionStringKey="DefaultConnectionString"
    <!--Custom SQL used to retrieve an attachment for a given DocumentId-->
    attachmentSql="SELECT Id as DocumentId, StorageType, DocumentUrl, File FROM Document WHERE Id= @DocumentId"
           />
  
</queues>

Configuration\queues.config - via Sendgrid
--------------------------
<?xml version="1.0" encoding="utf-8" ?>
<queues>
  <queue name="{unique_name}"
           
         queue="Mailer.Sql.SqlQueue"
         queueConnectionStringKey="DefaultConnectionString"
         queueReceiveBatchSize="1" <!-- How many emails to process -->
           
         recorder="Mailer.Recorders.Sql.SqlRecorder"
         recorderConnectionStringKey="DefaultConnectionString"
		   
         sender="Mailer.SG.SendGridSender"
         senderUserName="{Sendgrid API Key}"
           
         attachmentProvider="Mailer.Attachments.Sql.SqlAttachmentProvider"
         attachmentConnectionStringKey="DefaultConnectionString"
         <!--Custom SQL used to retrieve an attachment for a given DocumentId-->
         attachmentSql="SELECT Id as DocumentId, StorageType, DocumentUrl, File FROM Document WHERE Id= @DocumentId"
           />
  
</queues>