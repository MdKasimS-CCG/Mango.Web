using Mango.Web.Models;

namespace Mango.Web.Service.IService
{
    public interface IRewardsService
    {
        Task<ResponseDto?> GetRewardsForUserAsync(string userId, CancellationToken cancellationToken = default);
    }
}
