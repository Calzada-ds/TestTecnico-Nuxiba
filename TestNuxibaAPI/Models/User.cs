using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestNuxibaAPI.Models
{
    [Table("ccUsers")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int User_id { get; set; }

        [Required]
        [MaxLength(50)] 
        public string Login { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Nombres { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ApellidoPaterno { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ApellidoMaterno { get; set; }

        [Required]
        [MaxLength(255)] 
        public string Password { get; set; } = string.Empty;
        public int TipoUser_id { get; set; }
        public int Status { get; set; }
        public DateTime fCreate { get; set; }
        public int IDArea { get; set; }
        public DateTime? LastLoginAttempt { get; set; } // Puede ser nulo según la imagen
                                                       
        [ForeignKey(nameof(IDArea))]
        public Area? Area { get; set; }
        public ICollection<Login> Logins { get; set; } = new List<Login>();
    }
}