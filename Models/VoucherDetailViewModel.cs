using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; 
using Microsoft.AspNetCore.Mvc.Rendering; 
namespace MiniAccountingSystem.Models
{
    public class VoucherDetailViewModel
    {
        public long VoucherDetailId { get; set; }
        public long VoucherId { get; set; } 
        [Required(ErrorMessage = "Account is required.")]
        [Display(Name = "Account")]
        public int AccountId { get; set; }

        [Display(Name = "Account Name")]
        public string? AccountName { get; set; } 
        [Range(0.00, 9999999999999999.99, ErrorMessage = "Debit amount must be non-negative.")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Debit")]
        public decimal DebitAmount { get; set; } = 0;

        [Range(0.00, 9999999999999999.99, ErrorMessage = "Credit amount must be non-negative.")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Credit")]
        public decimal CreditAmount { get; set; } = 0;

        [NotMapped]
        public IEnumerable<SelectListItem>? AvailableAccounts { get; set; }
    }
}