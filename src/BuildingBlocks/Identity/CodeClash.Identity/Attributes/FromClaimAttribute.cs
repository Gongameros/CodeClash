namespace CodeClash.Identity.Attributes;

/// <summary>
/// Marks a property to be automatically populated from a user claim.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class FromClaimAttribute : Attribute
{
    /// <summary>
    /// The claim type to extract the value from.
    /// If not specified, uses the property name.
    /// </summary>
    public string? ClaimType { get; set; }

    /// <summary>
    /// Alternate claim type to try if the primary claim is not found.
    /// </summary>
    public string? AlternateClaimType { get; set; }

    /// <summary>
    /// Whether this claim is required. If true and claim is not found, throws an exception.
    /// </summary>
    public bool Required { get; set; } = true;

    public FromClaimAttribute()
    {
    }

    public FromClaimAttribute(string claimType)
    {
        ClaimType = claimType;
    }
}
