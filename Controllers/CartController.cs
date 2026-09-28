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
        [Authorize]
        public async Task<IActionResult> Checkout(CartDto cartDto)
        {
            CartDto cart = await LoadCartDtoBasedOnLoggedInUser();

            // 1. Validate cart
            if (cart.CartHeader == null ||
                cart.CartDetails == null ||
                !cart.CartDetails.Any())
            {
                TempData["error"] = "Your cart is empty.";
                return RedirectToAction(nameof(CartIndex));
            }

            // 2. Update checkout details
            cart.CartHeader.Name = cartDto.CartHeader.Name;
            cart.CartHeader.Email = cartDto.CartHeader.Email;
            cart.CartHeader.Phone = cartDto.CartHeader.Phone;

            // 3. Create order
            var response = await _orderService.CreateOrderAsync(cart);

            if (response == null || !response.IsSuccess)
            {
                TempData["error"] =
                    response?.Message ?? "Unable to create your order.";

                return RedirectToAction(nameof(CartIndex));
            }

            // 4. Deserialize the created order
            OrderHeaderDto? orderHeaderDto =
                JsonConvert.DeserializeObject<OrderHeaderDto>(
                    JsonConvert.SerializeObject(response.Result)
                );

            if (orderHeaderDto == null || orderHeaderDto.Id <= 0)
            {
                TempData["error"] =
                    "Order was submitted, but the order ID could not be retrieved.";

                return RedirectToAction(nameof(CartIndex));
            }

            // 5. Get logged-in user's ID
            var userId = User.Claims
                .FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Sub)
                ?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                TempData["warning"] =
                    "Order was created, but the cart could not be cleared.";

                return RedirectToAction(
                    nameof(Confirmation),
                    new { orderId = orderHeaderDto.Id }
                );
            }

            // 6. Clear cart
            try
            {
                var clearCartResponse =
                    await _cartService.ClearCartAsync(userId);

                if (clearCartResponse == null ||
                    !clearCartResponse.IsSuccess)
                {
                    TempData["warning"] =
                        "Order was created, but your cart could not be cleared.";
                }
            }
            catch (Exception)
            {
                TempData["warning"] =
                    "Order was created, but your cart could not be cleared.";
            }

            // 7. Redirect to confirmation with the actual database order ID
            return RedirectToAction(
                nameof(Confirmation),
                new { orderId = orderHeaderDto.Id }
            );
        }


        //     OrderHeaderDto orderHeaderDto = JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(response.Result));

        //     //TODO: Bug, for same cart, order is being created repeatedly. Once order is placed, cart must be empty.    
        //     if (response != null && response.IsSuccess)
        //     {
        //         //TODO: Stripe code & redirect to place order
        //     }

        //     return View();
        // }

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
