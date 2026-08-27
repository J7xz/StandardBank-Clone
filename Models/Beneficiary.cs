using System;
using System.ComponentModel.DataAnnotations;

namespace StandardBank.Models
{
    public class Beneficiary
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [StringLength(20)]
        public string AccountNumber { get; set; }

        [Required]
        public string BankName { get; set; }

        public string Nickname { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        // Foreign Key
        [Required]
        public string UserId { get; set; }

        // Navigation property
        public virtual User User { get; set; }
    }
}