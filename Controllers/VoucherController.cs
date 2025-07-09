using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; using MiniAccountingSystem.DAL;
using MiniAccountingSystem.Models;
using System.Diagnostics; using Microsoft.Data.SqlClient; 
namespace MiniAccountingSystem.Controllers
{
    [Authorize(Roles = "Admin")]     
    public class VoucherController : Controller
    {
        private readonly VoucherDAL _voucherDAL;
        private readonly AccountDAL _accountDAL; 
        public VoucherController(VoucherDAL voucherDAL, AccountDAL accountDAL)
        {
            _voucherDAL = voucherDAL;
            _accountDAL = accountDAL;
        }

        public async Task<IActionResult> Index()
        {
            var vouchers = await _voucherDAL.GetAllVouchersAsync();
            return View(vouchers);
        }

        private async Task PopulateAccountDropdowns(List<VoucherDetailViewModel> details)
        {
            var accounts = await _accountDAL.GetAccountsForDropdownAsync();             
            var selectList = new SelectList(accounts, "AccountId", "AccountName");

            foreach (var detail in details)
            {
                detail.AvailableAccounts = selectList;
            }
        }

        public async Task<IActionResult> Create()
        {
            var model = new VoucherViewModel();
            model.VoucherDetails.Add(new VoucherDetailViewModel());
            await PopulateAccountDropdowns(model.VoucherDetails);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VoucherViewModel model)
        {
             model.VoucherDetails.RemoveAll(d => d.AccountId == 0 && d.DebitAmount == 0 && d.CreditAmount == 0);

            if (!model.VoucherDetails.Any())
            {
                model.VoucherDetails.Add(new VoucherDetailViewModel());
            }

            model.TotalDebit = model.VoucherDetails.Sum(d => d.DebitAmount);
            model.TotalCredit = model.VoucherDetails.Sum(d => d.CreditAmount);

            if (model.TotalDebit != model.TotalCredit)
            {
                ModelState.AddModelError("", "Total Debit Amount must be equal to Total Credit Amount.");
            }

            if (!model.VoucherDetails.Any(d => d.AccountId != 0 && (d.DebitAmount > 0 || d.CreditAmount > 0)))
            {
                ModelState.AddModelError("", "Voucher must have at least one valid detail entry (Account selected with a debit or credit amount).");
            }


            if (ModelState.IsValid)
            {
                try
                {
                    long newVoucherId = await _voucherDAL.SaveVoucherAsync(model);
                    if (newVoucherId > 0)
                    {
                        TempData["SuccessMessage"] = $"Voucher (ID: {newVoucherId}) saved successfully!";
                        return RedirectToAction(nameof(Create));                     }
                    else
                    {
                        ModelState.AddModelError("", "Failed to save voucher. Please check inputs and try again.");
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Message.Contains("Total Debit Amount must be equal to Total Credit Amount."))
                    {
                        ModelState.AddModelError("", "Database validation error: Total Debit Amount must be equal to Total Credit Amount.");
                    }
                    else if (ex.Message.Contains("Voucher must have at least one detail entry."))
                    {
                        ModelState.AddModelError("", "Database validation error: Voucher must have at least one detail entry.");
                    }
                    else if (ex.Message.Contains("Account IDs in Voucher Details do not exist"))
                    {
                        ModelState.AddModelError("", "Database validation error: One or more selected accounts are invalid.");
                    }
                    else
                    {
                        ModelState.AddModelError("", $"Database error: {ex.Message}");
                    }
                    Debug.WriteLine($"Error saving voucher: {ex.Message}");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An unexpected error occurred: {ex.Message}");
                    Debug.WriteLine($"Error saving voucher: {ex.Message}");
                }
            }

            await PopulateAccountDropdowns(model.VoucherDetails);
            return View(model);
        }


        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var voucher = await _voucherDAL.GetVoucherByIdAsync(id.Value);
            if (voucher == null)
            {
                return NotFound();
            }

            if (!voucher.VoucherDetails.Any())
            {
                voucher.VoucherDetails.Add(new VoucherDetailViewModel());
            }

            await PopulateAccountDropdowns(voucher.VoucherDetails);
            return View(voucher);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, VoucherViewModel model)
        {
            if (id != model.VoucherId)
            {
                return NotFound();
            }

             model.VoucherDetails.RemoveAll(d => d.AccountId == 0 && d.DebitAmount == 0 && d.CreditAmount == 0);

            if (!model.VoucherDetails.Any())
            {
                model.VoucherDetails.Add(new VoucherDetailViewModel());
            }

            model.TotalDebit = model.VoucherDetails.Sum(d => d.DebitAmount);
            model.TotalCredit = model.VoucherDetails.Sum(d => d.CreditAmount);

            if (model.TotalDebit != model.TotalCredit)
            {
                ModelState.AddModelError("", "Total Debit Amount must be equal to Total Credit Amount.");
            }

            if (!model.VoucherDetails.Any(d => d.AccountId != 0 && (d.DebitAmount > 0 || d.CreditAmount > 0)))
            {
                ModelState.AddModelError("", "Voucher must have at least one valid detail entry (Account selected with a debit or credit amount).");
            }


            if (ModelState.IsValid)
            {
                try
                {
                    long updatedVoucherId = await _voucherDAL.UpdateVoucherAsync(model);
                    if (updatedVoucherId > 0)
                    {
                        TempData["SuccessMessage"] = "Voucher updated successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update voucher. No changes made or voucher not found.");
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Message.Contains("Total Debit Amount must be equal to Total Credit Amount."))
                    {
                        ModelState.AddModelError("", "Database validation error: Total Debit Amount must be equal to Total Credit Amount.");
                    }
                    else if (ex.Message.Contains("Voucher must have at least one detail entry."))
                    {
                        ModelState.AddModelError("", "Database validation error: Voucher must have at least one detail entry.");
                    }
                    else if (ex.Message.Contains("Account IDs in Voucher Details do not exist"))
                    {
                        ModelState.AddModelError("", "Database validation error: One or more selected accounts are invalid.");
                    }
                    else
                    {
                        ModelState.AddModelError("", $"Database error: {ex.Message}");
                    }
                    Debug.WriteLine($"Error updating voucher: {ex.Message}");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An unexpected error occurred: {ex.Message}");
                    Debug.WriteLine($"Error updating voucher: {ex.Message}");
                }
            }

                        await PopulateAccountDropdowns(model.VoucherDetails);
            return View(model);
        }

        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var voucher = await _voucherDAL.GetVoucherByIdAsync(id.Value);
            if (voucher == null)
            {
                return NotFound();
            }

            return View(voucher);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            try
            {
                long deletedVoucherId = await _voucherDAL.DeleteVoucherAsync(id);
                if (deletedVoucherId > 0)
                {
                    TempData["SuccessMessage"] = "Voucher deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete voucher. Voucher not found or has dependencies.";
                }
            }
            catch (SqlException ex)
            {
                TempData["ErrorMessage"] = $"Database error: {ex.Message}";
                Debug.WriteLine($"Error deleting voucher: {ex.Message}");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An unexpected error occurred: {ex.Message}";
                Debug.WriteLine($"Error deleting voucher: {ex.Message}");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

    
