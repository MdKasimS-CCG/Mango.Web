using System.Net;
using System.Text;
using Mango.Web.Models;
using Mango.Web.Service.IService;
using Newtonsoft.Json;
using static Mango.Web.Utility.SD;

namespace Mango.Web.Service
{
    /// <summary>
    /// You Will Notice Projects Are Completely Independent Of 
    /// Other API Projects Or Even UI Project
    /// </summary>
    public class BaseService : IBaseService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITokenProvider _tokenProvider;

        public BaseService(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider)
        {
            _httpClientFactory = httpClientFactory;
            _tokenProvider = tokenProvider;
        }
        public async Task<ResponseDto?> SendAsync(RequestDto requestDto, bool withBearer = true)
        {
            try
            {

                HttpClient client = _httpClientFactory.CreateClient("MangoAPI");
                HttpRequestMessage message = new();
                message.Headers.Add("Accept", "application/json");
                //token
                if(withBearer)
                {
                    var token = _tokenProvider.GetToken();
                    //Don't make any spelling mistake here.
                    message.Headers.Add("Authorization", $"Bearer {token}");
                }

                message.RequestUri = new Uri(requestDto.Url);
                if (requestDto.Data != null)
                {
                    message.Content = new StringContent(JsonConvert.SerializeObject(requestDto.Data)
                                                        , Encoding.UTF8, "application/json");
                }

                HttpResponseMessage? apiResponse = null;


                message.Method = requestDto.ApiType switch
                {
                    ApiType.POST => HttpMethod.Post,
                    ApiType.PUT => HttpMethod.Put,
                    ApiType.DELETE => HttpMethod.Delete,
                    _ => HttpMethod.Get
                };


                //Request is sent here
                apiResponse = await client.SendAsync(message);

                /// Note: 
                /// CouponAPI return ResponseDTO Whereas AuthAPI returens IActionResult
                /// API might return ResponseDto or IActionResult(or its derived child ObjectResult)
                /// Do remember, ObjectResult = IActionResult + Status Code + ResponseDto
                /// (OK/BadRequest/etc.)  (Inheritance)  (int Property) (object? Property)

                //For POST & PUT cases messages are not handled
                switch (apiResponse.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        return new() { IsSuccess = false, Message = "Not Found" };
                    case HttpStatusCode.Forbidden:
                        return new() { IsSuccess = false, Message = "Access Denied" };
                    case HttpStatusCode.Unauthorized:
                        return new() { IsSuccess = false, Message = "Unauthorized" };
                    case HttpStatusCode.InternalServerError:
                        return new() { IsSuccess = false, Message = "Internal Server Error" };
                    default:
                        var apiContent = await apiResponse.Content.ReadAsStringAsync();
                        var apiResponseDto = JsonConvert.DeserializeObject<ResponseDto>(apiContent);
                        return apiResponseDto;
                }
            }
            catch (Exception ex)
            {
                var dto = new ResponseDto
                {
                    Message = ex.Message.ToString(),
                    IsSuccess = false,
                };
                return dto;
            }
        }
    }
}
