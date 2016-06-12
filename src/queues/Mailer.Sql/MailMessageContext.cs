using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mailer.Sql
{
    public class MailMessageContext: DbContext
    {
        public MailMessageContext() : base("name=ConnectionString")
        {
            Database.SetInitializer<MailMessageContext>(null);
        }

        public MailMessageContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
            Database.SetInitializer<MailMessageContext>(null);
        }

        public DbSet<MailMessageQueue> Messages { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Entity<MailMessageQueue>()
                .Property(p => p.Payload).HasColumnType("varchar(max)");

        }
    }
}
