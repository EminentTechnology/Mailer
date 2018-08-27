using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mailer.Worker.WindowsService
{
    public class QueueConfigurationElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new QueueConfigurationElement();
        }
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((QueueConfigurationElement)element).Name;
        }
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }
        protected override string ElementName
        {
            get
            {
                return "queue";
            }
        }
    }
}
