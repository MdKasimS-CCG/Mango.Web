using System.Collections.Generic;

namespace Mango.Web.Models
{
    public class RewardPointsViewModel
    {
        public int TotalPoints { get; set; }
        public List<RewardDto> RecentRewards { get; set; } = new();
    }
}
