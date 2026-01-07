namespace POSSystem.Common.Helpers;

public static class InvoiceNumberGenerator
{
    public static string GenerateInvoiceNumber(string prefix = "INV")
    {
        var datePart = DateTime.Now.ToString("yyyyMMdd");
        var timePart = DateTime.Now.ToString("HHmmss");
        return $"{prefix}-{datePart}-{timePart}";
    }

    public static string GenerateGRNNumber(string prefix = "GRN")
    {
        var datePart = DateTime.Now.ToString("yyyyMMdd");
        var timePart = DateTime.Now.ToString("HHmmss");
        return $"{prefix}-{datePart}-{timePart}";
    }

    public static string GenerateCustomerCode()
    {
        var random = new Random();
        var number = random.Next(1000, 9999);
        return $"CUST-{number}";
    }

    public static string GenerateSupplierCode()
    {
        var random = new Random();
        var number = random.Next(1000, 9999);
        return $"SUPP-{number}";
    }
}
