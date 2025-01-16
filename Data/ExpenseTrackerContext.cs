using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace expense_tracker.Data;

public partial class ExpenseTrackerContext : DbContext
{
    public ExpenseTrackerContext()
    {
    }

    public ExpenseTrackerContext(DbContextOptions<ExpenseTrackerContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AppUser> AppUsers { get; set; }

    public virtual DbSet<AuthLog> AuthLogs { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("app_user_pkey");

            entity.ToTable("app_user");

            entity.HasIndex(e => e.Username, "app_user_username_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasColumnName("first_name");
            entity.Property(e => e.HashKey)
                .HasMaxLength(512)
                .HasColumnName("hash_key");
            entity.Property(e => e.HashPassword)
                .HasMaxLength(512)
                .HasColumnName("hash_password");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .HasColumnName("last_name");
            entity.Property(e => e.Username)
                .HasMaxLength(20)
                .HasColumnName("username");
        });

        modelBuilder.Entity<AuthLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("auth_logs_pkey");

            entity.ToTable("auth_logs");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Date)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("date");
            entity.Property(e => e.Type)
                .HasMaxLength(20)
                .HasColumnName("type");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.AuthLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("auth_logs_user_id_fkey");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("transaction_pkey");

            entity.ToTable("transaction");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasDefaultValueSql("400")
                .HasColumnName("amount");
            entity.Property(e => e.CardNumber)
                .HasMaxLength(19)
                .HasColumnName("card_number");
            entity.Property(e => e.Date)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("date");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Marked)
                .HasDefaultValue(false)
                .HasColumnName("marked");
            entity.Property(e => e.Purpose)
                .HasMaxLength(100)
                .HasColumnName("purpose");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_user_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
