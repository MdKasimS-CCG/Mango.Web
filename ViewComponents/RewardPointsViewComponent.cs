using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Mango.Web.Models;
using System.Security.Claims;

namespace Mango.Web.ViewComponents
{
    /// Rendered from _Layout on every page, so it must fail soft: a slow or
    /// down RewardAPI renders nothing instead of holding up the page.
    public class RewardPointsViewComponent : ViewComponent
    {
        private static readonly TimeSpan RewardsTimeout = TimeSpan.FromSeconds(2);
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

        // Failures are cached briefly so an outage doesn't add the timeout to every page.
        private static readonly TimeSpan FailureCacheDuration = TimeSpan.FromSeconds(30);

        private readonly IRewardsService _rewardsService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<RewardPointsViewComponent> _logger;

        public RewardPointsViewComponent(IRewardsService rewardsService
                                        , IMemoryCache cache
                                        , ILogger<RewardPointsViewComponent> logger)
        {
            _rewardsService = rewardsService;
            _cache = cache;
            _logger = logger;
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

            var cacheKey = $"rewards:{userId}";
            if (!_cache.TryGetValue(cacheKey, out RewardPointsViewModel? model))
            {
                model = await LoadRewardsAsync(userId);
                _cache.Set(cacheKey, model, model == null ? FailureCacheDuration : CacheDuration);
            }

            if (model == null)
                return Content(string.Empty);

            return View("Default", model);
        }

        private async Task<RewardPointsViewModel?> LoadRewardsAsync(string userId)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(HttpContext.RequestAborted);
            cts.CancelAfter(RewardsTimeout);

            // BaseService turns timeouts and connection errors into IsSuccess = false.
            var historyResp = await _rewardsService.GetRewardsForUserAsync(userId, cts.Token);
            if (historyResp == null || !historyResp.IsSuccess)
            {
                _logger.LogWarning(
                    "Reward points unavailable for user {UserId}: {Message}",
                    userId, historyResp?.Message);
                return null;
            }

            List<RewardDto> history;
            try
            {
                history = JsonConvert.DeserializeObject<List<RewardDto>>(JsonConvert.SerializeObject(historyResp.Result)) ?? new List<RewardDto>();
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Reward history for user {UserId} could not be read.", userId);
                return null;
            }

            return new RewardPointsViewModel
            {
                // The history endpoint returns every reward row, so the total
                // is derived here instead of a second call to /total.
                TotalPoints = history.Sum(r => r.PointsEarned),

                // Exclude entries with 0 points to avoid showing irrelevant rows
                RecentRewards = history.Where(r => r.PointsEarned > 0)
                                       .OrderByDescending(r => r.RewardsDate)
                                       .Take(5)
                                       .ToList()
            };
        }
    }
}
