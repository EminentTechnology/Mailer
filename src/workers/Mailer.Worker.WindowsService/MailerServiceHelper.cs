using Mailer.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using System.Timers;


namespace Mailer.Worker.WindowsService
{
    public class MailerServiceHelper 
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Timer timer = new Timer();

        private readonly List<QueueHolder> QueueHolders;

        public MailerServiceHelper(List<QueueHolder> queueHolders)
        {
            
            QueueHolders = queueHolders;


            timer.Enabled = false;
            timer.AutoReset = false;
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
        }

       
        

        

        public void OnStart(string[] args)
        {
            int interval = 60000; //every minute

            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["POLLING_INTERVAL_SECONDS"]))
            {
                try
                {
                    interval = Convert.ToInt32(ConfigurationManager.AppSettings["POLLING_INTERVAL_SECONDS"]) * 1000;
                    timer.Interval = interval;
                }
                catch { }
            }

            log.Info($"Checking for outbound email messages every {interval} ms");

            ProcessEmailMessages();

        }

        public void OnStop()
        {
            log.Info("stopped");
            timer.Stop();

        }

       public void OnContinue()
        {
            log.Info("continuing - checking for outbound email messages");

            ProcessEmailMessages();

        }

        public void OnPause()
        {
            log.Info("pausing");
            timer.Stop();
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ProcessEmailMessages();
        }

        private void ProcessEmailMessages()
        {
            try
            {
                log.Debug("Checking for outbound email messages");

                foreach (var queue in QueueHolders)
                {
                    try
                    {
                        SendMessagesInQueue(queue);
                    }
                    catch (Exception ex)
                    {
                        log.Error($"Error encountered while processing queue - {queue.Config.Name}", ex);
                    }

                }


            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            finally
            {
                timer.Start();
            }

        }



        private async void SendMessagesInQueue(QueueHolder holder)
        {
            log.Debug($"{holder.Config.Name} queue batch size is set to {holder.Config.QueueReceiveBatchSize} emails");

            if (holder.Config.QueueReceiveBatchSize > 1)
            {
                List<EmailMessage> messages = await holder.Queue.GetMessages(holder.Config.QueueReceiveBatchSize);

                log.Debug($"{messages.Count} emails received from {holder.Config.Name} queue");


                foreach (var message in messages)
                {
                    var messageId = message.Id;

                    try
                    {
                        log.Debug($"Sending message {messageId}");
                        await holder.Sender.Send(message);
                        log.Debug($"Recorded message {messageId} - {message.Id}");
                    }
                    catch (Exception ex)
                    {
                        log.Error($"Error processing message {messageId} ({message.Id}) in {holder.Config.Name} queue", ex);
                        await holder.Recorder.RecordFailure(message, ex);
                    }
                }
            }
            else
            {
                EmailMessage message = await holder.Queue.GetMessage();

                if (message != null)
                {
                    log.Debug($"1 email received from  {holder.Config.Name} queue");

                    var messageId = message.Id;

                    try
                    {
                        log.Debug($"Sending message {messageId}");
                        await holder.Sender.Send(message);
                        log.Debug($"Recorded message {messageId} - {message.Id}");
                    }
                    catch (Exception ex)
                    {
                        log.Error($"Error processing message {message.Id} in {holder.Config.Name} queue", ex);
                        await holder.Recorder.RecordFailure(message, ex);
                    }
                }
                else
                {
                    log.Debug($"0 emails received from  {holder.Config.Name} queue");
                }

            }

        }

    }
}
