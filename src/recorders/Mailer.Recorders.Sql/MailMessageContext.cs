using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mailer.Recorders.Sql
{
    public class MailMessageContext : DbContext
    {
        public MailMessageContext() : base("name=ConnectionString")
        {
            Database.SetInitializer<MailMessageContext>(null);
        }

        public MailMessageContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
            Database.SetInitializer<MailMessageContext>(null);
        }

        public DbSet<MailMessage> Messages { get; set; }
        public DbSet<MailMessageAddress> MessageMessageAddresses { get; set; }
        public DbSet<MailMessageAttachment> MessageAttachments { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            //modelBuilder.Entity<MailMessage>()
            //    .Property(p => p.MailMessageID).HasColumnType("uniqueidentifier");

            modelBuilder.Entity<MailMessage>()
                .Property(p => p.MailBody).HasColumnType("varchar(max)");

            //modelBuilder.Entity<MailMessageAddress>()
            //   .Property(p => p.MailMessageAddressID).HasColumnType("uniqueidentifier");

            //modelBuilder.Entity<MailMessageAddress>()
            //   .Property(p => p.MailMessageID).HasColumnType("uniqueidentifier");

            //modelBuilder.Entity<MailMessageAttachment>()
            //   .Property(p => p.MailMessageAttachmentID).HasColumnType("uniqueidentifier");

            //modelBuilder.Entity<MailMessageAttachment>()
            //   .Property(p => p.MailMessageID).HasColumnType("uniqueidentifier");

            //modelBuilder.Entity<MailMessageAttachment>()
            //   .Property(p => p.DocumentID).HasColumnType("uniqueidentifier");

        }
    }
}
