namespace DevOpsWithVsts.Web.FeatureFlag
{
    using System.ComponentModel.DataAnnotations;

    public enum FeatureFlags
    {
        [Display(Name = "New layout")]
        NewLayout
    }
}