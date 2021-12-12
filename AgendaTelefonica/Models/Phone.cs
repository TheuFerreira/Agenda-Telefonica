namespace AgendaTelefonica.Models
{
    public class Phone
    {
        public int Id { get; set; }
        public long Number { get; set; }

        public Phone() { }

        public Phone(int id, long number)
        {
            Id = id;
            Number = number;
        }
    }
}
