using Mango.Web.Models;
using Mango.Web.Models.Dto;
using Mango.Web.Service.IService;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace Mango.Web.Controllers
{
    public class CartController : Controller
    {
        private ICartService _cartService;
        private IOrderService _orderService;
        public CartController(ICartService cartService, IOrderService orderService)
        {
            _cartService = cartService;
            _orderService = orderService;
        }

        [Authorize]
        public async Task<IActionResult> CartIndex()
        {
            //TODO: Continue shopping is not working

            //TODO: Apply coupon should have dropw down
            return View(await LoadCartDtoBasedOnLoggedInUser());
        }

        public async Task<IActionResult> Remove(int cartDetailsId)
        {
            var userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?
                                    .FirstOrDefault()?.Value;

            ResponseDto? response = await _cartService.RemoveFromCartAsync(cartDetailsId);

            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Cart updated successfully!";
                return RedirectToAction(nameof(CartIndex));
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(CartDto cartDto)
        {
            // TODO: Seems by mistake its here. Might remove completely afetr testing
            //var userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?
            //                        .FirstOrDefault()?.Value;

            ResponseDto? response = await _cartService.ApplyCouponAsync(cartDto);

            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Cart updated successfully!";
                return RedirectToAction(nameof(CartIndex));
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RemoveCoupon(CartDto cartDto)
        {
            // TODO: Shoudl we assign like this?
            cartDto.CartHeader.CouponCode = "";
            ResponseDto? response = await _cartService.ApplyCouponAsync(cartDto);

            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Cart updated successfully!";
                return RedirectToAction(nameof(CartIndex));
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EmailCart(CartDto cartDto)
        {
            //TODO: Seems some bug here. Why cartDto received from Ui doesn't conatins cartDetails? Why we see it as null when message is sent?
            CartDto cart = await LoadCartDtoBasedOnLoggedInUser();

            //TODO: Why are we popluating/constructing cartDto in cart above and passing it to below?
            cart.CartHeader.Email = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Email)?
                                    .FirstOrDefault()?.Value;

            //Note: Tutor is using EmailCart instead EmailCartAsync method name
            ResponseDto? response = await _cartService.EmailCartAsync(cart);

            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Email will be processed and sent shortly!";
                return RedirectToAction(nameof(CartIndex));
            }
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            return View(await LoadCartDtoBasedOnLoggedInUser());
        }

        [HttpPost]
        [ActionName("Checkout")]
        public async Task<IActionResult> Checkout(CartDto cartDto)
        {
            CartDto cart = await LoadCartDtoBasedOnLoggedInUser();
            cart.CartHeader.Name = cartDto.CartHeader.Name;
            cart.CartHeader.Email = cartDto.CartHeader.Email;
            cart.CartHeader.Phone = cartDto.CartHeader.Phone;

            cart.CartHeader.Name = cartDto.CartHeader.Name;

            var response = await _orderService.CreateOrderAsync(cart);

            OrderHeaderDto orderHeaderDto = JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(response.Result));

            //TODO: Bug, for same cart, order is being created repeatedly. Once order is placed, cart must be empty.    
            if (response != null && response.IsSuccess)
            {
                //TODO: Stripe code & redirect to place order
            }

            return View();
        }

        public async Task<IActionResult> Confirmation(int orderId)
        {
            return View(orderId);
        }

        private async Task<CartDto> LoadCartDtoBasedOnLoggedInUser()
        {
            //TODO: Why this is working? How user was accessed?
            var userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?
                                    .FirstOrDefault()?.Value;

            ResponseDto? response = await _cartService.GetCartByUserIdAsync(userId);

            //TODO: Bug - If user don't have any item in cart, for that user CartIndex doesn't load
            if (response != null && response.IsSuccess)
            {
                //TODO: For tutor, this line is not giving exception for user with 0 items in cart.
                CartDto cartDto = JsonConvert.DeserializeObject<CartDto>(Convert.ToString(response.Result));
                return cartDto;
            }
            return new CartDto();
        }

    }
}
