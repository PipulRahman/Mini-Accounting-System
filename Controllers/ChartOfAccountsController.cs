using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; using Microsoft.Data.SqlClient;
using MiniAccountingSystem.DAL;
using MiniAccountingSystem.Models;
using System.Diagnostics; 
namespace MiniAccountingSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ChartOfAccountsController : Controller
    {
        private readonly AccountDAL _accountDAL;

        public ChartOfAccountsController(AccountDAL accountDAL)
        {
            _accountDAL = accountDAL;
        }

        public async Task<IActionResult> Index()
        {
            var accounts = await _accountDAL.GetAllAccountsAsync();

                                    
            return View(accounts);
        }

        public async Task<IActionResult> Create()
        {
            var model = new AccountViewModel();
            await PopulateParentAccounts(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int newAccountId = await _accountDAL.CreateAccountAsync(model);
                    if (newAccountId > 0)
                    {
                        TempData["SuccessMessage"] = "Account created successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create account. Check logs for details.");
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627 || ex.Number == 2601)                     
                    {
                        ModelState.AddModelError("AccountCode", "An account with this code already exists.");
                    }
                    else
                    {
                        ModelState.AddModelError("", $"Database error: {ex.Message}");
                    }
                    Debug.WriteLine($"Error creating account: {ex.Message}");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An unexpected error occurred: {ex.Message}");
                    Debug.WriteLine($"Error creating account: {ex.Message}");
                }
            }
            await PopulateParentAccounts(model);             
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _accountDAL.GetAccountByIdAsync(id.Value);
            if (account == null)
            {
                return NotFound();
            }

            await PopulateParentAccounts(account);
            return View(account);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AccountViewModel model)
        {
            if (id != model.AccountId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    int updatedAccountId = await _accountDAL.UpdateAccountAsync(model);
                    if (updatedAccountId > 0)
                    {
                        TempData["SuccessMessage"] = "Account updated successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update account. No changes made or account not found.");
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627 || ex.Number == 2601)
                    {
                        ModelState.AddModelError("AccountCode", "An account with this code already exists.");
                    }
                    else if (ex.Message.Contains("cannot be its own parent"))                     {
                        ModelState.AddModelError("ParentAccountId", ex.Message);
                    }
                    else
                    {
                        ModelState.AddModelError("", $"Database error: {ex.Message}");
                    }
                    Debug.WriteLine($"Error updating account: {ex.Message}");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An unexpected error occurred: {ex.Message}");
                    Debug.WriteLine($"Error updating account: {ex.Message}");
                }
            }
            await PopulateParentAccounts(model);             return View(model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _accountDAL.GetAccountByIdAsync(id.Value);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                int deletedAccountId = await _accountDAL.DeleteAccountAsync(id);
                if (deletedAccountId > 0)
                {
                    TempData["SuccessMessage"] = "Account deleted successfully!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete account. Account not found or has dependencies.";
                }
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("has child accounts") || ex.Message.Contains("used in existing voucher entries"))
                {
                    TempData["ErrorMessage"] = ex.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = $"Database error: {ex.Message}";
                }
                Debug.WriteLine($"Error deleting account: {ex.Message}");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An unexpected error occurred: {ex.Message}";
                Debug.WriteLine($"Error deleting account: {ex.Message}");
            }

            return RedirectToAction(nameof(Index));
        }


        private async Task PopulateParentAccounts(AccountViewModel model)
        {
            var parentAccounts = await _accountDAL.GetAccountsForDropdownAsync();
            model.AvailableParentAccounts = new SelectList(parentAccounts, "AccountId", "AccountName");
        }
    }
}