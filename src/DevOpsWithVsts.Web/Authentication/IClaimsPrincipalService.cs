namespace DevOpsWithVsts.Web.Authentication
{
    public interface IClaimsPrincipalService
    {
        string UserId { get; }

        string TenantId { get; }
    }
}