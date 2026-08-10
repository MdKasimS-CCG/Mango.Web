using Mango.Web.Utility;
using static Mango.Web.Utility.SD;

namespace Mango.Web.Models
{
    public class RequestDto
    {
        public string Url { get; set; }
        public ApiType ApiType { get; set; } = ApiType.GET; 
        public string AccessToken { get; set; }
        public object? Data { get; set; } // Nullable to allow for no data in some requests
    }
}
