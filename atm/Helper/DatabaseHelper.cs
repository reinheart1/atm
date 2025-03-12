using MySql.Data.MySqlClient;
using System;
using System.Transactions;

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

    public void UpdatePin(string cardNumber, string newPin)
    {
        using (var conn = GetConnection())
        {
            conn.Open();
            string query = "UPDATE ATMUsers SET PIN = @NewPin WHERE CardNumber = @CardNumber";

            using (var cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@NewPin", newPin);
                cmd.Parameters.AddWithValue("@CardNumber", cardNumber);
                cmd.ExecuteNonQuery();
            }
        }
    }

    public bool UserExists(string cardNumber)
    {
        using (var conn = GetConnection())
        {
            conn.Open();
            string query = "SELECT COUNT(*) FROM ATMUsers WHERE CardNumber = @CardNumber";

            using (var cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@CardNumber", cardNumber);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }
    }

    public void TransferFunds(string senderCardNumber, string recipientCardNumber, decimal amount)
    {
        using (var conn = GetConnection())
        {
            conn.Open();
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    // Deduct from sender
                    string deductQuery = "UPDATE ATMUsers SET Balance = Balance - @Amount WHERE CardNumber = @SenderCardNumber";
                    using (var deductCmd = new MySqlCommand(deductQuery, conn, transaction))
                    {
                        deductCmd.Parameters.AddWithValue("@Amount", amount);
                        deductCmd.Parameters.AddWithValue("@SenderCardNumber", senderCardNumber);
                        deductCmd.ExecuteNonQuery();
                    }

                    // Add to recipient
                    string addQuery = "UPDATE ATMUsers SET Balance = Balance + @Amount WHERE CardNumber = @RecipientCardNumber";
                    using (var addCmd = new MySqlCommand(addQuery, conn, transaction))
                    {
                        addCmd.Parameters.AddWithValue("@Amount", amount);
                        addCmd.Parameters.AddWithValue("@RecipientCardNumber", recipientCardNumber);
                        addCmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }

    public void RecordTransaction(string cardNumber, string transactionType, decimal amount, string recipientCardNumber = null)
    {
        using (var conn = GetConnection())
        {
            conn.Open();
            string query = "INSERT INTO Transactions (CardNumber, TransactionType, Amount, RecipientCardNumber) VALUES (@CardNumber, @TransactionType, @Amount, @RecipientCardNumber)";

            using (var cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@CardNumber", cardNumber);
                cmd.Parameters.AddWithValue("@TransactionType", transactionType);
                cmd.Parameters.AddWithValue("@Amount", amount);
                cmd.Parameters.AddWithValue("@RecipientCardNumber", (object)recipientCardNumber ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }
    }

    public List<Transaction> GetTransactions(string cardNumber)
    {
        List<Transaction> transactions = new List<Transaction>();

        using (var conn = GetConnection())
        {
            conn.Open();
            string query = "SELECT TransactionType, Amount, RecipientCardNumber, TransactionDate FROM Transactions WHERE CardNumber = @CardNumber ORDER BY TransactionDate DESC";

            using (var cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@CardNumber", cardNumber);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        transactions.Add(new Transaction
                        {
                            TransactionType = reader.GetString("TransactionType"),
                            Amount = reader.GetDecimal("Amount"),
                            RecipientCardNumber = reader.IsDBNull(2) ? null : reader.GetString("RecipientCardNumber"),
                            TransactionDate = reader.GetDateTime("TransactionDate")
                        });
                    }
                }
            }
        }

        return transactions;
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

                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected == 0 && !isDeposit)
                {
                    throw new Exception("Insufficient balance or invalid transaction.");
                }
            }
        }

    }
}
