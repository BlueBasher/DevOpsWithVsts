using System.Threading.Tasks;

namespace DevOpsWithVsts.Web.FeatureFlag
{
    public interface IFeatureFlagManager
    {
        Task Initialize();
        Task<bool> IsFeatureFlagEnabled(string userId, FeatureFlags featureFlag);
        Task<bool> IsFeatureFlagEnabledForCurrentUser(FeatureFlags featureFlag);
        Task SetFeatureFlag(string userId, FeatureFlags featureFlag, bool enabled);
        Task SetFeatureFlagForCurrentUser(FeatureFlags featureFlag, bool enabled);
    }
}