using System.ComponentModel.DataAnnotations;

namespace Heroes.Models
{
     
    public class HeroDetail
    {
        public string HeroID { get; set; }
        public string HeroName { get; set; }
        public string HeroDesc { get; set; }
        public string HeroIMG { get; set; }
        public int HeroVoted { get; set; }
    }

    public class PostComment
    {
        [Required]
        public string HeroID { get;set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Message { get; set; }
    }

    public class Account
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}