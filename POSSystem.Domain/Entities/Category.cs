namespace POSSystem.Domain.Entities;

public class Category
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation Properties
    public virtual ICollection<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
}

public class SubCategory
{
    public int SubCatId { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation Properties
    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
