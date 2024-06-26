﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace NutriDbService.DbModels
{
    public partial class railwayContext : DbContext
    {
        public railwayContext()
        {
        }

        public railwayContext(DbContextOptions<railwayContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Dish> Dishes { get; set; }
        public virtual DbSet<Loyalty> Loyalties { get; set; }
        public virtual DbSet<Meal> Meals { get; set; }
        public virtual DbSet<Messagelog> Messagelogs { get; set; }
        public virtual DbSet<Promo> Promos { get; set; }
        public virtual DbSet<Subscription> Subscriptions { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Userinfo> Userinfos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
#if !DEBUG
                optionsBuilder.UseNpgsql("Host=postgres.railway.internal;Port=5432;Username=postgres;Password=wTLZPRhYXHSReMKcUHSCNDEQlgQmbFDO;Database=railway");
#else               
                optionsBuilder.UseNpgsql("Host=viaduct.proxy.rlwy.net;Port=38794;Username=postgres;Password=wTLZPRhYXHSReMKcUHSCNDEQlgQmbFDO;Database=railway");
#endif
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Dish>(entity =>
            {
                entity.ToTable("dishes");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('dish_id_auto_inc'::regclass)");

                entity.Property(e => e.Carbs)
                    .HasPrecision(10, 2)
                    .HasColumnName("carbs");

                entity.Property(e => e.Description).HasColumnName("description");

                entity.Property(e => e.Fats)
                    .HasPrecision(10, 2)
                    .HasColumnName("fats");

                entity.Property(e => e.Kkal)
                    .HasPrecision(10, 2)
                    .HasColumnName("kkal");

                entity.Property(e => e.MealId).HasColumnName("meal_id");

                entity.Property(e => e.Protein)
                    .HasPrecision(10, 2)
                    .HasColumnName("protein");

                entity.Property(e => e.Weight)
                    .HasPrecision(10, 3)
                    .HasColumnName("weight");

                entity.HasOne(d => d.Meal)
                    .WithMany(p => p.Dishes)
                    .HasForeignKey(d => d.MealId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("dish_to_meal");
            });

            modelBuilder.Entity<Loyalty>(entity =>
            {
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

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Loyalties)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("loyal_to_user");
            });

            modelBuilder.Entity<Meal>(entity =>
            {
                entity.ToTable("meal");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('meal_id_auto_inc'::regclass)");

                entity.Property(e => e.Description).HasColumnName("description");

                entity.Property(e => e.Timestamp)
                    .HasColumnType("timestamp(0) without time zone")
                    .HasColumnName("timestamp");

                entity.Property(e => e.Type).HasColumnName("type");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.Weight)
                    .HasPrecision(10, 2)
                    .HasColumnName("weight");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Meals)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("meal_to_user");
            });

            modelBuilder.Entity<Messagelog>(entity =>
            {
                entity.ToTable("messagelog");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('log_id_auto_inc'::regclass)");

                entity.Property(e => e.BotMessage).HasColumnName("botMessage");

                entity.Property(e => e.Step).HasColumnName("step");

                entity.Property(e => e.UserId).HasColumnName("userId");

                entity.Property(e => e.UserMessage).HasColumnName("userMessage");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Messagelogs)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("log_to_user");
            });

            modelBuilder.Entity<Promo>(entity =>
            {
                entity.ToTable("promo");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('promo_id_auto_inc'::regclass)");

                entity.Property(e => e.Discount).HasColumnName("discount");

                entity.Property(e => e.Freeperiod).HasColumnName("freeperiod");

                entity.Property(e => e.PromoCode)
                    .HasMaxLength(255)
                    .HasColumnName("promoCode");
            });

            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.ToTable("subscription");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('sub_id_auto_inc'::regclass)");

                entity.Property(e => e.PaymentMethod)
                    .HasMaxLength(255)
                    .HasColumnName("paymentMethod");

                entity.Property(e => e.PromoId).HasColumnName("promoId");

                entity.Property(e => e.SubscriptionEndDate).HasColumnName("subscriptionEndDate");

                entity.Property(e => e.SubscriptionStartDate).HasColumnName("subscriptionStartDate");

                entity.Property(e => e.UserId).HasColumnName("userId");

                entity.HasOne(d => d.Promo)
                    .WithMany(p => p.Subscriptions)
                    .HasForeignKey(d => d.PromoId)
                    .HasConstraintName("subscription_to_promo");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Subscriptions)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("subscription_to_user");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("user");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('user_id_auto_inc'::regclass)");

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

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Userinfos)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("userInfo_to_user");
            });

            modelBuilder.HasSequence("dish_id_auto_inc");

            modelBuilder.HasSequence("meal_id_auto_inc");

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
