using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;

namespace Mango.Web.Service
{
    public class RewardsService : IRewardsService
    {
        private readonly IBaseService _baseService;
        public RewardsService(IBaseService baseService) => _baseService = baseService;

        public async Task<ResponseDto?> GetTotalPointsAsync(string userId)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.GET,
                Url = SD.RewardAPIBase + "/api/rewards/user/" + userId + "/total"
            });
        }

        public async Task<ResponseDto?> GetRewardsForUserAsync(string userId)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.GET,
                Url = SD.RewardAPIBase + "/api/rewards/user/" + userId
            });
        }
    }
}
