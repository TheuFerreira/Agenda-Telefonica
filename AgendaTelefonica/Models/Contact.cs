using System.ComponentModel.DataAnnotations;

namespace AgendaTelefonica.Models
{
    public class Contact
    {
        [Key]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "O Nome do Contato é necessário!!")]
        [MaxLength(100)]
        public string Name { get; set; }

        public List<Phone> Phones { get; set; }

        public Contact() { }

        public Contact(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
