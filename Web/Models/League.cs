using System.ComponentModel.DataAnnotations;

namespace Web.Models
{
    public class League
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Hockey Category")]
        public int Hockey_Category_Id { get; set; }
    }
}
