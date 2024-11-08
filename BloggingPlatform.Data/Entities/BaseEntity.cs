namespace BloggingPlatform.Data.Entities;

public abstract class BaseEntity
{
    public required string Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }  = null!;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } 
    public DateTime DeletedDate { get; set; }
    public bool IsActive { get; set; } = false;
}