using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestNuxibaAPI.Models
{
    [Table("ccRIACat_Areas")]
    public class Area
    {
        [Key]
        public int Id { get; set; } 

        public int IDArea { get; set; }

        [Required]
        [MaxLength(100)] 
        public string AreaName { get; set; } = string.Empty;
        public int StatusArea { get; set; }
        public DateTime CreateDate { get; set; }
    }
}