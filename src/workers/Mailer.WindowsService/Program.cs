
using log4net;
using Mailer.Attachments.Sql;
using Mailer.Recorders.Sql;
using Mailer.Smtp;
using Mailer.Sql;
using Mailer.Worker.WindowsService;
using ServiceHelper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mailer.WindowsService
{
    static class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            try
            {
                if (args.Length > 0 && args[0].ToLower().Equals("/debug"))
                {
                    Application.Run(new ServiceRunner(new MailerService()));
                }
                else
                {
                    ServiceBase[] ServicesToRun;
                    ServicesToRun = new ServiceBase[]
                    {
                    new MailerService()
                    };
                    ServiceBase.Run(ServicesToRun);
                }
            }
            catch (Exception ex)
            {
                log.Error("Unhandled service error", ex);
            }

            
        }

        

        

    }
}
