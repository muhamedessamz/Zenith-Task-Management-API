using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Entities;

namespace TaskManagement.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<TaskItem> Tasks { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<ChecklistItem> ChecklistItems { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<TaskTag> TaskTags { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
        public DbSet<TaskAssignment> TaskAssignments { get; set; } = null!;
        public DbSet<EmailOtp> EmailOtps { get; set; } = null!;
        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<ProjectMember> ProjectMembers { get; set; } = null!;
        public DbSet<TaskAttachment> TaskAttachments { get; set; } = null!;
        public DbSet<TimeEntry> TimeEntries { get; set; } = null!;
        public DbSet<TaskDependency> TaskDependencies { get; set; } = null!;
        public DbSet<UserCalendarIntegration> UserCalendarIntegrations { get; set; } = null!;
        public DbSet<SharedLink> SharedLinks { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TaskItem>(entity =>
            {
                entity.HasKey(t => t.Id);
                
                entity.Property(t => t.Title)
                      .IsRequired()
                      .HasMaxLength(100);
                
                entity.Property(t => t.Description)
                      .HasMaxLength(500);
                
                entity.Property(t => t.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(t => t.Priority)
                      .HasConversion<int>()
                      .IsRequired();

                // User-Task relationship (NoAction to avoid cascade conflicts)
                entity.HasOne(t => t.User)
                      .WithMany(u => u.Tasks)
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.NoAction)
                      .IsRequired();

                // Category-Task relationship
                entity.HasOne(t => t.Category)
                      .WithMany(c => c.Tasks)
                      .HasForeignKey(t => t.CategoryId)
                      .OnDelete(DeleteBehavior.NoAction)
                      .IsRequired(false);
            });

            // Configure Category entity
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.Name)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(c => c.Description)
                      .HasMaxLength(200);

                entity.Property(c => c.Color)
                      .IsRequired()
                      .HasMaxLength(7);

