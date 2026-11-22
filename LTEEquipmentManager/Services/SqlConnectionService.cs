//using System;
//using System.Collections.Generic;
//using System.Text;
using Microsoft.Data.SqlClient;

namespace LTEEquipmentManager.Services
{
    public class SqlConnectionService
    {
        public static string BuildConnectionString(
                string server,
                string database,
                string username,
                string password)
        {
            SqlConnectionStringBuilder builder = new ()
            {
                DataSource = server,
                InitialCatalog = database,
                UserID = username,
                Password = password,
                IntegratedSecurity = false,
                Encrypt = true,
                TrustServerCertificate = true
            };

            return builder.ConnectionString;
        }
        public async Task<bool> TestConnectionAsync(string connectionString)
           
        {
                  
            using var connection = new SqlConnection(connectionString);
            
            try
            {
                await connection.OpenAsync();
            }
            catch (SqlException)
            {
                return false;
            }
            return true;
        }
    }
}
