using System.Globalization;
using System.Text;
using SachidaPaudel.Models;

namespace SachidaPaudel.Utils
{
    public class CsvHelper
    {
        private readonly string _userFilePath;
        private readonly string _transactionFilePath;
        private readonly string _debtFilePath;

        public CsvHelper(string userFilePath, string transactionFilePath, string debtFilePath)
        {
            _userFilePath = userFilePath;
            _transactionFilePath = transactionFilePath;
            _debtFilePath = debtFilePath;
        }

        // Save user data
        public void SaveUser(User user)
        {
            var lines = new List<string>
            {
                $"{user.Username},{user.Password},{user.Currency}"
            };

            lines.AddRange(user.Transactions.Select(t =>
                $"{t.TransactionTitle},{t.TransactionAmount},{t.TransactionDate.ToString("o", CultureInfo.InvariantCulture)},{t.TransactionTransactionType},{t.Note},{string.Join(";", t.Tags)}"));
            lines.AddRange(user.Debts.Select(d =>
                $"{d.DebtSource},{d.DebtAmount},{d.DebtDueDate.ToString("o", CultureInfo.InvariantCulture)},{d.IsCleared}"));

            // Ensure the directory exists
            var directoryPath = Path.GetDirectoryName(_userFilePath);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            try
            {
                // Open file with FileShare.ReadWrite to allow other apps to read/write
                using (var fileStream = new FileStream(_userFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                using (var streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
                {
                    foreach (var line in lines)
                    {
                        streamWriter.WriteLine(line);
                    }
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error writing to file: {ex.Message}");
                throw;
            }
        }

        // Load user data
        public List<User> LoadUsers()
        {
            if (!File.Exists(_userFilePath))
            {
                // If the file does not exist, return an empty list
                return new List<User>();
            }

            var users = new List<User>();
            try
            {
                // Open file with FileShare.ReadWrite to allow other apps to read/write
                using (var fileStream = new FileStream(_userFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    while (!streamReader.EndOfStream)
                    {
                        var line = streamReader.ReadLine();
                        if (!string.IsNullOrEmpty(line))
                        {
                            var parts = line.Split(',');
                            if (parts.Length >= 3)
                            {
                                var user = new User
                                {
                                    Username = parts[0],
                                    Password = parts[1],
                                    Currency = parts[2]
                                };

                                // Add transactions and debts if available
                                for (int i = 3; i < parts.Length; i++)
                                {
                                    // Add parsing logic for transactions and debts if needed
                                }

                                users.Add(user);
                            }
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error reading from file: {ex.Message}");
                throw;
            }

            return users;
        }

        // Save transaction data
        public void SaveTransaction(Transaction transaction)
        {
            var line = $"{transaction.TransactionTitle},{transaction.TransactionAmount},{transaction.TransactionDate.ToString("o", CultureInfo.InvariantCulture)},{transaction.TransactionTransactionType},{transaction.Note},{string.Join(";", transaction.Tags)}";

            // Ensure the directory exists
            var directoryPath = Path.GetDirectoryName(_transactionFilePath);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            try
            {
                // Open file with FileShare.ReadWrite to allow other apps to read/write
                using (var fileStream = new FileStream(_transactionFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                using (var streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
                {
                    streamWriter.WriteLine(line);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error writing to file: {ex.Message}");
                throw;
            }
        }

        // Load transaction data
        public List<Transaction> LoadTransactions()
        {
            if (!File.Exists(_transactionFilePath))
            {
                // If the file does not exist, return an empty list
                return new List<Transaction>();
            }

            var transactions = new List<Transaction>();
            try
            {
                // Open file with FileShare.ReadWrite to allow other apps to read/write
                using (var fileStream = new FileStream(_transactionFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    while (!streamReader.EndOfStream)
                    {
                        var line = streamReader.ReadLine();
                        if (!string.IsNullOrEmpty(line))
                        {
                            var parts = line.Split(',');
                            if (parts.Length >= 6)
                            {
                                var transaction = new Transaction
                                {
                                    TransactionTitle = parts[0],
                                    TransactionAmount = decimal.Parse(parts[1]),
                                    TransactionDate = DateTime.Parse(parts[2], null, DateTimeStyles.RoundtripKind),
                                    TransactionTransactionType = Enum.Parse<TransactionType>(parts[3]),
                                    Note = parts[4],
                                    Tags = parts[5].Split(';').ToList()
                                };

                                transactions.Add(transaction);
                            }
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error reading from file: {ex.Message}");
                throw;
            }

            return transactions;
        }

        // Save debt data
        public void SaveDebt(Debts debt)
        {
            var line = $"{debt.DebtId},{debt.DebtSource},{debt.DebtAmount},{debt.DebtDueDate.ToString("o", CultureInfo.InvariantCulture)},{debt.IsCleared}";

            // Ensure the directory exists
            var directoryPath = Path.GetDirectoryName(_debtFilePath);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            try
            {
                // Open file with FileShare.ReadWrite to allow other apps to read/write
                using (var fileStream = new FileStream(_debtFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                using (var streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
                {
                    streamWriter.WriteLine(line);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error writing to file: {ex.Message}");
                throw;
            }
        }

        // Load debt data
        public List<Debts> LoadDebts()
        {
            if (!File.Exists(_debtFilePath))
            {
                // If the file does not exist, return an empty list
                return new List<Debts>();
            }

            var debts = new List<Debts>();
            try
            {
                // Open file with FileShare.ReadWrite to allow other apps to read/write
                using (var fileStream = new FileStream(_debtFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    while (!streamReader.EndOfStream)
                    {
                        var line = streamReader.ReadLine();
                        if (!string.IsNullOrEmpty(line))
                        {
                            var parts = line.Split(',');
                            if (parts.Length >= 6)
                            {
                                var debt = new Debts
                                {
                                    DebtId = int.Parse(parts[0]),
                                    DebtSource = parts[2],
                                    DebtAmount = decimal.Parse(parts[3]),
                                    DebtDueDate = DateTime.Parse(parts[4], null, DateTimeStyles.RoundtripKind),
                                    IsCleared = bool.Parse(parts[5])
                                };

                                debts.Add(debt);
                            }
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error reading from file: {ex.Message}");
                throw;
            }

            return debts;
        }
    }
}

