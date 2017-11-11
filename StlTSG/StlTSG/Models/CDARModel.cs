namespace StlTSG.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class CDARModel : DbContext
    {
        public CDARModel()
            : base("name=CDARModel")
        {
        }

        public virtual DbSet<App> Apps { get; set; }
        public virtual DbSet<Appointment> Appointments { get; set; }
        public virtual DbSet<Client> Clients { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Error> Errors { get; set; }
        public virtual DbSet<StripeInfo> StripeInfoes { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<CDARModel>(null);
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<App>()
                .Property(e => e.Status)
                .IsUnicode(false);

            modelBuilder.Entity<App>()
                .Property(e => e.DiagnosticsTitle)
                .IsUnicode(false);

            modelBuilder.Entity<App>()
                .Property(e => e.AppointmentTitle)
                .IsUnicode(false);

            modelBuilder.Entity<Appointment>()
                .Property(e => e.Brand)
                .IsUnicode(false);

            modelBuilder.Entity<Appointment>()
                .Property(e => e.Model)
                .IsUnicode(false);

            modelBuilder.Entity<Appointment>()
                .Property(e => e.Amount)
                .HasPrecision(12, 2);

            modelBuilder.Entity<Client>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<Client>()
                .Property(e => e.Email)
                .IsUnicode(false);

            modelBuilder.Entity<Client>()
                .HasMany(e => e.Customers)
                .WithRequired(e => e.Client)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Customer>()
                .Property(e => e.Email)
                .IsUnicode(false);

            modelBuilder.Entity<Customer>()
                .Property(e => e.FirstName)
                .IsUnicode(false);

            modelBuilder.Entity<Customer>()
                .Property(e => e.LastName)
                .IsUnicode(false);

            modelBuilder.Entity<Customer>()
                .HasMany(e => e.Appointments)
                .WithRequired(e => e.Customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Error>()
                .Property(e => e.Value)
                .IsUnicode(false);

            modelBuilder.Entity<StripeInfo>()
               .Property(e => e.UserID)
               .IsUnicode(false);

            modelBuilder.Entity<StripeInfo>()
                .Property(e => e.CustomerID)
                .IsUnicode(false);

            modelBuilder.Entity<StripeInfo>()
                .HasMany(e => e.Clients)
                .WithOptional(e => e.StripeInfo)
                .HasForeignKey(e => e.StripeID);
        }
    }
}
