using DatabaseMastery.HotCoffeePostgreSQL.Domain;
using DatabaseMastery.HotCoffeePostgreSQL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DatabaseMastery.HotCoffeePostgreSQL.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany()
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            var reservationStatusConverter = new EnumToStringConverter<ReservationStatus>();

            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.Property(r => r.ReservationDate)
                    .HasColumnType("date");

                entity.Property(r => r.ReservationTime)
                    .HasColumnType("time without time zone");

                entity.Property(r => r.Status)
                    .HasConversion(reservationStatusConverter)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_Reservations_Status",
                    "\"Status\" IN ('Pending', 'Confirmed', 'Cancelled')"));
            });
        }
    }
}
