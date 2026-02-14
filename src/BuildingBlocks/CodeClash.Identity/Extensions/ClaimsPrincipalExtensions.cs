using System.Security.Claims;

namespace CodeClash.Identity.Extensions;

public static class ClaimsPrincipalExtensions
{
    extension(ClaimsPrincipal principal)
    {
        public string? GetUserId()
        {
            return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? principal.FindFirst("sub")?.Value;
        }

        public string? GetUsername()
        {
            return principal.FindFirst("preferred_username")?.Value
                   ?? principal.FindFirst(ClaimTypes.Name)?.Value;
        }

        public string? GetEmail()
        {
            return principal.FindFirst(ClaimTypes.Email)?.Value
                   ?? principal.FindFirst("email")?.Value;
        }

        public IEnumerable<string> GetRoles()
        {
            return principal.FindAll(ClaimTypes.Role).Select(c => c.Value);
        }

        public bool HasRole(string role)
        {
            return principal.IsInRole(role);
        }

        public string? GetFirstName()
        {
            return principal.FindFirst(ClaimTypes.GivenName)?.Value
                   ?? principal.FindFirst("given_name")?.Value;
        }

        public string? GetLastName()
        {
            return principal.FindFirst(ClaimTypes.Surname)?.Value
                   ?? principal.FindFirst("family_name")?.Value;
        }

        public string? GetFullName()
        {
            var firstName = principal.GetFirstName();
            var lastName = principal.GetLastName();

            if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
            {
                return $"{firstName} {lastName}";
            }

            return principal.FindFirst("name")?.Value;
        }
    }
}
