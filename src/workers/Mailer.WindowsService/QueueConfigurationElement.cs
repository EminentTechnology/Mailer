using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mailer.WindowsService
{
    public class QueueConfigurationElement : ConfigurationElement
    {
 

        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get
            {
                return this["name"].ToString();
            }
            set
            {
                this["name"] = value;
            }
        }

        [ConfigurationProperty("queue", IsRequired = true)]
        public string Queue
        {
            get
            {
                return this["queue"].ToString();
            }
            set
            {
                this["queue"] = value;
            }
        }

        [ConfigurationProperty("queueReceiveBatchSize", IsRequired = false, DefaultValue=100)]
        public int QueueReceiveBatchSize
        {
            get
            {
                return (int)this["queueReceiveBatchSize"];
            }
            set
            {
                this["queueReceiveBatchSize"] = value;
            }
        }

        [ConfigurationProperty("sender", IsRequired = true)]
        public string Sender
        {
            get
            {
                return this["sender"].ToString();
            }
            set
            {
                this["sender"] = value;
            }
        }


        [ConfigurationProperty("recorder", IsRequired = false)]
        public string Recorder
        {
            get
            {
                return this["recorder"]?.ToString();
            }
            set
            {
                this["recorder"] = value;
            }
        }


        [ConfigurationProperty("attachmentProvider", IsRequired = false)]
        public string AttachmentProvider
        {
            get
            {
                return this["attachmentProvider"]?.ToString();
            }
            set
            {
                this["attachmentProvider"] = value;
            }
        }

        [ConfigurationProperty("attachmentConnectionStringKey", IsRequired = false)]
        public string AttachmentConnectionStringKey
        {
            get
            {
                return this["attachmentConnectionStringKey"]?.ToString();
            }
            set
            {
                this["attachmentConnectionStringKey"] = value;
            }
        }


        [ConfigurationProperty("queueConnectionStringKey", IsRequired = false)]
        public string QueueConnectionStringKey
        {
            get
            {
                return this["queueConnectionStringKey"]?.ToString();
            }
            set
            {
                this["queueConnectionStringKey"] = value;
            }
        }

        [ConfigurationProperty("recorderConnectionStringKey", IsRequired = false)]
        public string RecorderConnectionStringKey
        {
            get
            {
                return this["recorderConnectionStringKey"]?.ToString();
            }
            set
            {
                this["recorderConnectionStringKey"] = value;
            }
        }

        [ConfigurationProperty("senderDeliveryMethod", IsRequired = false)]
        public System.Net.Mail.SmtpDeliveryMethod DeliveryMethod
        {
            get
            {
                if (this["senderDeliveryMethod"] == null)
                    return System.Net.Mail.SmtpDeliveryMethod.Network;

                return (System.Net.Mail.SmtpDeliveryMethod)this["senderDeliveryMethod"];
            }
            set
            {
                this["senderDeliveryMethod"] = value;
            }
        }

        [ConfigurationProperty("senderUserName", IsRequired = false)]
        public string SenderUserName
        {
            get
            {
                return this["senderUserName"]?.ToString();
            }
            set
            {
                this["senderUserName"] = value;
            }
        }

        [ConfigurationProperty("senderPassword", IsRequired = false)]
        public string SenderPassword
        {
            get
            {
                return this["senderPassword"]?.ToString();
            }
            set
            {
                this["senderPassword"] = value;
            }
        }


        [ConfigurationProperty("senderUseDefaultCredentials", IsRequired = false, DefaultValue=true)]
        public bool SenderUseDefaultCredentials
        {
            get
            {


                return (bool)this["senderUseDefaultCredentials"];
            }
            set
            {
                this["senderUseDefaultCredentials"] = value;
            }
        }

        [ConfigurationProperty("senderEnableSsl", IsRequired = false)]
        public bool SenderEnableSsl
        {
            get
            {
                return (bool)this["senderEnableSsl"];
            }
            set
            {
                this["senderEnableSsl"] = value;
            }
        }

        [ConfigurationProperty("senderHost", IsRequired = false)]
        public string SenderHost
        {
            get
            {
                return this["senderHost"]?.ToString();
            }
            set
            {
                this["senderHost"] = value;
            }
        }

        [ConfigurationProperty("senderPort", IsRequired = false, DefaultValue = 587)]
        public int SenderPort
        {
            get
            {
                return (int)this["senderPort"];
            }
            set
            {
                this["senderPort"] = value;
            }
        }

        [ConfigurationProperty("senderPickupDirectoryLocation", IsRequired = false)]
        public string SenderPickupDirectoryLocation
        {
            get
            {
                return this["senderPickupDirectoryLocation"]?.ToString();
            }
            set
            {
                this["senderPickupDirectoryLocation"] = value;
            }
        }

        [ConfigurationProperty("senderTimeout", IsRequired = false, DefaultValue=100000)]
        public int SenderTimeout
        {
            get
            {
                return (int)this["senderTimeout"];
            }
            set
            {
                this["senderTimeout"] = value;
            }
        }

        [ConfigurationProperty("senderTargetName", IsRequired = false)]
        public string SenderTargetName
        {
            get
            {
                return this["senderTargetName"]?.ToString();
            }
            set
            {
                this["senderTargetName"] = value;
            }
        }
    }
}
