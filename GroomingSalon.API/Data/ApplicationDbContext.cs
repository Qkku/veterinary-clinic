using System;
using System.Collections.Generic;
using GroomingSalon.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GroomingSalon.API.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<Groomer> Groomers { get; set; }

    public virtual DbSet<GroomerSchedule> GroomerSchedules { get; set; }

    public virtual DbSet<GroomerTimeOff> GroomerTimeOffs { get; set; }

    public virtual DbSet<Pet> Pets { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("appointments");

            entity.HasIndex(e => e.UserId, "idx_app_client");

            entity.HasIndex(e => e.GroomerId, "idx_app_groomer");

            entity.HasIndex(e => e.PetId, "idx_app_pet");

            entity.HasIndex(e => e.ServiceId, "idx_app_service");

            entity.HasIndex(e => e.StartDatetime, "idx_app_start");

            entity.HasIndex(e => e.Status, "idx_app_status");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Comment)
                .HasColumnType("text")
                .HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.EndDatetime)
                .HasColumnType("datetime")
                .HasColumnName("end_datetime");
            entity.Property(e => e.GroomerId).HasColumnName("groomer_id");
            entity.Property(e => e.PetId).HasColumnName("pet_id");
            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.StartDatetime)
                .HasColumnType("datetime")
                .HasColumnName("start_datetime");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'booked'")
                .HasColumnType("enum('booked','confirmed','completed','cancelled','no_show')")
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Groomer).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.GroomerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_app_groomer");

            entity.HasOne(d => d.Pet).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.PetId)
                .HasConstraintName("fk_app_pet");

            entity.HasOne(d => d.Service).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_app_service");

            entity.HasOne(d => d.User).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_app_user");
        });

        modelBuilder.Entity<Groomer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("groomers");

            entity.HasIndex(e => e.IsActive, "idx_groomers_is_active");

            entity.HasIndex(e => e.Email, "uq_groomers_email").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Age).HasColumnName("age");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Education)
                .HasMaxLength(255)
                .HasColumnName("education");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.ExperienceYears).HasColumnName("experience_years");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(30)
                .HasColumnName("phone");
            entity.Property(e => e.Specialization)
                .HasMaxLength(150)
                .HasColumnName("specialization");
        }   );

        modelBuilder.Entity<GroomerSchedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("groomer_schedule");

            entity.HasIndex(e => e.DayOfWeek, "idx_schedule_day");

            entity.HasIndex(e => e.GroomerId, "idx_schedule_groomer");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DayOfWeek)
                .HasComment("1=Пн ... 7=Вс")
                .HasColumnName("day_of_week");
            entity.Property(e => e.EndTime)
                .HasColumnType("time")
                .HasColumnName("end_time");
            entity.Property(e => e.GroomerId).HasColumnName("groomer_id");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.SlotDurationMinutes)
                .HasDefaultValueSql("'60'")
                .HasColumnName("slot_duration_minutes");
            entity.Property(e => e.StartTime)
                .HasColumnType("time")
                .HasColumnName("start_time");
            entity.Property(e => e.ValidFrom)
                .HasColumnType("date")
                .HasColumnName("valid_from");
            entity.Property(e => e.ValidTo)
                .HasColumnType("date")
                .HasColumnName("valid_to");

            entity.HasOne(d => d.Groomer).WithMany(p => p.GroomerSchedules)
                .HasForeignKey(d => d.GroomerId)
                .HasConstraintName("fk_schedule_groomer");
        });

        modelBuilder.Entity<GroomerTimeOff>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("groomer_time_off");

            entity.HasIndex(e => e.GroomerId, "idx_timeoff_groomer");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Comment)
                .HasColumnType("text")
                .HasColumnName("comment");
            entity.Property(e => e.EndDatetime)
                .HasColumnType("datetime")
                .HasColumnName("end_datetime");
            entity.Property(e => e.GroomerId).HasColumnName("groomer_id");
            entity.Property(e => e.StartDatetime)
                .HasColumnType("datetime")
                .HasColumnName("start_datetime");
            entity.Property(e => e.Type)
                .HasDefaultValueSql("'other'")
                .HasColumnType("enum('vacation','sick_leave','other')")
                .HasColumnName("type");

            entity.HasOne(d => d.Groomer).WithMany(p => p.GroomerTimeOffs)
                .HasForeignKey(d => d.GroomerId)
                .HasConstraintName("fk_timeoff_groomer");
        });

        modelBuilder.Entity<Pet>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("pets");

            entity.HasIndex(e => e.UserId, "idx_pets_client");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Age).HasColumnName("age");
            entity.Property(e => e.Breed)
                .HasMaxLength(100)
                .HasColumnName("breed");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Notes)
                .HasColumnType("text")
                .HasColumnName("notes");
            entity.Property(e => e.Size)
                .HasColumnType("enum('small','medium','large')")
                .HasColumnName("size");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Pets)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_pets_user");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("reviews");

            entity.HasIndex(e => e.AppointmentId, "idx_rev_appointment");

            entity.HasIndex(e => e.UserId, "idx_rev_client");

            entity.HasIndex(e => e.GroomerId, "idx_rev_groomer");

            entity.HasIndex(e => e.AppointmentId, "uq_review_per_appointment").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AppointmentId).HasColumnName("appointment_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.GroomerId).HasColumnName("groomer_id");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.Text)
                .HasColumnType("text")
                .HasColumnName("text");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Appointment).WithOne(p => p.Review)
                .HasForeignKey<Review>(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_rev_appointment");

            entity.HasOne(d => d.Groomer).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.GroomerId)
                .HasConstraintName("fk_rev_groomer");
                
            entity.HasOne(d => d.User).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_rev_user");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("services");

            entity.HasIndex(e => e.IsActive, "idx_services_is_active");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.Price)
                .HasPrecision(10)
                .HasColumnName("price");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("users");

            entity.HasIndex(e => e.IsAdmin, "idx_users_is_admin");

            entity.HasIndex(e => e.Email, "uq_users_email").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.IsAdmin).HasColumnName("is_admin");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(30)
                .HasColumnName("phone");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
