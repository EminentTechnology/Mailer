using Mailer.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Mailer.Smtp
{
    

    public class SmtpSender : IEmailSender
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        readonly IEmailAttachmentProvider attachmentProvider;
        readonly SmtpSenderConfiguration config;
        readonly IEmailRecorder recorder;

        public SmtpSender(IEmailAttachmentProvider emailAttachmentProvider, SmtpSenderConfiguration configuration, IEmailRecorder emailRecorder)
        {
            config = configuration;
            attachmentProvider = emailAttachmentProvider;
            recorder = emailRecorder;
        }

        

        public async Task Send(EmailMessage message)
        {
            try
            {
                //send message
                MailMessage mail = await GetMailMessage(message);

                using (SmtpClient client = GetMailClient())
                {
                    await client.SendMailAsync(mail);
                }

                //record success
                if (recorder != null)
                {
                    await recorder.RecordSuccess(message);
                }
            }
            catch (Exception ex)
            {
                //record failure
                if (recorder != null)
                {
                    await recorder.RecordFailure(message, ex);

                    log.Error($"Error encountered trying to send message {message.Id} - {ex.Message}", ex);
                }
            }
            
        }

        private async Task<MailMessage> GetMailMessage(EmailMessage msg)
        {
            MailMessage message = new MailMessage();

            message.Subject = msg.Subject;
            message.Body = msg.Body;
            message.Priority = (System.Net.Mail.MailPriority)Enum.Parse(typeof(Abstractions.MailPriority), msg.Priority.ToString(), true);
            message.IsBodyHtml = msg.IsBodyHtml;

            //from address

            
            if (msg.From != null)
            {
                MailAddress from = null;
                if (String.IsNullOrEmpty(msg.From.DisplayName))
                {
                    from = new MailAddress(msg.From.Address);
                }
                else
                {
                    from = new MailAddress(
                        msg.From.Address,
                        msg.From.DisplayName
                        );
                }
                message.From = from;

            }

            //to address
            MailAddress address = null;
            if (msg.To.Count > 0)
            {
                foreach (var item in msg.To)
                {
                    address = null;
                    if (String.IsNullOrEmpty(item.DisplayName))
                    {
                        address = new MailAddress(item.Address);
                    }
                    else
                    {
                        address = new MailAddress(
                            item.Address,
                            item.DisplayName
                            );
                    }
                    message.To.Add(address);
                }
            }

            //replyto address
            if (msg.ReplyTo.Count > 0)
            {
                foreach (var item in msg.ReplyTo)
                {
                    address = null;
                    if (String.IsNullOrEmpty(item.DisplayName))
                    {
                        address = new MailAddress(item.Address);
                    }
                    else
                    {
                        address = new MailAddress(
                            item.Address,
                            item.DisplayName
                            );
                    }
                    message.ReplyToList.Add(address);
                }
            }

            //cc address
            if (msg.CC.Count > 0)
            {
                foreach (var item in msg.CC)
                {
                    address = null;
                    if (String.IsNullOrEmpty(item.DisplayName))
                    {
                        address = new MailAddress(item.Address);
                    }
                    else
                    {
                        address = new MailAddress(
                            item.Address,
                            item.DisplayName
                            );
                    }
                    message.CC.Add(address);
                }
            }



            //bcc address
            if (msg.BCC.Count > 0)
            {
                foreach (var item in msg.BCC)
                {
                    address = null;
                    if (String.IsNullOrEmpty(item.DisplayName))
                    {
                        address = new MailAddress(item.Address);
                    }
                    else
                    {
                        address = new MailAddress(
                            item.Address,
                            item.DisplayName
                            );
                    }

                    message.Bcc.Add(address);
                }
            }

            //attachments
            if (msg.Attachments.Count > 0)
            {
                foreach (var item in msg.Attachments)
                {
                    byte[] btDoc = await attachmentProvider.GetAttachmentSource(item.AttachmentId);

                    MemoryStream s = new MemoryStream(btDoc);
                    StreamWriter writer = new StreamWriter(s);
                    writer.Flush();
                    s.Position = 0;

                    Attachment attachment = new Attachment(s, item.FileName, item.ContentType);
                    message.Attachments.Add(attachment);
                }
            }
            

            return message;
        }

        private SmtpClient GetMailClient()
        {
            SmtpClient client = new SmtpClient();

            if (client.DeliveryMethod == SmtpDeliveryMethod.Network)
            {

                client.Host = config.Host;
                client.Port = config.Port;
            }

            if (client.DeliveryMethod == SmtpDeliveryMethod.SpecifiedPickupDirectory)
            {
                client.PickupDirectoryLocation = config.PickupDirectoryLocation;
            }

            client.EnableSsl = config.EnableSsl;
            client.Timeout = config.Timeout;
            client.TargetName = config.TargetName;
            client.UseDefaultCredentials = config.UseDefaultCredentials;

            if ((!config.UseDefaultCredentials) && (!String.IsNullOrEmpty(config.UserName)))
            {
                NetworkCredential cred = new NetworkCredential(config.UserName, config.Password);
                client.Credentials = cred;
            }

            return client;

        }
    }
}
