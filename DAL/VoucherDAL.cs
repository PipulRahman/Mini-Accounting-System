using System.Data;
using Microsoft.Data.SqlClient; 
using MiniAccountingSystem.Models; 
namespace MiniAccountingSystem.DAL
{
    public class VoucherDAL
    {
        private readonly string _connectionString;

        public VoucherDAL(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<long> SaveVoucherAsync(VoucherViewModel model)
        {
            long newVoucherId = 0;

            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("sp_SaveVoucher", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@VoucherDate", model.VoucherDate);
                    cmd.Parameters.AddWithValue("@VoucherType", model.VoucherType);
                    cmd.Parameters.AddWithValue("@ReferenceNo", (object?)model.ReferenceNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Narration", (object?)model.Narration ?? DBNull.Value);

                    DataTable voucherDetailsTable = new DataTable();
                    voucherDetailsTable.Columns.Add("AccountId", typeof(int));
                    voucherDetailsTable.Columns.Add("DebitAmount", typeof(decimal));
                    voucherDetailsTable.Columns.Add("CreditAmount", typeof(decimal));

                    foreach (var detail in model.VoucherDetails)
                    {
                        if (detail.AccountId != 0 || detail.DebitAmount > 0 || detail.CreditAmount > 0)
                        {
                            voucherDetailsTable.Rows.Add(
                                detail.AccountId,
                                detail.DebitAmount,
                                detail.CreditAmount
                            );
                        }
                    }

                    SqlParameter tvpParam = cmd.Parameters.AddWithValue("@VoucherDetails", voucherDetailsTable);
                    tvpParam.SqlDbType = SqlDbType.Structured;                     
                    tvpParam.TypeName = "VoucherDetailType";   
                    await conn.OpenAsync();
                    var result = await cmd.ExecuteScalarAsync(); 
                    if (result != null && result != DBNull.Value)
                    {
                        newVoucherId = Convert.ToInt64(result);
                    }
                }
            }
            return newVoucherId;
        }

        public async Task<IEnumerable<AccountViewModel>> GetAccountsForVoucherDropdownAsync()
        {
            var accounts = new List<AccountViewModel>();
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("sp_ManageChartOfAccounts", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@OperationType", "SELECT_FOR_DROPDOWN");

                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            accounts.Add(new AccountViewModel
                            {
                                AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
                                AccountName = reader.GetString(reader.GetOrdinal("AccountDisplay"))
                            });
                        }
                    }
                }
            }
            return accounts;
        }

        public async Task<IEnumerable<VoucherViewModel>> GetAllVouchersAsync()
        {
            var vouchers = new List<VoucherViewModel>();
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("SELECT VoucherId, VoucherDate, VoucherType, ReferenceNo, Narration, CreatedAt FROM VoucherMaster ORDER BY VoucherDate DESC, VoucherId DESC", conn))
                {
                    cmd.CommandType = CommandType.Text; 
                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            vouchers.Add(new VoucherViewModel
                            {
                                VoucherId = reader.GetInt64(reader.GetOrdinal("VoucherId")),
                                VoucherDate = reader.GetDateTime(reader.GetOrdinal("VoucherDate")),
                                VoucherType = reader.GetString(reader.GetOrdinal("VoucherType")),
                                ReferenceNo = reader.IsDBNull(reader.GetOrdinal("ReferenceNo")) ? null : reader.GetString(reader.GetOrdinal("ReferenceNo")),
                                Narration = reader.IsDBNull(reader.GetOrdinal("Narration")) ? null : reader.GetString(reader.GetOrdinal("Narration")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                            });
                        }
                    }
                }
            }
            return vouchers;
        }

        public async Task<VoucherViewModel?> GetVoucherByIdAsync(long voucherId)
        {
            VoucherViewModel? voucher = null;
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("sp_GetVoucherById", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VoucherId", voucherId);

                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            voucher = new VoucherViewModel
                            {
                                VoucherId = reader.GetInt64(reader.GetOrdinal("VoucherId")),
                                VoucherDate = reader.GetDateTime(reader.GetOrdinal("VoucherDate")),
                                VoucherType = reader.GetString(reader.GetOrdinal("VoucherType")),
                                ReferenceNo = reader.IsDBNull(reader.GetOrdinal("ReferenceNo")) ? null : reader.GetString(reader.GetOrdinal("ReferenceNo")),
                                Narration = reader.IsDBNull(reader.GetOrdinal("Narration")) ? null : reader.GetString(reader.GetOrdinal("Narration")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                            };
                        }

                        if (voucher != null)
                        {
                            await reader.NextResultAsync();
                            while (await reader.ReadAsync())
                            {
                                voucher.VoucherDetails.Add(new VoucherDetailViewModel
                                {
                                    VoucherDetailId = reader.GetInt64(reader.GetOrdinal("VoucherDetailId")),
                                    VoucherId = reader.GetInt64(reader.GetOrdinal("VoucherId")),
                                    AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
                                    AccountName = reader.GetString(reader.GetOrdinal("AccountName")),                                     DebitAmount = reader.GetDecimal(reader.GetOrdinal("DebitAmount")),
                                    CreditAmount = reader.GetDecimal(reader.GetOrdinal("CreditAmount"))
                                });
                            }

                            voucher.TotalDebit = voucher.VoucherDetails.Sum(d => d.DebitAmount);
                            voucher.TotalCredit = voucher.VoucherDetails.Sum(d => d.CreditAmount);
                        }
                    }
                }
            }
            return voucher;
        }

        public async Task<long> UpdateVoucherAsync(VoucherViewModel model)
        {
            long updatedVoucherId = 0;

            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("sp_UpdateVoucher", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@VoucherId", model.VoucherId);
                    cmd.Parameters.AddWithValue("@VoucherDate", model.VoucherDate);
                    cmd.Parameters.AddWithValue("@VoucherType", model.VoucherType);
                    cmd.Parameters.AddWithValue("@ReferenceNo", (object?)model.ReferenceNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Narration", (object?)model.Narration ?? DBNull.Value);

                    DataTable voucherDetailsTable = new DataTable();
                    voucherDetailsTable.Columns.Add("AccountId", typeof(int));
                    voucherDetailsTable.Columns.Add("DebitAmount", typeof(decimal));
                    voucherDetailsTable.Columns.Add("CreditAmount", typeof(decimal));

                    foreach (var detail in model.VoucherDetails)
                    {
                        if (detail.AccountId != 0 || detail.DebitAmount > 0 || detail.CreditAmount > 0)
                        {
                            voucherDetailsTable.Rows.Add(
                                detail.AccountId,
                                detail.DebitAmount,
                                detail.CreditAmount
                            );
                        }
                    }

                    SqlParameter tvpParam = cmd.Parameters.AddWithValue("@VoucherDetails", voucherDetailsTable);
                    tvpParam.SqlDbType = SqlDbType.Structured;
                    tvpParam.TypeName = "VoucherDetailType";

                    await conn.OpenAsync();
                    var result = await cmd.ExecuteScalarAsync();

                    if (result != null && result != DBNull.Value)
                    {
                        updatedVoucherId = Convert.ToInt64(result);
                    }
                }
            }
            return updatedVoucherId;
        }

        public async Task<long> DeleteVoucherAsync(long voucherId)
        {
            long deletedVoucherId = 0;
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("sp_DeleteVoucher", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VoucherId", voucherId);

                    await conn.OpenAsync();
                    var result = await cmd.ExecuteScalarAsync();
                    if (result != null && result != DBNull.Value)
                    {
                        deletedVoucherId = Convert.ToInt64(result);
                    }
                }
            }
            return deletedVoucherId;
        }
    }
}