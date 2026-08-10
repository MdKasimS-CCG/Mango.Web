using Mango.Web.Models;

namespace Mango.Web.Service.IService
{
    public interface IOrderService
    {
        Task<ResponseDto?> CreateOrderAsync(CartDto cartDto);
        //Task<ResponseDto?> GetOrderAsync(string couponCode);
        //Task<ResponseDto?> GetAllOrdersAsync();
        //Task<ResponseDto?> GetOrderByIdAsync(int id);
        //Task<ResponseDto?> UpdateOrderAsync(CouponDto couponDto);
        //Task<ResponseDto?> DeleteOrderAsync(int id);

    }
}
