namespace DevOpsWithVsts.Web.Controllers
{
    using DevOpsWithVsts.Web.Authentication;
    using DevOpsWithVsts.Web.FeatureFlag;
    using DevOpsWithVsts.Web.Todo;
    using System.Configuration;
    using System.Net;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize]
    public class HomeController : Controller
    {
        private readonly ITodoStorage todoStorage;
        private readonly IClaimsPrincipalService claimsPrincipalService;
        private readonly IFeatureFlagManager featureFlagManager;

        public HomeController(ITodoStorage todoStorage, IClaimsPrincipalService claimsPrincipalService, IFeatureFlagManager featureFlagManager)
        {
            this.todoStorage = todoStorage;
            this.claimsPrincipalService = claimsPrincipalService;
            this.featureFlagManager = featureFlagManager;
        }

        // GET: TodoItems
        public async Task<ActionResult> Index()
        {
            try
            {
                await this.featureFlagManager.Initialize();
                ViewBag.NewLayoutEnabled = await this.featureFlagManager.IsFeatureFlagEnabledForCurrentUser(FeatureFlags.NewLayout);
            }
            catch (Microsoft.IdentityModel.Clients.ActiveDirectory.AdalSilentTokenAcquisitionException)
            {
                return RedirectToAction("RefreshSession", "Account", new { redirectUri = "/" });
            }

            return View(await this.todoStorage.RetrieveAsync(this.claimsPrincipalService.UserId));
        }

        // GET: Test
        [AllowAnonymous]
        public ActionResult Test()
        {
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        // GET: TodoItems/Details/5
        public async Task<ActionResult> Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var todoItem = await this.todoStorage.RetrieveAsync(this.claimsPrincipalService.UserId, id.Value);
            if (todoItem == null)
            {
                return HttpNotFound();
            }
            return View(todoItem);
        }

        // GET: TodoItems/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TodoItems/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Text,IsClosed")] TodoItem todoItem)
        {
            if (ModelState.IsValid)
            {
                await this.todoStorage.InsertAsync(this.claimsPrincipalService.UserId, todoItem.Text, todoItem.IsClosed);
                return RedirectToAction("Index");
            }

            return View(todoItem);
        }

        // GET: TodoItems/Edit/5
        public async Task<ActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TodoItem todoItem = await this.todoStorage.RetrieveAsync(this.claimsPrincipalService.UserId, id.Value);
            if (todoItem == null)
            {
                return HttpNotFound();
            }
            return View(todoItem);
        }

        // POST: TodoItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,UserId,Text,IsClosed")] TodoItem todoItem)
        {
            if (ModelState.IsValid)
            {
                await this.todoStorage.UpdateAsync(todoItem.UserId, todoItem.Id, todoItem.Text, todoItem.IsClosed);
                return RedirectToAction("Index");
            }
            return View(todoItem);
        }

        // GET: TodoItems/Delete/5
        public async Task<ActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var todoItem = await this.todoStorage.RetrieveAsync(this.claimsPrincipalService.UserId, id.Value);
            if (todoItem == null)
            {
                return HttpNotFound();
            }
            return View(todoItem);
        }

        // POST: TodoItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(long id)
        {
            await this.todoStorage.RemoveAsync(this.claimsPrincipalService.UserId, id);
            return RedirectToAction("Index");
        }
    }
}