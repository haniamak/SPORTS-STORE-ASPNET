using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Data
{
    [Table("PASSWORD")]
    public class PASSWORD
    {
        public int UserId { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public int HashRounds { get; set; }
        public DateTime PasswordSetDate { get; set; }

    }
}


