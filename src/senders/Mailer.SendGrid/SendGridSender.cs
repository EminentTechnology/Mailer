using Mailer.Abstractions;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Mailer.SG
{
    public class SendGridSender : IEmailSender
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        readonly IEmailAttachmentProvider attachmentProvider;
        readonly SendGridSenderConfiguration config;
        readonly IEmailRecorder recorder;

        readonly SendGridClient client;

        public SendGridSender(IEmailAttachmentProvider emailAttachmentProvider, SendGridSenderConfiguration configuration, IEmailRecorder emailRecorder)
        {
            config = configuration;
            attachmentProvider = emailAttachmentProvider;
            recorder = emailRecorder;

            client = new SendGridClient(configuration.ApiKey);
        }

        public async Task Send(EmailMessage message)
        {
            try
            {
                SendGrid.Helpers.Mail.EmailAddress from = GetAddress(message.From);
                List<SendGrid.Helpers.Mail.EmailAddress> tos = GetAddresses(message.To);

                var sgMessage = MailHelper.CreateSingleEmailToMultipleRecipients(from, tos, message.Subject, message.Body, message.Body);

                //replyto address
                if (message.ReplyTo.Count > 0)
                {
                    //SendGrid only supports 1 reply to
                    var replyTo = GetAddress(message.ReplyTo[0]);
                    sgMessage.SetReplyTo(replyTo);
                }

                //cc address
                if (message.CC.Count > 0)
                {
                    var ccs = GetAddresses(message.CC);
                    sgMessage.AddCcs(ccs);
                }

                //bcc address
                if (message.BCC.Count > 0)
                {
                    var bccs = GetAddresses(message.BCC);
                    sgMessage.AddBccs(bccs);
                }


                //attachments
                if (message.Attachments.Count > 0)
                {

                    foreach (var item in message.Attachments)
                    {
                        byte[] btDoc = await attachmentProvider.GetAttachmentSource(item.AttachmentId);

                        MemoryStream s = new MemoryStream(btDoc);
                        StreamWriter writer = new StreamWriter(s);
                        writer.Flush();
                        s.Position = 0;

                        await sgMessage.AddAttachmentAsync(item.FileName, s, item.ContentType, item.Disposition, item.ContentId);
                    }
                }

                var response = await client.SendEmailAsync(sgMessage);
                log.Debug($"SendGrid http response for {message.Id}: { response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    //record success
                    if (recorder != null)
                    {
                        await recorder.RecordSuccess(message);
                    }
                }
                else
                {
                    var body = await response.DeserializeResponseBodyAsync(response.Body);
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(body);
                    log.Debug(json);

                    //record failure - sendgrid error
                    if (recorder != null)
                    {
                        var ex = new Exception(json);
                        await recorder.RecordFailure(message, ex);
                    }
                }

            }
            catch (Exception ex)
            {
                //record failure - exception in code
                if (recorder != null)
                {
                    await recorder.RecordFailure(message, ex);

                    log.Error($"Error encountered trying to send message {message.Id} - {ex.Message}", ex);
                }
            }
        }

        private SendGrid.Helpers.Mail.EmailAddress GetAddress(Abstractions.EmailAddress contact)
        {
            SendGrid.Helpers.Mail.EmailAddress address = null;

            if (contact != null)
            {
                if (String.IsNullOrEmpty(contact.DisplayName))
                {
                    address = new SendGrid.Helpers.Mail.EmailAddress(contact.Address);
                }
                else
                {
                    address = new SendGrid.Helpers.Mail.EmailAddress(
                        contact.Address,
                        contact.DisplayName
                    );
                }
            }

            return address;
        }

        

        private List<SendGrid.Helpers.Mail.EmailAddress> GetAddresses(List<Abstractions.EmailAddress> contacts)
        {
            List<SendGrid.Helpers.Mail.EmailAddress> addresses = new List<SendGrid.Helpers.Mail.EmailAddress>();

            if ((contacts != null) && (contacts.Count > 0))
            {
                foreach (var contact in contacts)
                {
                    var address = new SendGrid.Helpers.Mail.EmailAddress(contact.Address, contact.DisplayName);
                    addresses.Add(address);
                }
            }

            return addresses;
        }
    }
}
