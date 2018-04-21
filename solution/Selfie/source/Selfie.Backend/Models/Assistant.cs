using System.ComponentModel.DataAnnotations;

namespace Selfie.Backend.Models
{
    public class Assistant
    {        
        [Key]
        public int Id { get; set; }
        [MaxLength(50)]
        public string Name { get; set; }
        [MaxLength(20)]
        public string Telephone { get; set; }
        [MaxLength(100)]
        public string Email { get; set; }
        [MaxLength(100)]
        public string Blobname { get; set; }
    }
}