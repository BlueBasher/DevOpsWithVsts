namespace DevOpsWithVsts.Web.Controllers
{
    using System;
    using System.Configuration;
    using System.Linq;
    using System.Security.Claims;
    using System.Web;
    using System.Web.Mvc;
    using System.Threading.Tasks;
    using Microsoft.Azure.ActiveDirectory.GraphClient;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.OpenIdConnect;
    using DevOpsWithVsts.Web.Authentication;
    using DevOpsWithVsts.Web.Aad;
    using DevOpsWithVsts.Web.Models;
    using DevOpsWithVsts.Web.FeatureFlag;
    using System.ComponentModel.DataAnnotations;
    using System.Collections.Generic;
    using Microsoft.ApplicationInsights;

    [Authorize]
    public class UserProfileController : Controller
    {
        private readonly IAadClient aadClient;
        private readonly IFeatureFlagManager featureFlagManager;

        public UserProfileController(IAadClient aadClient, IFeatureFlagManager featureFlagManager)
        {
            this.aadClient = aadClient;
            this.featureFlagManager = featureFlagManager;
        }

        // GET: UserProfile
        public async Task<ActionResult> Index()
        {
            try
            {
                var user = await this.aadClient.GetCurrentUser();

                var model = new UserProfileIndexModel
                {
                    DisplayName = user.DisplayName,
                    GivenName = user.GivenName,
                    Surname = user.Surname
                };

                foreach (var featureFlag in Enum.GetValues(typeof(FeatureFlags)).Cast<FeatureFlags>())
                {
                    model.FeatureFlags.Add(
                        typeof(FeatureFlags).GetField(Enum.GetName(typeof(FeatureFlags), featureFlag))
                            .GetCustomAttributes(false)
                            .OfType<DisplayAttribute>()
                            .Single().Name,
                        await featureFlagManager.IsFeatureFlagEnabledForCurrentUser(featureFlag));
                }

                return View(model);
            }
            catch (AdalException)
            {
                // Return to error page.
                return View("Error");
            }
            // if the above failed, the user needs to explicitly re-authenticate for the app to obtain the required token
            catch (Exception)
            {
                return View("Relogin");
            }
        }

        // POST: TodoItems/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(UserProfileIndexModel model)
        {
            if (ModelState.IsValid)
            {
                foreach (var kvp in model.FeatureFlags)
                {
                    var featureFlag = Enum.GetValues(typeof(FeatureFlags)).Cast<FeatureFlags>()
                        .Single(ff => typeof(FeatureFlags).GetField(Enum.GetName(typeof(FeatureFlags), ff))
                            .GetCustomAttributes(false)
                            .OfType<DisplayAttribute>()
                            .Single().Name == kvp.Key);

                    await this.featureFlagManager.SetFeatureFlagForCurrentUser(featureFlag, kvp.Value);
                }
            }

            return RedirectToAction("Index");
        }
    }
}
