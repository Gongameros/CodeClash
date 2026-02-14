using System.Security.Claims;

namespace CodeClash.Identity.Attributes;

/// <summary>
/// Marks a property to be automatically populated with the authenticated user's ID.
/// Uses ClaimTypes.NameIdentifier or "sub" claim.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class FromUserIdAttribute : FromClaimAttribute
{
    public FromUserIdAttribute()
    {
        ClaimType = ClaimTypes.NameIdentifier;
        AlternateClaimType = "sub";
        Required = true;
    }
}
