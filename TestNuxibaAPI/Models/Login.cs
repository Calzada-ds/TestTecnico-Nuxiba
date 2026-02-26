using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestNuxibaAPI.Models
{
    [Table("ccloglogin")]
    public class Login
    {
        [Key]
        public int Id { get; set; } 

        [Required]
        public int User_id { get; set; }

        [Required]
        public int Extension { get; set; }

        [Required]
        [Range(0, 1, ErrorMessage = "TipoMov debe ser 0 (Logout) o 1 (Login).")]
        public int TipoMov { get; set; }

        [Required]
        public DateTime fecha { get; set; }

        [ForeignKey(nameof(User_id))]
        public User? User { get; set; }
    }
}