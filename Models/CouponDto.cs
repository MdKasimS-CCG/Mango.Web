using Newtonsoft.Json;

namespace Mango.Web.Models
{
    public class CouponDto
    {
        //This is needed since Entity model has some other column name
        //Response conatins exact table record matching column names that are in entity model
        //and hence in table. Revealing potential details.
        [JsonProperty("id")]
        public int CouponId { get; set; }
        public string CouponCode { get; set; }
        public double DiscountAmount { get; set; }
        public int MinAmount { get; set; }
    }
}
