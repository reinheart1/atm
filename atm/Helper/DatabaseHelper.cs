using MySql.Data.MySqlClient;
using System;
using System.Data;

public class DatabaseHelper
{
    private string connectionString = "server=localhost;database=ATMDB;user=root;password=;";

    public MySqlConnection GetConnection()
    {
        return new MySqlConnection(connectionString);
    }

    public bool ValidateUser(string cardNumber, string pin)
    {
        using (var conn = GetConnection())
        {
            conn.Open();
            string query = "SELECT COUNT(*) FROM ATMUsers WHERE CardNumber = @CardNumber AND PIN = @PIN";
            using (var cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@CardNumber", cardNumber);
                cmd.Parameters.AddWithValue("@PIN", pin);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }
    }

    public decimal GetBalance(string cardNumber)
    {
        using (var conn = GetConnection())
        {
            conn.Open();
            string query = "SELECT Balance FROM ATMUsers WHERE CardNumber = @CardNumber";
            using (var cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@CardNumber", cardNumber);
                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }
    }

    public void UpdateBalance(string cardNumber, decimal amount, bool isDeposit)
    {
        using (var conn = GetConnection())
        {
            conn.Open();
            string query = isDeposit
                ? "UPDATE ATMUsers SET Balance = Balance + @Amount WHERE CardNumber = @CardNumber"
                : "UPDATE ATMUsers SET Balance = Balance - @Amount WHERE CardNumber = @CardNumber AND Balance >= @Amount";

            using (var cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Amount", amount);
                cmd.Parameters.AddWithValue("@CardNumber", cardNumber);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
