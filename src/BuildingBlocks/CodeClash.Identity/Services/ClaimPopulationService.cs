using System.Reflection;
using System.Security.Claims;
using CodeClash.Identity.Attributes;

namespace CodeClash.Identity.Services;

/// <summary>
/// Service to populate object properties from claims.
/// </summary>
public interface IClaimPopulationService
{
    void PopulateFromClaims(object obj, ClaimsPrincipal user);
}

public class ClaimPopulationService : IClaimPopulationService
{
    public void PopulateFromClaims(object obj, ClaimsPrincipal user)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        if (!user.Identity?.IsAuthenticated ?? true)
            return;

        var type = obj.GetType();
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var fromClaimAttr = property.GetCustomAttribute<FromClaimAttribute>();
            if (fromClaimAttr == null)
                continue;

            var claimType = fromClaimAttr.ClaimType ?? property.Name;
            var claim = user.FindFirst(claimType);

            if (claim == null && !string.IsNullOrEmpty(fromClaimAttr.AlternateClaimType))
            {
                claim = user.FindFirst(fromClaimAttr.AlternateClaimType);
            }

            if (claim == null && fromClaimAttr.Required)
            {
                throw new UnauthorizedAccessException(
                    $"Required claim '{claimType}' not found in user claims for property '{property.Name}' on type '{type.Name}'. " +
                    "User must be authenticated and have the required claim.");
            }

            if (claim != null && property.CanWrite)
            {
                var value = ConvertClaimValue(claim.Value, property.PropertyType);
                property.SetValue(obj, value);
            }
        }
    }

    private static object? ConvertClaimValue(string claimValue, Type targetType)
    {
        if (string.IsNullOrEmpty(claimValue))
            return null;

        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (underlyingType == typeof(string))
            return claimValue;

        if (underlyingType == typeof(Guid))
            return Guid.Parse(claimValue);

        if (underlyingType == typeof(int))
            return int.Parse(claimValue);

        if (underlyingType == typeof(long))
            return long.Parse(claimValue);

        if (underlyingType == typeof(bool))
            return bool.Parse(claimValue);

        if (underlyingType == typeof(DateTime))
            return DateTime.Parse(claimValue);

        if (underlyingType == typeof(DateTimeOffset))
            return DateTimeOffset.Parse(claimValue);

        return Convert.ChangeType(claimValue, underlyingType);
    }
}
