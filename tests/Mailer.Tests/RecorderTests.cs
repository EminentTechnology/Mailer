using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mailer.Abstractions;
using Mailer.Recorders.Sql;
using System.Threading.Tasks;
using System.Data.Entity.Validation;
using System.Text;

namespace Mailer.Tests
{
    [TestClass]
    public class RecorderTests
    {
        [TestMethod]
        public async Task Recorder_AddingNewMessage_ShouldInsertIntoMailTables()
        {
            
            try
            {
                EmailMessage message = new EmailMessage("support@eminenttechnology.com", "pochu@eminenttechnology.com", "subject", "body");
                message.Id = Guid.NewGuid().ToString();

                IEmailRecorder recorder = new SqlRecorder("name=ontrack");
               // await recorder.RecordMessage(message);

                message.Id = Guid.NewGuid().ToString();
                await recorder.RecordFailure(message, new ApplicationException("generic error"));

                message.Id = Guid.NewGuid().ToString();
                await recorder.RecordSuccess(message);

            }
            catch (DbEntityValidationException e)
            {
                var newException = new FormattedDbEntityValidationException(e);
                throw newException;
            }

        }
    }

    public class FormattedDbEntityValidationException : Exception
    {
        public FormattedDbEntityValidationException(DbEntityValidationException innerException) :
            base(null, innerException)
        {
        }

        public override string Message
        {
            get
            {
                var innerException = InnerException as DbEntityValidationException;
                if (innerException != null)
                {
                    StringBuilder sb = new StringBuilder();

                    sb.AppendLine();
                    sb.AppendLine();
                    foreach (var eve in innerException.EntityValidationErrors)
                    {
                        sb.AppendLine(string.Format("- Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().FullName, eve.Entry.State));
                        foreach (var ve in eve.ValidationErrors)
                        {
                            sb.AppendLine(string.Format("-- Property: \"{0}\", Value: \"{1}\", Error: \"{2}\"",
                                ve.PropertyName,
                                eve.Entry.CurrentValues.GetValue<object>(ve.PropertyName),
                                ve.ErrorMessage));
                        }
                    }
                    sb.AppendLine();

                    return sb.ToString();
                }

                return base.Message;
            }
        }
    }
}
