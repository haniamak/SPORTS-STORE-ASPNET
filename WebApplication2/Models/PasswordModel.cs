namespace WebApplication2.Models
{
    public class PasswordModel
    {
        public int UserId { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public int HashRounds { get; set; }
        public DateTime PasswordSetDate { get; set; }
    }
}
