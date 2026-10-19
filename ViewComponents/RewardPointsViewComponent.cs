using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Mango.Web.ViewComponents
{
    public class RewardPointsViewComponent : ViewComponent
    {
        private readonly IRewardsService _rewardsService;

        public RewardPointsViewComponent(IRewardsService rewardsService)
        {
            _rewardsService = rewardsService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = ViewContext?.HttpContext?.User;
            if (user == null || !user.Identity.IsAuthenticated)
                return Content(string.Empty);

            // Try common claim types for subject / user id
            var userId = user.Claims.FirstOrDefault(c => c.Type == "sub")?.Value
                         ?? user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
                return Content(string.Empty);

            var resp = await _rewardsService.GetTotalPointsAsync(userId);
            if (resp == null || !resp.IsSuccess)
                return Content(string.Empty);

            // Result is expected to be a numeric total
            var total = resp.Result?.ToString() ?? "0";

            return View("Default", total);
        }
    }
}
