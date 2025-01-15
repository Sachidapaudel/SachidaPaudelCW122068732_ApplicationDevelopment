using Microsoft.Extensions.Logging;
using SachidaPaudel.Service.TransactionService;
using SachidaPaudel.Service.UserService;
using SachidaPaudel.Service.DebtsService;
using Microsoft.AspNetCore.Components.Web;
using SachidaPaudel.Utils;
using SachidaPaudel.Service;
using MudBlazor.Services;

namespace SachidaPaudel
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

            builder.Services.AddMudServices();



#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif
            // Registering the service as a singleton to share a single instance across the app
            builder.Services.AddSingleton<IUserService, UserService>();
            builder.Services.AddSingleton<ITransactionService, TransactionService>();
            builder.Services.AddSingleton<IDebtsService, DebtsService>();
            builder.Services.AddSingleton<BalanceService>(); // Register BalanceService

            // Define and register the CSV file paths using environment special folders
            string userFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "user_credentials.csv");
            string transactionFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "transactions.csv");
            string debtFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "debts.csv");
            builder.Services.AddSingleton(new CsvHelper(userFilePath, transactionFilePath, debtFilePath));

            return builder.Build();
        }
    }

    // Service to encapsulate the CSV file path
    public class CsvFilePathService
    {
        public string FilePath { get; }

        public CsvFilePathService(string filePath)
        {
            FilePath = filePath;
        }
    }
}

