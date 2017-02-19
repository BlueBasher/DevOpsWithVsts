namespace DevOpsWithVsts.Web.Authentication
{
    using System.Security.Claims;

    public class ClaimsPrincipalService : IClaimsPrincipalService
    {
        public string UserId
        {
            get
            {
                var idClaim = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier");
                if (idClaim != null)
                {
                    return idClaim.Value;
                }
                return string.Empty;
            }
        }

        public string TenantId
        {
            get
            {
                var tenantIdClaim = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid");
                if (tenantIdClaim != null)
                {
                    return tenantIdClaim.Value;
                }
                return string.Empty;
            }
        }
    }
}