using Mango.Web.Models;

namespace Mango.Web.Service.IService
{
    public interface IRewardsService
    {
        Task<ResponseDto?> GetTotalPointsAsync(string userId);
        Task<ResponseDto?> GetRewardsForUserAsync(string userId);
    }
}
