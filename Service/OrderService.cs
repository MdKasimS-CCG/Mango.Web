using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;

namespace Mango.Web.Service
{
    public class OrderService : IOrderService
    {
        private readonly IBaseService _baseService;

        public async Task<ResponseDto?> CreateOrderAsync(CartDto cartDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.POST,
                //TODO: unsual way, tutor has used. Because method is with [HttpPost("CreateOrder")] attribute
                Url = SD.OrderAPIBase + "/api/order/CreateOrder",
                Data = cartDto
            });
        }
        
        public OrderService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDto?> GetAllOrdersAsync()
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.GET,
                Url = SD.CouponAPIBase + "/api/order",
            });
        }

        public async Task<ResponseDto?> GetOrderByIdAsync(int id)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.GET,
                Url = SD.CouponAPIBase + "/api/order/" + id
            });
        }

        public async Task<ResponseDto?> GetOrderAsync(string couponCode)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.GET,
                Url = SD.CouponAPIBase + "/api/order/GetByCode/" + couponCode,
            });
        }

        public async Task<ResponseDto?> UpdateOrderAsync(CouponDto couponDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.PUT,
                Url = SD.CouponAPIBase + "/api/order",
                Data = couponDto
            });
        }

        public async Task<ResponseDto?> DeleteOrderAsync(int id)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.DELETE,
                //URL path not matching hence getting error
                Url = SD.CouponAPIBase + "/api/order/" + id
            });
        }




    }
}