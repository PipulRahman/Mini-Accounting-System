using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; 
namespace MiniAccountingSystem.Models
{
    public class VoucherViewModel
    {
        public long VoucherId { get; set; }

        [Required(ErrorMessage = "Voucher Date is required.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Voucher Date")]
        public DateTime VoucherDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Voucher Type is required.")]
        [StringLength(20, ErrorMessage = "Voucher Type cannot exceed 20 characters.")]
        [Display(Name = "Voucher Type")]
        public string VoucherType { get; set; } = "Journal"; 
        [StringLength(50, ErrorMessage = "Reference No. cannot exceed 50 characters.")]
        [Display(Name = "Reference No.")]
        public string? ReferenceNo { get; set; }

        [StringLength(255, ErrorMessage = "Narration cannot exceed 255 characters.")]
        [Display(Name = "Narration")]
        public string? Narration { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }

        public List<VoucherDetailViewModel> VoucherDetails { get; set; } = new List<VoucherDetailViewModel>();

        [NotMapped]
        [Display(Name = "Total Debit")]
        public decimal TotalDebit { get; set; }

        [NotMapped]
        [Display(Name = "Total Credit")]
        public decimal TotalCredit { get; set; }
    }
}