using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Motorbike.Data;
using Motorbike.ViewModels;
using System.Text.Json;

namespace Motorbike.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CartSessionKey = "Cart";

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Cart
        public IActionResult Index()
        {
            var cart = GetCartFromSession();
            return View(cart);
        }

        // POST: /Cart/AddToCart - Chỉ giữ lại phương thức này
        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartModel model)
        {
            // Kiểm tra tham số đầu vào
            if (model == null || model.MotorbikeId <= 0 || model.Quantity <= 0)
            {
                return BadRequest(new { success = false, message = "Thông tin không hợp lệ." });
            }

            try
            {
                // Lấy thông tin xe
                var motorbike = await _context.Motorbikes
                    .FirstOrDefaultAsync(m => m.MotorbikeId == model.MotorbikeId);

                if (motorbike == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy sản phẩm." });
                }

                // Lấy giỏ hàng hiện tại
                var cart = GetCartFromSession();

                // Kiểm tra nếu sản phẩm đã có trong giỏ hàng
                var existingItem = cart.CartItems.FirstOrDefault(item => item.MotorbikeId == model.MotorbikeId);
                if (existingItem != null)
                {
                    // Cập nhật số lượng
                    existingItem.Quantity += model.Quantity;
                }
                else
                {
                    // Thêm sản phẩm mới vào giỏ hàng
                    cart.CartItems.Add(new CartItem
                    {
                        MotorbikeId = motorbike.MotorbikeId,
                        MotorbikeName = motorbike.MotorbikeName,
                        Image = motorbike.Image,
                        Price = motorbike.Price,
                        Quantity = model.Quantity
                    });
                }

                // Cập nhật tổng tiền và số lượng
                UpdateCartTotals(cart);

                // Lưu giỏ hàng vào session
                SaveCartToSession(cart);

                return Json(new { success = true, totalItems = cart.TotalItems });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi thêm sản phẩm vào giỏ hàng: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi khi thêm vào giỏ hàng." });
            }
        }

        // POST: /Cart/UpdateQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity([FromBody] UpdateQuantityModel model)
        {
            var cart = GetCartFromSession();
            var item = cart.CartItems.FirstOrDefault(item => item.MotorbikeId == model.MotorbikeId);

            if (item == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy sản phẩm trong giỏ hàng." });
            }

            if (model.Quantity <= 0)
            {
                // Xóa sản phẩm nếu số lượng <= 0
                cart.CartItems.Remove(item);
            }
            else
            {
                // Cập nhật số lượng
                item.Quantity = model.Quantity;
            }

            // Cập nhật tổng tiền và số lượng
            UpdateCartTotals(cart);

            // Lưu giỏ hàng vào session
            SaveCartToSession(cart);

            return Json(new 
            { 
                success = true, 
                itemTotal = item?.Total ?? 0, 
                cartTotal = cart.TotalAmount,
                totalItems = cart.TotalItems
            });
        }

        // POST: /Cart/RemoveFromCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromCart([FromBody] RemoveFromCartModel model)
        {
            var cart = GetCartFromSession();
            var item = cart.CartItems.FirstOrDefault(item => item.MotorbikeId == model.MotorbikeId);

            if (item != null)
            {
                cart.CartItems.Remove(item);
                UpdateCartTotals(cart);
                SaveCartToSession(cart);
            }

            return Json(new { success = true, totalItems = cart.TotalItems });
        }

        // Phương thức lấy giỏ hàng từ session
        private CartViewModel GetCartFromSession()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(cartJson))
            {
                return new CartViewModel();
            }

            try
            {
                return JsonSerializer.Deserialize<CartViewModel>(cartJson);
            }
            catch
            {
                return new CartViewModel();
            }
        }

        // Phương thức lưu giỏ hàng vào session
        private void SaveCartToSession(CartViewModel cart)
        {
            var cartJson = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString("Cart", cartJson);
        }

        // Phương thức cập nhật tổng tiền và số lượng
        private void UpdateCartTotals(CartViewModel cart)
        {
            cart.TotalAmount = cart.CartItems.Sum(item => item.Total);
            cart.TotalItems = cart.CartItems.Sum(item => item.Quantity);
        }
    }

    // Thêm lớp model để binding dữ liệu JSON
    public class AddToCartModel
    {
        public int MotorbikeId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateQuantityModel
    {
        public int MotorbikeId { get; set; }
        public int Quantity { get; set; }
    }

    public class RemoveFromCartModel
    {
        public int MotorbikeId { get; set; }
    }
}