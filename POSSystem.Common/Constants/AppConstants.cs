namespace POSSystem.Common.Constants;

public static class AppConstants
{
    public const string ApplicationName = "POS Pro";
    public const string ApplicationVersion = "1.0.0";
    
    // Default values
    public const decimal DefaultTaxRate = 0.18m;
    public const int DefaultMinStockLevel = 10;
    public const int DefaultMaxStockLevel = 1000;
    
    // Pagination
    public const int DefaultPageSize = 50;
    public const int MaxPageSize = 100;
    
    // Security
    public const int MinPasswordLength = 6;
    public const int MaxLoginAttempts = 5;
    public const int SessionTimeoutMinutes = 30;
}
