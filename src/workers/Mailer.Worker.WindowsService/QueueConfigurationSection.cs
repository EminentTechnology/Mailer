using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mailer.Worker.WindowsService
{
    public class QueueConfigurationSection : ConfigurationSection
    {
        public QueueConfigurationSection()
        {

        }

        [ConfigurationProperty("", IsDefaultCollection = true)]
        public QueueConfigurationElementCollection Queues
        {
            get
            {
                return (QueueConfigurationElementCollection)base[""];
            }
        }
    }
}
