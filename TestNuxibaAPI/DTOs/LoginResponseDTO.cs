using System.ComponentModel.DataAnnotations;

namespace TestNuxibaAPI.DTOs
{
    public class LoginResponseDTO
    {
        public int Id { get; set; }
        public int User_id { get; set; }
        public int Extension { get; set; }
        public int TipoMov { get; set; }
        public DateTime fecha { get; set; }
    }
}
