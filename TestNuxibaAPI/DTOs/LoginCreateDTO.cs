using System.ComponentModel.DataAnnotations;

namespace NuxibaPracticeAPI.DTOs
{
    public class LoginCreateDTO
    {
        [Required]
        public int User_id { get; set; }

        public int Extension { get; set; }

        [Required]
        [Range(0, 1, ErrorMessage = "TipoMov debe ser 0 (Logout) o 1 (Login).")]
        public int TipoMov { get; set; }

        [Required]
        public DateTime fecha { get; set; }
    }
}