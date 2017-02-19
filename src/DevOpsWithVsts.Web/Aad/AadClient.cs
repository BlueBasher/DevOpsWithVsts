namespace DevOpsWithVsts.Web.Aad
{
    using DevOpsWithVsts.Web.Authentication;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using System.Configuration;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.Azure.ActiveDirectory.GraphClient;
    using System;
    using System.Linq;
    using System.Collections.Generic;

    public class AadClient : IAadClient
    {
        private readonly IClaimsPrincipalService claimsPrincipalService;
        private string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private string appKey = ConfigurationManager.AppSettings["ida:ClientSecret"];
        private string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private string aadDomain = ConfigurationManager.AppSettings["ida:Domain"];
        private string graphResourceID = "https://graph.windows.net";

        public AadClient(IClaimsPrincipalService claimsPrincipalService)
        {
            this.claimsPrincipalService = claimsPrincipalService;
        }

        public async Task<IUser> GetUser(string userId)
        {
            var activeDirectoryClient = GetActiveDirectoryClient();

            var user = await activeDirectoryClient.Users
                .Where(u => u.ObjectId == userId)
                .ExecuteSingleAsync();

            return user;
        }

        public Task<IUser> GetCurrentUser()
        {
            return GetUser(this.claimsPrincipalService.UserId);
        }

        public async Task<IEnumerable<IGroup>> GetGroups()
        {
            var activeDirectoryClient = GetActiveDirectoryClient();

            var result = new List<IGroup>();
            var pagedResult = await activeDirectoryClient.Groups.ExecuteAsync();
            do
            {
                result.AddRange(pagedResult.CurrentPage.ToList());
                pagedResult = await pagedResult.GetNextPageAsync();
            } while (pagedResult != null && pagedResult.MorePagesAvailable);

            return result;
        }

        public async Task<IGroup> GetGroupByName(string groupName)
        {
            var activeDirectoryClient = GetActiveDirectoryClient();

            var group = await activeDirectoryClient.Groups
                .Where(g => g.DisplayName.Equals(groupName))
                .ExecuteSingleAsync();

            return group;
        }

        public Task AddGroup(string groupName)
        {
            var activeDirectoryClient = GetActiveDirectoryClient();

            return activeDirectoryClient.Groups.AddGroupAsync(new Group
            {
                DisplayName = groupName,
                Description = groupName,
                MailNickname = Guid.NewGuid().ToString(),
                MailEnabled = false,
                SecurityEnabled = true
            });
        }

        public Task RemoveGroup(IGroup group)
        {
            return group.DeleteAsync();
        }

        public async Task AddUserToGroup(string userId, string groupId)
        {
            var activeDirectoryClient = GetActiveDirectoryClient();

            var group = (Group)await activeDirectoryClient.Groups
                        .Where(x => x.ObjectId == groupId)
                        .Expand(x => x.Members)
                        .ExecuteSingleAsync();

            var user = (User)await activeDirectoryClient.Users
                .Where(x => x.ObjectId == userId)
                .ExecuteSingleAsync();

            group.Members.Add(user);
            await group.UpdateAsync();
        }

        public async Task RemoveUserFromGroup(string userId, string groupId)
        {
            var activeDirectoryClient = GetActiveDirectoryClient();

            var group = (Group)await activeDirectoryClient.Groups
                        .Where(x => x.ObjectId == groupId)
                        .Expand(x => x.Members)
                        .ExecuteSingleAsync();

            var user = (User)await activeDirectoryClient.Users
                .Where(x => x.ObjectId == userId)
                .ExecuteSingleAsync();

            group.Members.Remove(user);
            await group.UpdateAsync();
        }

        private ActiveDirectoryClient GetActiveDirectoryClient()
        {
            var servicePointUri = new Uri(graphResourceID);
            var serviceRoot = new Uri(servicePointUri, this.claimsPrincipalService.TenantId);
            var activeDirectoryClient = new ActiveDirectoryClient(serviceRoot,
                  async () => await GetTokenForApplication());
            return activeDirectoryClient;
        }

        private async Task<string> GetTokenForApplication()
        {
            var authenticationContext = new AuthenticationContext(aadInstance + aadDomain, false);
            var clientCred = new ClientCredential(clientId, appKey);
            var authenticationResult = await authenticationContext.AcquireTokenAsync(graphResourceID, clientCred);

            return authenticationResult.AccessToken;
        }

        private async Task<string> GetTokenForApplicationAsUser()
        {
            var signedInUserID = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;

            // get a token for the Graph without triggering any user interaction (from the cache, via multi-resource refresh token, etc)
            var clientcred = new ClientCredential(clientId, appKey);
            // initialize AuthenticationContext with the token cache of the currently signed in user, as kept in the app's database
            var authenticationContext = new AuthenticationContext(aadInstance + this.claimsPrincipalService.TenantId, new AdalTokenCache(signedInUserID));
            var authenticationResult = await authenticationContext.AcquireTokenSilentAsync(graphResourceID, clientcred, new UserIdentifier(this.claimsPrincipalService.UserId, UserIdentifierType.UniqueId));
            return authenticationResult.AccessToken;
        }
    }
}