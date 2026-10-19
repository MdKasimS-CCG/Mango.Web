using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Mango.Web.Models;
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

            var totalResp = await _rewardsService.GetTotalPointsAsync(userId);
            if (totalResp == null || !totalResp.IsSuccess)
                return Content(string.Empty);

            var historyResp = await _rewardsService.GetRewardsForUserAsync(userId);
            if (historyResp == null || !historyResp.IsSuccess)
                return Content(string.Empty);

            int total = 0;
            try
            {
                total = JsonConvert.DeserializeObject<int>(JsonConvert.SerializeObject(totalResp.Result));
            }
            catch
            {
                total = 0;
            }

            var recent = new List<RewardDto>();
            try
            {
                recent = JsonConvert.DeserializeObject<List<RewardDto>>(JsonConvert.SerializeObject(historyResp.Result)) ?? new List<RewardDto>();
            }
            catch
            {
                recent = new List<RewardDto>();
            }

            // Exclude entries with 0 points to avoid showing irrelevant rows
            var filteredRecent = recent.Where(r => r.PointsEarned > 0)
                                     .OrderByDescending(r => r.RewardsDate)
                                     .Take(5)
                                     .ToList();

            var model = new RewardPointsViewModel
            {
                TotalPoints = total,
                RecentRewards = filteredRecent
            };

            return View("Default", model);
        }
    }
}
