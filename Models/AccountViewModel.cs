using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace MiniAccountingSystem.Models
{
    public class AccountViewModel
    {
        public int AccountId { get; set; }

        [Required(ErrorMessage = "Account Name is required.")]
        [StringLength(100, ErrorMessage = "Account Name cannot exceed 100 characters.")]
        [Display(Name = "Account Name")]
        public string AccountName { get; set; }

        [StringLength(20, ErrorMessage = "Account Code cannot exceed 20 characters.")]
        [Display(Name = "Account Code")]
        public string? AccountCode { get; set; } 
        [Display(Name = "Parent Account")]
        public int? ParentAccountId { get; set; } 
        [Display(Name = "Parent Account Name")]
        public string? ParentAccountName { get; set; } 
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [NotMapped]
        public IEnumerable<SelectListItem>? AvailableParentAccounts { get; set; }
    }
}