using System;

using Mailer.Abstractions;
using Mailer.Recorders.Sql;
using System.Threading.Tasks;

using System.Text;
using NUnit.Framework;

namespace Mailer.Tests
{
    [TestFixture]
    public class RecorderTests
    {
        [Test]
        [Ignore("Integration")]
        public async Task Recorder_AddingNewMessage_ShouldInsertIntoMailTables()
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
    }

    
}
