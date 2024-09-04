using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EchoBot1.Modelos
{
    [Table("UserProfile")]
    public class UserProfile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public string UserId { get; set; }  // ID do usuário

        [MaxLength(255)]
        public string Name { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public UserProfile() { }

        public UserProfile(string userId, string name, string email)
        {
            UserId = userId;
            Name = name;
            Email = email;
        }
    }
}
