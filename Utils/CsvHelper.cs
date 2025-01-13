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
                "Username,Password,Currency"
            };

            lines.Add($"{user.Username},{user.Password},{user.Currency}");

            lines.Add("TransactionId,TransactionTitle,TransactionAmount,TransactionDate,TransactionTransactionType,Note,Tags");
            lines.AddRange(user.Transactions.Select(t =>
                $"{t.TransactionId},{t.TransactionTitle},{t.TransactionAmount},{t.TransactionDate.ToString("o", CultureInfo.InvariantCulture)},{t.TransactionTransactionType},{t.Note},{string.Join(";", t.Tags)}"));

            lines.Add("DebtId,DebtSource,DebtAmount,DebtDueDate,IsCleared");
            lines.AddRange(user.Debts.Select(d =>
                $"{d.DebtId},{d.DebtSource},{d.DebtAmount},{d.DebtDueDate.ToString("o", CultureInfo.InvariantCulture)},{d.IsCleared}"));

            // Ensure the directory exists
            var directoryPath = Path.GetDirectoryName(_userFilePath);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            try
            {
                // Open file with FileShare.ReadWrite to allow other apps to read/write
                using (var fileStream = new FileStream(_userFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
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
                    // Skip the header line
                    streamReader.ReadLine();

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
            var transactions = LoadTransactions();
            transaction.TransactionId = GetNextTransactionId(transactions);

            var line = $"{transaction.TransactionId},{transaction.TransactionTitle},{transaction.TransactionAmount},{transaction.TransactionDate.ToString("o", CultureInfo.InvariantCulture)},{transaction.TransactionTransactionType},{transaction.Note},{string.Join(";", transaction.Tags)}";

            // Ensure the directory exists
            var directoryPath = Path.GetDirectoryName(_transactionFilePath);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            try
            {
                // Check if the file exists and add headers if it doesn't
                var fileExists = File.Exists(_transactionFilePath);
                using (var fileStream = new FileStream(_transactionFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                using (var streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
                {
                    if (!fileExists)
                    {
                        streamWriter.WriteLine("TransactionId,TransactionTitle,TransactionAmount,TransactionDate,TransactionTransactionType,Note,Tags");
                    }
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
                    // Skip the header line
                    streamReader.ReadLine();

                    while (!streamReader.EndOfStream)
                    {
                        var line = streamReader.ReadLine();
                        if (!string.IsNullOrEmpty(line))
                        {
                            var parts = line.Split(',');
                            if (parts.Length >= 7)
                            {
                                var transaction = new Transaction
                                {
                                    TransactionId = int.Parse(parts[0]),
                                    TransactionTitle = parts[1],
                                    TransactionAmount = decimal.Parse(parts[2]),
                                    TransactionDate = DateTime.Parse(parts[3], null, DateTimeStyles.RoundtripKind),
                                    TransactionTransactionType = Enum.Parse<TransactionType>(parts[4]),
                                    Note = parts[5],
                                    Tags = parts[6].Split(';').ToList()
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

        // Update transaction data
        public void UpdateTransaction(Transaction transaction)
        {
            if (File.Exists(_transactionFilePath))
            {
                var lines = File.ReadAllLines(_transactionFilePath).ToList();
                for (int i = 1; i < lines.Count; i++) // Start from 1 to skip the header
                {
                    var parts = lines[i].Split(',');
                    if (int.Parse(parts[0]) == transaction.TransactionId)
                    {
                        lines[i] = $"{transaction.TransactionId},{transaction.TransactionTitle},{transaction.TransactionAmount},{transaction.TransactionDate.ToString("o", CultureInfo.InvariantCulture)},{transaction.TransactionTransactionType},{transaction.Note},{string.Join(";", transaction.Tags)}";
                        break;
                    }
                }
                File.WriteAllLines(_transactionFilePath, lines);
            }
        }

        // Delete transaction data
        public void DeleteTransaction(int transactionId)
        {
            if (File.Exists(_transactionFilePath))
            {
                var lines = File.ReadAllLines(_transactionFilePath).ToList();
                var header = lines[0]; // Store the header line
                var dataLines = lines.Skip(1) // Skip the header line
                                     .Where(line => int.Parse(line.Split(',')[0]) != transactionId)
                                     .ToList();
                dataLines.Insert(0, header); // Reinsert the header line at the beginning
                File.WriteAllLines(_transactionFilePath, dataLines);
            }
        }

        // Get the next available transaction ID
        private int GetNextTransactionId(List<Transaction> transactions)
        {
            if (transactions.Count == 0)
            {
                return 1;
            }

            var existingIds = transactions.Select(t => t.TransactionId).ToList();
            existingIds.Sort();

            for (int i = 1; i <= existingIds.Count; i++)
            {
                if (i != existingIds[i - 1])
                {
                    return i;
                }
            }

            return existingIds.Count + 1;
        }

        // Save debt data
        public void SaveDebts(List<Debts> debts)
        {
            var lines = new List<string>
            {
                "DebtId,DebtSource,DebtAmount,DebtDueDate,IsCleared"
            };

            lines.AddRange(debts.Select(d =>
                $"{d.DebtId},{d.DebtSource},{d.DebtAmount},{d.DebtDueDate.ToString("o", CultureInfo.InvariantCulture)},{d.IsCleared}"));

            // Ensure the directory exists
            var directoryPath = Path.GetDirectoryName(_debtFilePath);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            try
            {
                // Open file with FileShare.ReadWrite to allow other apps to read/write
                using (var fileStream = new FileStream(_debtFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
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
                    // Skip the header line
                    streamReader.ReadLine();

                    while (!streamReader.EndOfStream)
                    {
                        var line = streamReader.ReadLine();
                        if (!string.IsNullOrEmpty(line))
                        {
                            var parts = line.Split(',');
                            if (parts.Length >= 5)
                            {
                                var debt = new Debts
                                {
                                    DebtId = int.Parse(parts[0]),
                                    DebtSource = parts[1],
                                    DebtAmount = decimal.Parse(parts[2]),
                                    DebtDueDate = DateTime.Parse(parts[3], null, DateTimeStyles.RoundtripKind),
                                    IsCleared = bool.Parse(parts[4])
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
