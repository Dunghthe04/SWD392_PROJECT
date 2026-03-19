using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SWD392_PROJECT.Models
{
    public class Payment
   {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Column(TypeName = "real")]
        public float Amount { get; set; }

        [MaxLength(100)]
        // Ensure non-null default to satisfy nullable reference checks
        public string Status { get; set; } = "Pending";
    }
}