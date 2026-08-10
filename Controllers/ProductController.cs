using Mango.Web.Models;
using Mango.Web.Service;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Reflection;

namespace Mango.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ResponseDto _response;
        public ProductController(IProductService productService)
        {
            _productService = productService;
            _response = new ResponseDto();
        }
        public async Task<IActionResult> ProductIndex()
        {
            List<ProductDto>? list = new();
            ResponseDto? response = await _productService.GetAllProductsAsync();
            if (response != null && response.IsSuccess)
            {
                //TODO: De Serialization is not working correctly. It did not mapped Ids for CouponId
                list = JsonConvert.DeserializeObject<List<ProductDto>>(Convert.ToString(response.Result));
            }
            else
            {
                TempData["error"] = response?.Message;
            }

            //This View() is connected to ProductIndex.cshtml
            return View(list);
        }
        
        public async Task<IActionResult> ProductCreate()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProductCreate(ProductDto product)
        {
            if (ModelState.IsValid)
            {
                ResponseDto? response = await _productService.CreateProductAsync(product);

                //No use of result field yet.
                if (response != null && response.IsSuccess)
                {
                    TempData["success"] = "Product Created Successfully!";
                    return RedirectToAction(nameof(ProductIndex));
                }
                else
                {
                    TempData["error"] = response?.Message;
                }
            }
            return View(product);
        }

        public async Task<IActionResult> ProductDelete(int productId)
        {
            ResponseDto? response = await _productService.GetProductByIdAsync(productId);

            if (response != null && response.IsSuccess)
            {
                ProductDto? model = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(response.Result));
                return View(model);
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            //We can return 404 view here. For simplicity it is kept as it is
            return NotFound();
        }

        //This is called in ProductDelete view's Delete button
        [HttpPost]
        public async Task<IActionResult> ProductDelete(ProductDto productDto)
        {

            ResponseDto? response = await _productService.DeleteProductAsync(productDto.ProductId);

            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Product Deleted Successfully!";
                return RedirectToAction(nameof(ProductIndex));
            }
            else
            {
                TempData["error"] = response?.Message;
            }

            return View(productDto);
        }

        //TODO: Implement Edit View

        public async Task<IActionResult> ProductEdit(int productId)
        {
            ResponseDto? response = await _productService.GetProductByIdAsync(productId);

            if (response != null && response.IsSuccess)
            {
                ProductDto? model = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(response.Result));
                return View(model);
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            //We can return 404 view here. For simplicity it is kept as it is
            return NotFound();
        }

        //This is called in ProductEdit view's Delete button
        [HttpPost]
        public async Task<IActionResult> ProductEdit(ProductDto productDto)
        {

            ResponseDto? response = await _productService.UpdateProductAsync(productDto);

            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Product Updated Successfully!";
                return RedirectToAction(nameof(ProductIndex));
            }
            else
            {
                TempData["error"] = response?.Message;
            }

            return View(productDto);
        }

    }
}

