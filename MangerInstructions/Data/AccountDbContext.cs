using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MangerInstructions.Models
{
    public class AccountDbContext : DbContext
    {
        public DbSet<Tag> Tags { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Instruction> Instructions { get; set; }

        private IConfiguration Configuration { get; }

        public AccountDbContext(DbContextOptions<AccountDbContext> options, IConfiguration configuration)
            : base(options)
        {
            Configuration = configuration;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            CommentSetting(modelBuilder);
            StepInstructionSetting(modelBuilder);
            SetInstruction(modelBuilder);
            SetPersonalPage(modelBuilder);
            UserSetting(modelBuilder);
            SetData(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies().UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
        }

        private void SetPersonalPage(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PersonalPage>()
                .HasMany(i => i.Instructions)
                .WithOne(p => p.PersonalPage)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private void SetInstruction(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Instruction>()
                .HasMany(s => s.Steps)
                .WithOne(i => i.Instruction)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Instruction>()
                .HasMany(t => t.Tags)
                .WithOne(i => i.Instruction)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Instruction>()
                .HasMany(v => v.Votes)
                .WithOne(i => i.Instruction)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Instruction>()
                .HasMany(c => c.Comments)
                .WithOne(i => i.Instruction)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private void CommentSetting(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Comment>()
                .HasMany(l => l.Likes)
                .WithOne(c => c.Comment)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private void UserSetting(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(r => r.Role)
                .HasConversion(
                    v => v.ToString(),
                    v => (Role)Enum.Parse(typeof(Role), v));
            modelBuilder.Entity<User>()
                .HasOne(p => p.PersonalPage)
                .WithOne(u => u.User)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private void SetData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>()
                .HasData(
                    new Category { Id = 1, Index = "ArtsAndEntertainment" },
                    new Category { Id = 2, Index = "Cars" },
                    new Category { Id = 3, Index = "ComputersAndElectronics" },
                    new Category { Id = 4, Index = "EducationAndCommunications" },
                    new Category { Id = 5, Index = "FamilyLife" },
                    new Category { Id = 6, Index = "FinanceAndBusiness" },
                    new Category { Id = 7, Index = "FoodAndEntertaining" },
                    new Category { Id = 8, Index = "Health" },
                    new Category { Id = 9, Index = "HobbiesAndCrafts" },
                    new Category { Id = 10, Index = "HolidaysAndTraditions" },
                    new Category { Id = 11, Index = "HomeAndGarden" },
                    new Category { Id = 12, Index = "PersonalCareAndStyle" },
                    new Category { Id = 13, Index = "PetsAndAnimals" },
                    new Category { Id = 14, Index = "Relationships" },
                    new Category { Id = 15, Index = "PhilosophyAndReligion" },
                    new Category { Id = 16, Index = "SportsAndFitness" },
                    new Category { Id = 17, Index = "Travel" },
                    new Category { Id = 18, Index = "WorkWorld" },
                    new Category { Id = 19, Index = "Youth" }
                );
        }

        private void StepInstructionSetting(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StepInstruction>()
                .Property(i => i.ImageLinks)
                .HasConversion(
                    i => String.Join(';', i),
                    i => (i.Length > 0) ? i.Split(new char[] { ';' }).ToList() : new List<string>());
        }
    }
}
