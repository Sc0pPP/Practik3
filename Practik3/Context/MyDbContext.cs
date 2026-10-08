using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Practik3.Entities;

namespace Practik3.Context;

public partial class MyDbContext : DbContext
{
    public MyDbContext()
    {
    }

    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cloth> Cloths { get; set; }

    public virtual DbSet<ClothSize> ClothSizes { get; set; }

    public virtual DbSet<ClothType> ClothTypes { get; set; }

    public virtual DbSet<Clothorder> Clothorders { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Size> Sizes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserType> UserTypes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cloth>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cloth_pk");

            entity.ToTable("cloth");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();

            entity.HasOne(d => d.Type).WithMany(p => p.Cloths)
                .HasForeignKey(d => d.TypeId)
                .HasConstraintName("cloth_TypeId_fkey");
        });

        modelBuilder.Entity<ClothSize>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("clothsize_pk");

            entity.ToTable("ClothSize");

            entity.HasOne(d => d.Cloth).WithMany(p => p.ClothSizes)
                .HasForeignKey(d => d.ClothId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("ClothSize_ClothId_fkey");

            entity.HasOne(d => d.Size).WithMany(p => p.ClothSizes)
                .HasForeignKey(d => d.SizeId)
                .HasConstraintName("ClothSize_SizeId_fkey");
        });

        modelBuilder.Entity<ClothType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ClothType_pkey");

            entity.ToTable("ClothType");

            entity.HasIndex(e => e.Name, "ClothType_Name_key").IsUnique();
        });

        modelBuilder.Entity<Clothorder>(entity =>
        {
            entity.HasKey(e => e.ClothOrderId).HasName("clothorder_pk");

            entity.ToTable("clothorder");

            entity.Property(e => e.ClothOrderId).HasDefaultValueSql("nextval('clothorder_clothorderid_seq'::regclass)");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");

            entity.HasOne(d => d.Cloth).WithMany(p => p.Clothorders)
                .HasForeignKey(d => d.ClothId)
                .HasConstraintName("clothorder_ClothId_fkey");

            entity.HasOne(d => d.Order).WithMany(p => p.Clothorders)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("clothorder_OrderID_fkey");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("order_pk");

            entity.ToTable("Order");

            entity.Property(e => e.Status).HasDefaultValueSql("'Собирается'::text");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Order_UserId_fkey");
        });

        modelBuilder.Entity<Size>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("size_pk");

            entity.ToTable("Size");

            entity.Property(e => e.Id).HasDefaultValueSql("nextval('\"clothsize_Id_seq\"'::regclass)");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Users_pkey");

            entity.HasIndex(e => e.Login, "Users_Login_key").IsUnique();

            entity.HasOne(d => d.UserType).WithMany(p => p.Users)
                .HasForeignKey(d => d.UserTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Users_UserTypeId_fkey");
        });

        modelBuilder.Entity<UserType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("UserType_pkey");

            entity.ToTable("UserType");

            entity.HasIndex(e => e.Name, "UserType_Name_key").IsUnique();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
