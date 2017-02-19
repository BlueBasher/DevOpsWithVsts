using DevOpsWithVsts.Web.FeatureFlag;
using System.Collections.Generic;

namespace DevOpsWithVsts.Web.Models
{
    public class UserProfileIndexModel
    {
        public string DisplayName { get; set; }

        public string GivenName { get; set; }

        public string Surname { get; set; }

        public Dictionary<string, bool> FeatureFlags { get; set; }

        public UserProfileIndexModel()
        {
            FeatureFlags = new Dictionary<string, bool>();
        }
    }
}