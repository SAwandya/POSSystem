using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using POSSystem.Application.Services;
using POSSystem.Infrastructure.Data;
using POSSystem.Infrastructure.Repositories;
using System.IO;
using System.Windows;

namespace POSSystem.UI;

public partial class App : System.Windows.Application
{
    public IServiceProvider ServiceProvider { get; private set; } = null!;
    public IConfiguration Configuration { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            // Load configuration
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();

            // Setup DI container
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();

            // Initialize database
            InitializeDatabase();

            // Show main window
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error starting application: {ex.Message}\n\nPlease ensure:\n1. MySQL server is running\n2. Database connection string in appsettings.json is correct\n3. Database 'pos_system_pro' exists",
                "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Configuration
        services.AddSingleton(Configuration);

        // Database Context
        var connectionString = Configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<POSDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        // Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ISalesRepository, SalesRepository>();

        // Services
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ISalesService, SalesService>();

        // Windows (register as transient so new instances are created)
        services.AddTransient<MainWindow>();
        services.AddTransient<Views.Dashboard.Dashboard>();
        services.AddTransient<Views.Sales.BillingPage>();
        services.AddTransient<Views.inventory.Inventory>();
        services.AddTransient<Views.Stock.StockPage>();
        services.AddTransient<Views.Products.ItemRegistryPage>();
    }

    private void InitializeDatabase()
    {
        using var scope = ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<POSDbContext>();

        try
        {
            // Create database if it doesn't exist
            context.Database.EnsureCreated();

            // Seed initial data
            DbInitializer.SeedDataAsync(context).Wait();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Database initialization error: {ex.Message}\n\nInner Exception: {ex.InnerException?.Message}",
                "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
        base.OnExit(e);
    }
}
