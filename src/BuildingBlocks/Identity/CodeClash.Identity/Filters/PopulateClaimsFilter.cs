using System.Reflection;
using System.Security.Claims;
using CodeClash.Identity.Attributes;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CodeClash.Identity.Filters;

/// <summary>
/// Action filter that automatically populates properties marked with [FromClaim] or [FromUserId]
/// from the authenticated user's claims.
/// </summary>
public class PopulateClaimsFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            return; // Skip if user is not authenticated
        }

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument == null)
            {
                continue;
            }

            PopulateClaimsInObject(argument, user);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Nothing to do after action execution
    }

    private static void PopulateClaimsInObject(object obj, ClaimsPrincipal user)
    {
        var type = obj.GetType();
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            // Check for FromClaim attribute
            var fromClaimAttr = property.GetCustomAttribute<FromClaimAttribute>();
            if (fromClaimAttr == null)
            {
                continue;
            }

            // Determine which claim type to use
            var claimType = fromClaimAttr.ClaimType ?? property.Name;

            // Try to find the claim
            var claim = user.FindFirst(claimType);

            // If not found and there's an alternate, try that
            if (claim == null && !string.IsNullOrEmpty(fromClaimAttr.AlternateClaimType))
            {
                claim = user.FindFirst(fromClaimAttr.AlternateClaimType);
            }

            // If required and not found, throw exception
            if (claim == null && fromClaimAttr.Required)
            {
                throw new InvalidOperationException(
                    $"Required claim '{claimType}' not found in user claims for property '{property.Name}' on type '{type.Name}'");
            }

            // If found, set the property value
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
        {
            return null;
        }

        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        // Convert based on type
        if (underlyingType == typeof(string))
        {
            return claimValue;
        }

        if (underlyingType == typeof(Guid))
        {
            return Guid.Parse(claimValue);
        }

        if (underlyingType == typeof(int))
        {
            return int.Parse(claimValue);
        }

        if (underlyingType == typeof(long))
        {
            return long.Parse(claimValue);
        }

        if (underlyingType == typeof(bool))
        {
            return bool.Parse(claimValue);
        }

        // For other types, try Convert.ChangeType
        return Convert.ChangeType(claimValue, underlyingType);
    }
}
