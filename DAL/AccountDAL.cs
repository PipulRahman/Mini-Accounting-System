using System.Data;
using Microsoft.Data.SqlClient;
using MiniAccountingSystem.Models; 
namespace MiniAccountingSystem.DAL
{
    public class AccountDAL
    {
        private readonly string _connectionString;

        public AccountDAL(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<AccountViewModel>> GetAllAccountsAsync()
        {
            var accounts = new List<AccountViewModel>();
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("sp_ManageChartOfAccounts", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@OperationType", "SELECT_ALL");

                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            accounts.Add(MapReaderToAccountViewModel(reader));
                        }
                    }
                }
            }
            return accounts;
        }

        public async Task<AccountViewModel?> GetAccountByIdAsync(int accountId)
        {
            AccountViewModel? account = null;
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("sp_ManageChartOfAccounts", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@OperationType", "SELECT_BY_ID");
                    cmd.Parameters.AddWithValue("@AccountId", accountId);

                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            account = MapReaderToAccountViewModel(reader);
                        }
                    }
                }
            }
            return account;
        }

        public async Task<IEnumerable<AccountViewModel>> GetAccountsForDropdownAsync()
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


        public async Task<int> CreateAccountAsync(AccountViewModel model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("sp_ManageChartOfAccounts", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@OperationType", "CREATE");
                    cmd.Parameters.AddWithValue("@AccountName", model.AccountName);
                    cmd.Parameters.AddWithValue("@AccountCode", (object?)model.AccountCode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ParentAccountId", (object?)model.ParentAccountId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsActive", model.IsActive);

                    await conn.OpenAsync();
                    var result = await cmd.ExecuteScalarAsync();
                    return result != null ? Convert.ToInt32(result) : 0;                 
                }
            }
        }

        public async Task<int> UpdateAccountAsync(AccountViewModel model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("sp_ManageChartOfAccounts", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@OperationType", "UPDATE");
                    cmd.Parameters.AddWithValue("@AccountId", model.AccountId);
                    cmd.Parameters.AddWithValue("@AccountName", model.AccountName);
                    cmd.Parameters.AddWithValue("@AccountCode", (object?)model.AccountCode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ParentAccountId", (object?)model.ParentAccountId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsActive", model.IsActive);

                    await conn.OpenAsync();
                    var result = await cmd.ExecuteScalarAsync();
                    return result != null ? Convert.ToInt32(result) : 0;                 
                }
            }
        }

        public async Task<int> DeleteAccountAsync(int accountId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("sp_ManageChartOfAccounts", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@OperationType", "DELETE");
                    cmd.Parameters.AddWithValue("@AccountId", accountId);

                    await conn.OpenAsync();
                    var result = await cmd.ExecuteScalarAsync();
                    return result != null ? Convert.ToInt32(result) : 0;                 
                }
            }
        }

        private AccountViewModel MapReaderToAccountViewModel(SqlDataReader reader)
        {
            return new AccountViewModel
            {
                AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
                AccountName = reader.GetString(reader.GetOrdinal("AccountName")),
                AccountCode = reader.IsDBNull(reader.GetOrdinal("AccountCode")) ? null : reader.GetString(reader.GetOrdinal("AccountCode")),
                ParentAccountId = reader.IsDBNull(reader.GetOrdinal("ParentAccountId")) ? null : reader.GetInt32(reader.GetOrdinal("ParentAccountId")),
                ParentAccountName = reader.IsDBNull(reader.GetOrdinal("ParentAccountName")) ? null : reader.GetString(reader.GetOrdinal("ParentAccountName")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
            };
        }
    }
}