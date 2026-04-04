using CodeClash.Identity.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace CodeClash.Identity.Extensions;

public static class ClaimFilterExtensions
{
    /// <summary>
    /// Adds the PopulateClaimsFilter to automatically populate properties
    /// marked with [FromClaim] or [FromUserId] attributes.
    /// </summary>
    public static IServiceCollection AddClaimPopulation(this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add<PopulateClaimsFilter>();
        });

        return services;
    }
}
