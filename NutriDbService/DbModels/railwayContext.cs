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
        public virtual DbSet<Gptrequest> Gptrequests { get; set; }
        public virtual DbSet<Loyalty> Loyalties { get; set; }
        public virtual DbSet<Meal> Meals { get; set; }
        public virtual DbSet<Messagelog> Messagelogs { get; set; }
        public virtual DbSet<Subscription> Subscriptions { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Userinfo> Userinfos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#if DEBUG
                optionsBuilder.UseNpgsql("Host=viaduct.proxy.rlwy.net;Port=38794;Username=postgres;Password=wTLZPRhYXHSReMKcUHSCNDEQlgQmbFDO;Database=railway");
#else
                        optionsBuilder.UseNpgsql("Host=postgres.railway.internal;Port=5432;Username=postgres;Password=wTLZPRhYXHSReMKcUHSCNDEQlgQmbFDO;Database=railway");
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

            modelBuilder.Entity<Gptrequest>(entity =>
            {
                entity.ToTable("gptrequest");

                entity.HasIndex(e => e.Id, "gpt_id_index")
                    .HasMethod("hash");

                entity.HasIndex(e => e.Request, "gpt_req_index")
                    .HasMethod("hash");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('gptrequest_id_auto_inc'::regclass)");

                entity.Property(e => e.Answer).HasColumnName("answer");

                entity.Property(e => e.CreationDate)
                    .HasColumnType("timestamp(6) without time zone")
                    .HasColumnName("creation_date");

                entity.Property(e => e.Done).HasColumnName("done");

                entity.Property(e => e.FinishDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("finish_date");

                entity.Property(e => e.Iserror).HasColumnName("iserror");

                entity.Property(e => e.ReqType)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("req_type");

                entity.Property(e => e.Request).HasColumnName("request");

                entity.Property(e => e.UserTgid).HasColumnName("user_tgid");
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

                entity.Property(e => e.MealTime)
                    .HasColumnType("timestamp(0) without time zone")
                    .HasColumnName("meal_time");

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

            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.ToTable("subscription");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('sub_id_auto_inc'::regclass)");

                entity.Property(e => e.AccountId).HasMaxLength(255);

                entity.Property(e => e.Amount).HasPrecision(10, 2);

                entity.Property(e => e.DateCreate).HasColumnType("timestamp without time zone");

                entity.Property(e => e.DateTime).HasColumnType("timestamp without time zone");

                entity.Property(e => e.DateUpdate).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Email).HasMaxLength(255);

                entity.Property(e => e.Extra).IsRequired();

                entity.Property(e => e.InvoiceId).HasMaxLength(255);

                entity.Property(e => e.Rrn).HasMaxLength(255);

                entity.Property(e => e.Status).HasMaxLength(255);

                entity.Property(e => e.SubscriptionId).HasMaxLength(255);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasDefaultValueSql("'unknown'::character varying");

                entity.Property(e => e.UserTgId).HasColumnName("userTgId");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("user");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('user_id_auto_inc'::regclass)");

                entity.Property(e => e.Email).HasMaxLength(255);

                entity.Property(e => e.LessonId).HasColumnName("lessonId");

                entity.Property(e => e.NotifyStatus)
                    .IsRequired()
                    .HasColumnName("notifyStatus")
                    .HasDefaultValueSql("true");

                entity.Property(e => e.RegistrationTime).HasColumnName("registrationTime");

                entity.Property(e => e.StageId).HasColumnName("stageId");

                entity.Property(e => e.TgId).HasColumnName("tgId");

                entity.Property(e => e.Timeslide)
                    .HasPrecision(10, 2)
                    .HasColumnName("timeslide");

                entity.Property(e => e.Username)
                    .HasMaxLength(255)
                    .HasColumnName("username");
            });

            modelBuilder.Entity<Userinfo>(entity =>
            {
                entity.ToTable("userinfo");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('userinfo_id_auto_inc'::regclass)");

                entity.Property(e => e.Age).HasColumnName("age");

                entity.Property(e => e.Donelessonlist)
                    .HasMaxLength(255)
                    .HasColumnName("donelessonlist");

                entity.Property(e => e.EveningPing)
                    .HasPrecision(6)
                    .HasColumnName("eveningPing");

                entity.Property(e => e.Extra).HasColumnName("extra");

                entity.Property(e => e.Gender)
                    .HasMaxLength(255)
                    .HasColumnName("gender");

                entity.Property(e => e.Goal)
                    .HasMaxLength(5)
                    .HasColumnName("goal");

                entity.Property(e => e.Goalkk)
                    .HasPrecision(10, 2)
                    .HasColumnName("goalkk");

                entity.Property(e => e.Height)
                    .HasPrecision(10, 2)
                    .HasColumnName("height");

                entity.Property(e => e.LastlessonTime)
                    .HasColumnType("timestamp(0) without time zone")
                    .HasColumnName("lastlessonTime")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.MorningPing)
                    .HasPrecision(6)
                    .HasColumnName("morningPing");

                entity.Property(e => e.TgId).HasColumnName("tgId");

                entity.Property(e => e.Timeslide)
                    .HasPrecision(10, 2)
                    .HasColumnName("timeslide");

                entity.Property(e => e.UserId).HasColumnName("userId");

                entity.Property(e => e.Vote)
                    .HasColumnName("vote")
                    .HasDefaultValueSql("11");

                entity.Property(e => e.Weight)
                    .HasPrecision(10, 2)
                    .HasColumnName("weight");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Userinfos)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_userinfo_user");
            });

            modelBuilder.HasSequence("dish_id_auto_inc");

            modelBuilder.HasSequence("meal_id_auto_inc");

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
