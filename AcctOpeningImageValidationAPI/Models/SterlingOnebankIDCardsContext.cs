using System;
using Microsoft.EntityFrameworkCore;

namespace AcctOpeningImageValidationAPI.Models
{
    public class SterlingOnebankIDCardsContext : DbContext
    {
        public SterlingOnebankIDCardsContext()
        {
        }
        public SterlingOnebankIDCardsContext(DbContextOptions<SterlingOnebankIDCardsContext> options)
          : base(options)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                @"Server=localhost;Database=SterlingOnebankIDCards;Persist Security Info=False;User ID=sa;Password=reallyStrongPwd123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;ConnectRetryCount=0");
        }
        public DbSet<AppruvResponse> AppruvResponses { get; set; }
        public DbSet<OCRResponse> OCRResponses { get; set; }
        public DbSet<ScannedIDCardDetails> ScannedIDCardDetail { get; set; }
        public DbSet<FacialValidation> FacialValidations { get; set; }
        public DbSet<OCRUsage> OCRUsages { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OCRResponse>(entity =>
            {
                entity.Property(e => e.DateInserted)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.BVN)
              .HasMaxLength(11);


            });
            modelBuilder.Entity<ScannedIDCardDetails>(entity =>
            {
                entity.Property(e => e.DateInserted)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.BVN)
              .HasMaxLength(11);


            });

            modelBuilder.Entity<AppruvResponse>(entity =>
            {
                entity.Property(e => e.DateInserted)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.BVN)
              .HasMaxLength(11);


            });

            modelBuilder.Entity<FacialValidation>(entity =>
            {
                entity.Property(e => e.DateInserted)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.BVN)
              .HasMaxLength(11);


            });
        }


    }


    public class OCRResponse
    {
        public int Id { get; set; }

        public string BVN { get; set; }
        public string JsonResponse { get; set; }
        public DateTime? DateInserted { get; set; }
    }

    public class ScannedIDCardDetails
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string IDType { get; set; }
        public string IDNumber { get; set; }
        public string IssueDate { get; set; }
        public string DateOfBirth { get; set; }
        public string ExpiryDate { get; set; }
        public string FormerIDNumber { get; set; }
        public string IDClass { get; set; }
        public string BVN { get; set; }
        public string Email { get; set; }
        public string BloodGroup { get; set; }
        public string Height { get; set; }
        public string IssuingAuthority { get; set; }
        public string Address { get; set; }
        public string Occupation { get; set; }
        public string NextOfKin { get; set; }
        public string FirstIssueState { get; set; }
        public string Delim { get; set; }
        public string Gender { get; set; }
        public DateTime? DateInserted { get; set; }
    }

    public class AppruvResponse
    {
        public int Id { get; set; }

        public string BVN { get; set; }
        public string Email { get; set; }
        public string StatusOfRequest { get; set; }
        public DateTime? DateInserted { get; set; }

        public string JsonResponse { get; set; }

    }

    public class FacialValidation
    {
        public int Id { get; set; }

        public string BVN { get; set; }
        public string Email { get; set; }
        public string Accessories { get; set; }
        public string FacialHair { get; set; }
        public string Hair { get; set; }
        public string Emotion { get; set; }
        public string Smile { get; set; }
        public string Age { get; set; }
        public string HeadPose { get; set; }
        public string Gender { get; set; }
        public string Occlusion { get; set; }
        public DateTime? DateInserted { get; set; }
    }


}
