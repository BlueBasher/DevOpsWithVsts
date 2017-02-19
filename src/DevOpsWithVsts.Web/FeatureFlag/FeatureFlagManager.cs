namespace DevOpsWithVsts.Web.FeatureFlag
{
    using DevOpsWithVsts.Web.Aad;
    using DevOpsWithVsts.Web.Authentication;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class FeatureFlagManager : IFeatureFlagManager
    {
        private const string AadGroupFeatureFlagPrefix = "FF_";
        private bool isInitialized;
        private object initializeLock = new object();

        private readonly IAadClient aadClient;
        private readonly IClaimsPrincipalService claimsPrincipalService;

        public FeatureFlagManager(IAadClient aadClient, IClaimsPrincipalService claimsPrincipalService)
        {
            this.aadClient = aadClient;
            this.claimsPrincipalService = claimsPrincipalService;
        }

        public async Task Initialize()
        {
            var runInitialization = false;
            lock (initializeLock)
            {
                runInitialization = !isInitialized;
                isInitialized = true;
            }

            if (runInitialization)
            {
                try
                {
                    var featureFlagNames = Enum.GetNames(typeof(FeatureFlags));
                    var existingFeatureFlags = await this.aadClient.GetGroups();

                    // First add any new featureFlag
                    foreach (var newFeatureFlag in
                        featureFlagNames
                            .Where(ff => !existingFeatureFlags.Any(eff => eff.DisplayName == $"{AadGroupFeatureFlagPrefix}{ff}")))
                    {
                        await this.aadClient.AddGroup($"{AadGroupFeatureFlagPrefix}{newFeatureFlag}");
                    }

                    // Remove old featureFlags
                    foreach (var oldFeatureFlag in
                        existingFeatureFlags
                            .Where(eff => !featureFlagNames.Any(ff => $"{AadGroupFeatureFlagPrefix}{ff}" == eff.DisplayName)))
                    {
                        await this.aadClient.RemoveGroup(oldFeatureFlag);
                    }
                }
                catch (AdalException)
                {
                    isInitialized = false;
                }
            }
        }

        public Task<bool> IsFeatureFlagEnabledForCurrentUser(FeatureFlags featureFlag)
        {
            var userId = this.claimsPrincipalService.UserId;
            return IsFeatureFlagEnabled(userId, featureFlag);
        }

        public async Task<bool> IsFeatureFlagEnabled(string userId, FeatureFlags featureFlag)
        {
            var user = await this.aadClient.GetUser(userId);
            var featureFlagGroup = await this.aadClient.GetGroupByName($"{AadGroupFeatureFlagPrefix}{Enum.GetName(typeof(FeatureFlags), featureFlag)}");
            var memberGroups = await user.GetMemberGroupsAsync(false);
            return memberGroups.Any(g => g == featureFlagGroup.ObjectId);
        }

        public Task SetFeatureFlagForCurrentUser(FeatureFlags featureFlag, bool enabled)
        {
            var userId = this.claimsPrincipalService.UserId;
            return SetFeatureFlag(userId, featureFlag, enabled);
        }

        public async Task SetFeatureFlag(string userId, FeatureFlags featureFlag, bool enabled)
        {
            var user = await this.aadClient.GetUser(userId);
            var featureFlagName = Enum.GetName(typeof(FeatureFlags), featureFlag);
            var featureFlagGroup = await this.aadClient.GetGroupByName($"{AadGroupFeatureFlagPrefix}{featureFlagName}");
            var memberGroups = await user.GetMemberGroupsAsync(false);

            if (!enabled && memberGroups.Any(g => g == featureFlagGroup.ObjectId))
            {
                await this.aadClient.RemoveUserFromGroup(userId, featureFlagGroup.ObjectId);
                var ai = new TelemetryClient();
                ai.TrackTrace($"{user.DisplayName} disabled Feature Flag {featureFlagName}", SeverityLevel.Warning);            }

            if (enabled && !memberGroups.Any(g => g == featureFlagGroup.ObjectId))
            {
                await this.aadClient.AddUserToGroup(userId, featureFlagGroup.ObjectId);
                var ai = new TelemetryClient();
                ai.TrackTrace($"{user.DisplayName} enabled Feature Flag {featureFlagName}", SeverityLevel.Information);            }
        }
    }
}