                entity.Property(c => c.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                // User-Category relationship
                entity.HasOne(c => c.User)
                      .WithMany(u => u.Categories)
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.NoAction)
                      .IsRequired();
            });

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.FirstName)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(u => u.LastName)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(u => u.DisplayName)
                      .HasMaxLength(50);

                entity.Property(u => u.ProfilePicture)
                      .HasMaxLength(500);

                entity.Property(u => u.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");
            });

            // Configure ChecklistItem entity
            modelBuilder.Entity<ChecklistItem>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.Title)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(c => c.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                // Task-ChecklistItem relationship
                entity.HasOne(c => c.Task)
                      .WithMany(t => t.ChecklistItems)
                      .HasForeignKey(c => c.TaskId)
                      .OnDelete(DeleteBehavior.NoAction)
                      .IsRequired();
            });

            // Configure RefreshToken entity
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.Property(r => r.Token)
                      .IsRequired()
                      .HasMaxLength(500);

                entity.Property(r => r.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                // User-RefreshToken relationship
                entity.HasOne(r => r.User)
                      .WithMany(u => u.RefreshTokens)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.NoAction)
                      .IsRequired();
            });

            // Configure Tag entity
            modelBuilder.Entity<Tag>(entity =>
            {
                entity.HasKey(t => t.Id);

                entity.Property(t => t.Name)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(t => t.Color)
                      .IsRequired()
                      .HasMaxLength(7);

                entity.Property(t => t.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                // User-Tag relationship
                entity.HasOne(t => t.User)
                      .WithMany(u => u.Tags)
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.NoAction)
                      .IsRequired();
            });

            // Configure TaskTag (many-to-many junction table)
            modelBuilder.Entity<TaskTag>(entity =>
            {
                entity.HasKey(tt => new { tt.TaskId, tt.TagId });

                entity.HasOne(tt => tt.Task)
                      .WithMany(t => t.TaskTags)
                      .HasForeignKey(tt => tt.TaskId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(tt => tt.Tag)
                      .WithMany(t => t.TaskTags)
                      .HasForeignKey(tt => tt.TagId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.Property(tt => tt.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");
            });

            // Configure Comment entity
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.Content)
                      .IsRequired()
                      .HasMaxLength(1000);

                entity.Property(c => c.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                // Task-Comment relationship (NoAction to avoid cascade conflict)
                entity.HasOne(c => c.Task)
                      .WithMany(t => t.Comments)
                      .HasForeignKey(c => c.TaskId)
                      .OnDelete(DeleteBehavior.NoAction);

                // User-Comment relationship WITH navigation property
                entity.HasOne(c => c.User)
                      .WithMany(u => u.Comments)
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // Configure TaskAssignment entity
            modelBuilder.Entity<TaskAssignment>(entity =>
            {
                entity.HasKey(ta => ta.Id);

                entity.Property(ta => ta.Note)
                      .HasMaxLength(500);

                entity.Property(ta => ta.Permission)
                      .IsRequired()
                      .HasMaxLength(20)
                      .HasDefaultValue("Viewer");

                entity.Property(ta => ta.AssignedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                // Task-Assignment relationship (NoAction to avoid cascade conflict with User)
                entity.HasOne(ta => ta.Task)
                      .WithMany(t => t.Assignments)
                      .HasForeignKey(ta => ta.TaskId)
                      .OnDelete(DeleteBehavior.NoAction);

                // AssignedTo User relationship
                entity.HasOne(ta => ta.AssignedToUser)
                      .WithMany()
                      .HasForeignKey(ta => ta.AssignedToUserId)
                      .OnDelete(DeleteBehavior.NoAction);

                // AssignedBy User relationship
                entity.HasOne(ta => ta.AssignedByUser)
                      .WithMany()
                      .HasForeignKey(ta => ta.AssignedByUserId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // Configure EmailOtp entity
            modelBuilder.Entity<EmailOtp>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Email)
                      .IsRequired()
                      .HasMaxLength(256);

                entity.Property(e => e.OtpCode)
                      .IsRequired()
                      .HasMaxLength(6);

                entity.Property(e => e.Purpose)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(e => new { e.Email, e.OtpCode, e.Purpose });
            });

            // Configure Project entity
            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Title).IsRequired().HasMaxLength(200);
                entity.Property(p => p.Description).HasMaxLength(2000);
                entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                // Priority conversion (enum to int)
                entity.Property(p => p.Priority)
                      .HasConversion<int>()
                      .IsRequired();

                // User-Project relationship
                entity.HasOne(p => p.User)
                      .WithMany()
                      .HasForeignKey(p => p.UserId)
                      .OnDelete(DeleteBehavior.NoAction);

                // Category-Project relationship
                entity.HasOne(p => p.Category)
                      .WithMany()
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.NoAction);

                // Project-Tasks relationship
                entity.HasMany(p => p.Tasks)
                      .WithOne(t => t.Project)
                      .HasForeignKey(t => t.ProjectId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // Configure ProjectMember entity
            modelBuilder.Entity<ProjectMember>(entity =>
            {
                entity.HasKey(pm => pm.Id);
                entity.Property(pm => pm.Role).IsRequired().HasMaxLength(50);
                entity.Property(pm => pm.JoinedAt).HasDefaultValueSql("GETUTCDATE()");

                // Project-Members relationship
                entity.HasOne(pm => pm.Project)
                      .WithMany(p => p.Members)
                      .HasForeignKey(pm => pm.ProjectId)
                      .OnDelete(DeleteBehavior.NoAction);

                // User-ProjectMember relationship
                entity.HasOne(pm => pm.User)
                      .WithMany()
                      .HasForeignKey(pm => pm.UserId)
                      .OnDelete(DeleteBehavior.NoAction);

                // Unique constraint: User can only be member once per project
                entity.HasIndex(pm => new { pm.ProjectId, pm.UserId })
                      .IsUnique();
            });

            // Configure TaskAttachment entity
            modelBuilder.Entity<TaskAttachment>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.FileName).IsRequired().HasMaxLength(255);
                entity.Property(a => a.StoredFileName).IsRequired().HasMaxLength(255);
                entity.Property(a => a.FilePath).IsRequired().HasMaxLength(500);
                entity.Property(a => a.ContentType).HasMaxLength(100);
                entity.Property(a => a.UploadedAt).HasDefaultValueSql("GETUTCDATE()");

                // Task-Attachment relationship
                entity.HasOne(a => a.Task)
                      .WithMany(t => t.Attachments)
                      .HasForeignKey(a => a.TaskId)
                      .OnDelete(DeleteBehavior.NoAction);

                // User-Attachment relationship
                entity.HasOne(a => a.UploadedByUser)
                      .WithMany()
                      .HasForeignKey(a => a.UploadedByUserId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // Configure TimeEntry
            modelBuilder.Entity<TimeEntry>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.StartTime).IsRequired();
                entity.Property(t => t.Notes).HasMaxLength(500);
                entity.Property(t => t.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(t => t.Task)
                      .WithMany()
                      .HasForeignKey(t => t.TaskId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(t => t.User)
                      .WithMany()
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // Configure TaskDependency
            modelBuilder.Entity<TaskDependency>(entity =>
            {
                entity.HasKey(d => new { d.TaskId, d.DependsOnTaskId });

                entity.HasOne(d => d.Task)
                      .WithMany()
                      .HasForeignKey(d => d.TaskId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.DependsOnTask)
                      .WithMany()
                      .HasForeignKey(d => d.DependsOnTaskId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure UserCalendarIntegration
            modelBuilder.Entity<UserCalendarIntegration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.Provider }).IsUnique();

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure SharedLink
            modelBuilder.Entity<SharedLink>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Token).IsUnique();
                
                entity.HasOne(e => e.CreatedBy)
                      .WithMany()
                      .HasForeignKey(e => e.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}

