using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mailer.Attachments.Sql
{
    public class AttachmentContext: DbContext
    {
        public AttachmentContext() : base("name=ConnectionString")
        {
            Database.SetInitializer<AttachmentContext>(null);
        }

        public AttachmentContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
            Database.SetInitializer<AttachmentContext>(null);
        }

        public DbSet<Document> Documents { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

     

            modelBuilder.Entity<Document>()
                .Property(p => p.File).HasColumnType("image");


           
        }
    }
}
