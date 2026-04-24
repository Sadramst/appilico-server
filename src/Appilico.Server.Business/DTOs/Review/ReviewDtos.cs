namespace Appilico.Server.Business.DTOs.Review;

/// <summary>DTO for product review.</summary>
public class ReviewDto
{
    /// <summary>Gets or sets the ID.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the product ID.</summary>
    public Guid ProductId { get; set; }
    /// <summary>Gets or sets the product name.</summary>
    public string? ProductName { get; set; }
    /// <summary>Gets or sets the customer ID.</summary>
    public Guid CustomerId { get; set; }
    /// <summary>Gets or sets the customer name.</summary>
    public string? CustomerName { get; set; }
    /// <summary>Gets or sets the rating.</summary>
    public int Rating { get; set; }
    /// <summary>Gets or sets the title.</summary>
    public string? Title { get; set; }
    /// <summary>Gets or sets the comment.</summary>
    public string? Comment { get; set; }
    /// <summary>Gets or sets whether it's verified.</summary>
    public bool IsVerifiedPurchase { get; set; }
    /// <summary>Gets or sets whether it's approved.</summary>
    public bool IsApproved { get; set; }
    /// <summary>Gets or sets the creation date.</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>DTO for creating a review.</summary>
public class CreateReviewRequest
{
    /// <summary>Gets or sets the product ID.</summary>
    public Guid ProductId { get; set; }
    /// <summary>Gets or sets the rating.</summary>
    public int Rating { get; set; }
    /// <summary>Gets or sets the title.</summary>
    public string? Title { get; set; }
    /// <summary>Gets or sets the comment.</summary>
    public string? Comment { get; set; }
}

/// <summary>DTO for updating a review.</summary>
public class UpdateReviewRequest
{
    /// <summary>Gets or sets the rating.</summary>
    public int Rating { get; set; }
    /// <summary>Gets or sets the title.</summary>
    public string? Title { get; set; }
    /// <summary>Gets or sets the comment.</summary>
    public string? Comment { get; set; }
}
