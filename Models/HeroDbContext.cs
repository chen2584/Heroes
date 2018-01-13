using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace Heroes.Models
{
    public class HeroDbContext : DbContext
    {
        public DbSet<Hero> Heroes { get; set; }
        public DbSet<Comments> Comments { get; set; }
        public DbSet<Votes> Votes { get; set; }
        public DbSet<Admins> Admins { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=127.0.0.1;Database=HeroDB;UID=Chen;Password=1234;Timeout=30;");
        }
    }

    public class Hero
    {
        [Key]
        public string HeroID { get; set; }
        public string HeroName { get; set; }
        public string HeroDesc { get; set; }
        public string HeroIMG { get; set; }
    }

    public class Comments
    {
        [Key]
        public int CommentID { get; set; }
        public int ParentID { get; set; }
        public string HeroID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public string IPAddress { get; set; }
        public DateTime CommentDate { get; set; }
        [ForeignKey("HeroID")]
        public Hero Heroes { get; set; }

    }

    public class Votes
    {
        [Key]
        public int VoteID { get; set; }
        public string HeroID { get; set; }
        public string IPAddress { get; set; } 
        public DateTime VoteDate { get; set; }
        [ForeignKey("HeroID")]
        public Hero Heroes { get; set; }
    }

    public class Admins
    {
        [Key]
        public int AdminID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; } 
        public DateTime LastLogin { get; set; }
    }
}