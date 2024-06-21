using Microsoft.EntityFrameworkCore;

namespace NutriDbService.Models;

public partial class RailwayContext : DbContext
{
    public RailwayContext()
    {
    }

    public RailwayContext(DbContextOptions<RailwayContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Loyalty> Loyalties { get; set; }

    public virtual DbSet<Messagelog> Messagelogs { get; set; }

    public virtual DbSet<Promo> Promos { get; set; }

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Userinfo> Userinfos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Server=monorail.proxy.rlwy.net;Port=55087;Database=railway;user id=postgres;password=FuuRrlYXtXIADfHrjbzymzuLcOFhKOdu");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Loyalty>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("loyalty_pkey");

            entity.ToTable("loyalty");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Balance)
                .HasPrecision(10, 1)
                .HasColumnName("balance");
            entity.Property(e => e.CurrentCount).HasColumnName("currentCount");
            entity.Property(e => e.TargetCount).HasColumnName("targetCount");
            entity.Property(e => e.UserId).HasColumnName("userId");

            entity.HasOne(d => d.User).WithMany(p => p.Loyalties)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("loyal_to_user");
        });

        modelBuilder.Entity<Messagelog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("messagelog_pkey");

            entity.ToTable("messagelog");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('log_id_auto_inc'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.BotMessage).HasColumnName("botMessage");
            entity.Property(e => e.Step).HasColumnName("step");
            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.UserMessage).HasColumnName("userMessage");

            entity.HasOne(d => d.User).WithMany(p => p.Messagelogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("log_to_user");
        });

        modelBuilder.Entity<Promo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("promo_pkey");

            entity.ToTable("promo");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('promo_id_auto_inc'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Discount).HasColumnName("discount");
            entity.Property(e => e.Freeperiod).HasColumnName("freeperiod");
            entity.Property(e => e.PromoCode)
                .HasMaxLength(255)
                .HasColumnName("promoCode");
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("subscription_pkey");

            entity.ToTable("subscription");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('sub_id_auto_inc'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(255)
                .HasColumnName("paymentMethod");
            entity.Property(e => e.PromoId).HasColumnName("promoId");
            entity.Property(e => e.SubscriptionEndDate).HasColumnName("subscriptionEndDate");
            entity.Property(e => e.SubscriptionStartDate).HasColumnName("subscriptionStartDate");
            entity.Property(e => e.UserId).HasColumnName("userId");

            entity.HasOne(d => d.Promo).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.PromoId)
                .HasConstraintName("subscription_to_promo");

            entity.HasOne(d => d.User).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("subscription_to_user");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_pkey");

            entity.ToTable("user");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('user_id_auto_inc'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.LessonId).HasColumnName("lessonId");
            entity.Property(e => e.RegistrationTime).HasColumnName("registrationTime");
            entity.Property(e => e.StageId).HasColumnName("stageId");
            entity.Property(e => e.TgId).HasColumnName("tgId");
            entity.Property(e => e.Timezone).HasColumnName("timezone");
            entity.Property(e => e.Username)
                .HasMaxLength(255)
                .HasColumnName("username");
        });

        modelBuilder.Entity<Userinfo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("userinfo_pkey");

            entity.ToTable("userinfo");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Age).HasColumnName("age");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Gender)
                .HasMaxLength(255)
                .HasColumnName("gender");
            entity.Property(e => e.Height).HasColumnName("height");
            entity.Property(e => e.Phone)
                .HasMaxLength(255)
                .HasColumnName("phone");
            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.Weight).HasColumnName("weight");

            entity.HasOne(d => d.User).WithMany(p => p.Userinfos)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("userInfo_to_user");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
