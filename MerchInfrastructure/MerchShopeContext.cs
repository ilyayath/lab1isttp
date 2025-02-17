using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MerchDomain.Model;

//namespace MerchDomain.Model;
namespace MerchInfrastructure;
public partial class MerchShopeContext : DbContext
{
    public MerchShopeContext()
    {
    }

    public MerchShopeContext(DbContextOptions<MerchShopeContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<Buyer> Buyers { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<MerchOrder> MerchOrders { get; set; }

    public virtual DbSet<Merchandise> Merchandises { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<OrderStatus> OrderStatuses { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Shipment> Shipments { get; set; }

    public virtual DbSet<Size> Sizes { get; set; }

    public virtual DbSet<Team> Teams { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=merch_shope;Username=postgres;Password=googlemaybeop314;Pooling=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("brands_pkey");

            entity.Property(e => e.Id).HasDefaultValueSql("nextval('brands_id_seq'::regclass)");
            entity.Property(e => e.BrandName).HasMaxLength(255);
        });

        modelBuilder.Entity<Buyer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("buyers_pkey");

            entity.HasIndex(e => e.Username, "buyers_username_key").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("nextval('buyers_id_seq'::regclass)");
            entity.Property(e => e.Address).HasColumnType("character varying");
            entity.Property(e => e.Username).HasMaxLength(255);

            entity.HasOne(d => d.City).WithMany(p => p.Buyers)
                .HasForeignKey(d => d.CityId)
                .HasConstraintName("fr_city");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("categories_pkey");

            entity.Property(e => e.Id).HasDefaultValueSql("nextval('categories_id_seq'::regclass)");
            entity.Property(e => e.CategoryName).HasMaxLength(255);
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Cities_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CityName).HasMaxLength(255);
        });

        modelBuilder.Entity<MerchOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("merchorders_pkey");

            entity.Property(e => e.Id).HasDefaultValueSql("nextval('merchorders_id_seq'::regclass)");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.Buyer).WithMany(p => p.MerchOrders)
                .HasForeignKey(d => d.BuyerId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("merchorders_buyerid_fkey");

            entity.HasOne(d => d.Payment).WithMany(p => p.MerchOrders)
                .HasForeignKey(d => d.PaymentId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("merchorders_paymentid_fkey");

            entity.HasOne(d => d.Shipment).WithMany(p => p.MerchOrders)
                .HasForeignKey(d => d.ShipmentId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("merchorders_shipmentid_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.MerchOrders)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("merchorders_statusid_fkey");
        });

        modelBuilder.Entity<Merchandise>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("merchandises_pkey");

            entity.Property(e => e.Id).HasDefaultValueSql("nextval('merchandises_id_seq'::regclass)");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Price).HasPrecision(10, 2);

            entity.HasOne(d => d.Brand).WithMany(p => p.Merchandises)
                .HasForeignKey(d => d.BrandId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("merchandises_brandid_fkey");

            entity.HasOne(d => d.Category).WithMany(p => p.Merchandises)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("merchandises_categoryid_fkey");

            entity.HasOne(d => d.Size).WithMany(p => p.Merchandises)
                .HasForeignKey(d => d.SizeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("merchandises_sizeid_fkey");

            entity.HasOne(d => d.Team).WithMany(p => p.Merchandises)
                .HasForeignKey(d => d.TeamId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("merchandises_teamid_fkey");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => new { e.OrderId, e.MerchId }).HasName("orderitems_pkey");

            entity.HasOne(d => d.Merch).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.MerchId)
                .HasConstraintName("orderitems_merchid_fkey");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("orderitems_orderid_fkey");
        });

        modelBuilder.Entity<OrderStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("orderstatus_pkey");

            entity.Property(e => e.Id).HasDefaultValueSql("nextval('orderstatus_id_seq'::regclass)");
            entity.Property(e => e.StatusName).HasMaxLength(255);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("payments_pkey");

            entity.Property(e => e.Id).HasDefaultValueSql("nextval('payments_id_seq'::regclass)");
            entity.Property(e => e.TypePayment).HasMaxLength(255);
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Reviews_pkey");

            entity.Property(e => e.ReviewDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.Buyer).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.BuyerId)
                .HasConstraintName("reviews_buyerid_fkey");

            entity.HasOne(d => d.Merchandise).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.MerchandiseId)
                .HasConstraintName("reviews_merchandiseid_fkey");
        });

        modelBuilder.Entity<Shipment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("shipments_pkey");

            entity.Property(e => e.Id).HasDefaultValueSql("nextval('shipments_id_seq'::regclass)");
            entity.Property(e => e.TypeShipment).HasMaxLength(255);
        });

        modelBuilder.Entity<Size>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sizes_pkey");

            entity.Property(e => e.Id).HasDefaultValueSql("nextval('sizes_id_seq'::regclass)");
            entity.Property(e => e.SizeName).HasMaxLength(50);
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("teams_pkey");

            entity.Property(e => e.Id).HasDefaultValueSql("nextval('teams_id_seq'::regclass)");
            entity.Property(e => e.TeamName).HasMaxLength(255);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
