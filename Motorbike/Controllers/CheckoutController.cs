using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Motorbike.Data;
using Motorbike.Models;
using Motorbike.ViewModels;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Motorbike.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CartSessionKey = "Cart";

        public CheckoutController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Lấy giỏ hàng từ session
            var cart = GetCartFromSession();
            if (cart == null || cart.CartItems.Count == 0)
            {
                return RedirectToAction("Index", "Cart");
            }

            // Tạo model cho view thanh toán
            var model = new CheckoutViewModel
            {
                Items = cart.CartItems,
                TotalAmount = cart.TotalAmount
            };

            // Nếu người dùng đã đăng nhập, điền sẵn thông tin
            if (HttpContext.Session.GetInt32("AccountId") is int userId)
            {
                var user = _context.Accounts.FirstOrDefault(a => a.AccountId == userId);
                if (user != null)
                {
                    model.CustomerName = user.Username;
                    model.Phone = user.Phone;
                    model.Address = user.Address;
                    model.AccountId = user.AccountId;
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            try
            {
                // Kiểm tra session tồn tại không
                var cartJson = HttpContext.Session.GetString(CartSessionKey);
                Console.WriteLine($"Cart JSON: {cartJson}");
                
                // Kiểm tra model binding
                Console.WriteLine($"Received model - Name: {model.CustomerName}, Address: {model.Address}, Phone: {model.Phone}");
                
                var cart = GetCartFromSession();
                if (cart == null || cart.CartItems.Count == 0)
                {
                    ModelState.AddModelError("", "Giỏ hàng của bạn đã hết hạn hoặc trống");
                    return View("Index", model);
                }

                // Bổ sung Items từ cart vào model
                model.Items = cart.CartItems;
                model.TotalAmount = cart.TotalAmount;

                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState.Values
                        .SelectMany(x => x.Errors)
                        .Select(x => x.ErrorMessage));
                    Console.WriteLine($"Validation errors: {errors}");

                    return View("Index", model);
                }

                // Kiểm tra tồn kho
                var stockValidation = await ValidateStockQuantity(cart);
                if (!stockValidation.IsValid)
                {
                    ViewBag.OutOfStockItems = stockValidation.InvalidItems;
                    ModelState.AddModelError("", "Số lượng sản phẩm vượt quá tồn kho hiện có");
                    return View("Index", model);
                }

                // Tách logic tạo đơn hàng ra thành method riêng
                var orderId = await CreateOrder(model, cart);
                
                if (orderId > 0)
                {
                    // Xóa giỏ hàng sau khi đặt hàng thành công
                    //HttpContext.Session.Remove(CartSessionKey);

                    // Thay vì xóa, gán giỏ hàng rỗng vào session
                    HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(new CartViewModel()));

                    // Đảm bảo giữ lại thông tin đăng nhập
                    int? existingAccountId = HttpContext.Session.GetInt32("AccountId");
                    var username = HttpContext.Session.GetString("Username");
                    var role = HttpContext.Session.GetString("Role");

                    if (existingAccountId.HasValue && !string.IsNullOrEmpty(username))
                    {
                        // Refresh session để đảm bảo thông tin đăng nhập không bị mất
                        HttpContext.Session.SetInt32("AccountId", existingAccountId.Value);
                        HttpContext.Session.SetString("Username", username);
                        if (!string.IsNullOrEmpty(role))
                            HttpContext.Session.SetString("Role", role);
                    }

                    // Chuyển đến trang xác nhận đơn hàng
                    return RedirectToAction("OrderConfirmation", new { orderId = orderId });
                }
                else
                {
                    ModelState.AddModelError("", "Không thể lưu đơn hàng, vui lòng thử lại sau");
                    return View("Index", model);
                }
            }
            catch (Exception ex)
            {
                // Log chi tiết hơn
                Console.WriteLine($"Exception type: {ex.GetType().Name}");
                Console.WriteLine($"Exception message: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                
                ModelState.AddModelError("", $"Lỗi: {ex.Message}");
                
                // Đảm bảo có giỏ hàng
                var cart = GetCartFromSession();
                if (cart != null && cart.CartItems.Count > 0)
                {
                    model.Items = cart.CartItems;
                    model.TotalAmount = cart.TotalAmount;
                }
                
                return View("Index", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrderAjax([FromBody] CheckoutViewModel model)
        {
            try
            {
                // Các kiểm tra khác - giữ nguyên code hiện tại
                Console.WriteLine($"Received AJAX request - Name: {model.CustomerName}, Address: {model.Address}, Phone: {model.Phone}");
                
                var cart = GetCartFromSession();
                if (cart == null || cart.CartItems.Count == 0)
                {
                    return Json(new { success = false, message = "Giỏ hàng của bạn đã hết hạn hoặc trống" });
                }

                // Validate model - giữ nguyên code hiện tại
                if (string.IsNullOrWhiteSpace(model.CustomerName))
                {
                    return Json(new { success = false, message = "Vui lòng nhập họ và tên" });
                }
                
                if (string.IsNullOrWhiteSpace(model.Address))
                {
                    return Json(new { success = false, message = "Vui lòng nhập địa chỉ giao hàng" });
                }
                
                if (string.IsNullOrWhiteSpace(model.Phone) || !System.Text.RegularExpressions.Regex.IsMatch(model.Phone, @"^0\d{9}$"))
                {
                    return Json(new { success = false, message = "Số điện thoại không hợp lệ" });
                }

                // Kiểm tra tồn kho
                var stockValidation = await ValidateStockQuantity(cart);
                if (!stockValidation.IsValid)
                {
                    string errorMessage = "Số lượng sản phẩm vượt quá tồn kho hiện có:<br/>";
                    foreach (var item in stockValidation.InvalidItems)
                    {
                        errorMessage += $"- {item.Key}: Chỉ còn {item.Value} sản phẩm<br/>";
                    }
                    return Json(new { success = false, message = errorMessage, outOfStock = true, invalidItems = stockValidation.InvalidItems });
                }

                // Sửa code xử lý giao dịch từ đây
                var strategy = _context.Database.CreateExecutionStrategy();
                int orderId = 0;
                
                await strategy.ExecuteAsync(async () =>
                {
                    using (var transaction = await _context.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            // Tạo đơn hàng mới
                            var order = new Order
                            {
                                OrderDate = DateTime.Now,
                                AccountId = HttpContext.Session.GetInt32("AccountId"),
                                Status = "Chờ xác nhận",
                                Address = model.Address,
                                TotalPrice = cart.TotalAmount,
                                PaymentMethod = model.PaymentMethod ?? "COD",
                                CreatedAt = DateTime.Now,
                                Phone = model.Phone,
                                CustomerName = model.CustomerName,
                                DeliveryDate = DateTime.Now.AddDays(3)
                            };

                            _context.Orders.Add(order);
                            await _context.SaveChangesAsync();
                            
                            Console.WriteLine($"Order created with ID: {order.OrderId}");
                            orderId = order.OrderId;
                            
                            // Thêm chi tiết đơn hàng bằng SQL thuần
                            foreach (var item in cart.CartItems)
                            {
                                string sql = "INSERT INTO order_details (order_id, motorbike_id, quantity, unit_price) " +
                                             "VALUES ({0}, {1}, {2}, {3})";
                                     
                                await _context.Database.ExecuteSqlRawAsync(sql,
                                    order.OrderId, item.MotorbikeId, item.Quantity, item.Price);
                                
                                // Cập nhật tồn kho
                                string updateSql = "UPDATE motorbike SET stock = stock - {0}, " +
                                                  "total_sold = ISNULL(total_sold, 0) + {1} " +
                                                  "WHERE motorbike_id = {2}";
                                          
                                await _context.Database.ExecuteSqlRawAsync(updateSql,
                                    item.Quantity, item.Quantity, item.MotorbikeId);
                            }

                            await transaction.CommitAsync();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Transaction error: {ex.Message}");
                            await transaction.RollbackAsync();
                            throw;
                        }
                    }
                });
                
                // Xử lý sau khi giao dịch hoàn thành thành công
                if (orderId > 0)
                {
                    // Thay vì xóa, gán giỏ hàng rỗng vào session
                    HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(new CartViewModel()));

                    // Đảm bảo giữ lại thông tin đăng nhập
                    int? existingAccountId = HttpContext.Session.GetInt32("AccountId");
                    var username = HttpContext.Session.GetString("Username");
                    var role = HttpContext.Session.GetString("Role");

                    if (existingAccountId.HasValue && !string.IsNullOrEmpty(username))
                    {
                        // Refresh session để đảm bảo thông tin đăng nhập không bị mất
                        HttpContext.Session.SetInt32("AccountId", existingAccountId.Value);
                        HttpContext.Session.SetString("Username", username);
                        if (!string.IsNullOrEmpty(role))
                            HttpContext.Session.SetString("Role", role);
                    }
                    
                    return Json(new { success = true, orderId = orderId });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể tạo đơn hàng" });
                }
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // Tách logic tạo và lưu đơn hàng để dễ debug
        private async Task<int> CreateOrder(CheckoutViewModel model, CartViewModel cart)
        {
            try
            {
                int orderId = 0;
                var strategy = _context.Database.CreateExecutionStrategy();
                
                await strategy.ExecuteAsync(async () => 
                {
                    using (var transaction = await _context.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            // Tạo đơn hàng mới
                            var order = new Order
                            {
                                OrderDate = DateTime.Now,
                                AccountId = model.AccountId > 0 ? model.AccountId : null,
                                Status = "Chờ xác nhận",
                                Address = model.Address,
                                TotalPrice = cart.TotalAmount,
                                PaymentMethod = model.PaymentMethod ?? "COD",
                                CreatedAt = DateTime.Now,
                                Phone = model.Phone,
                                CustomerName = model.CustomerName,
                                DeliveryDate = DateTime.Now.AddDays(3)
                            };

                            _context.Orders.Add(order);
                            await _context.SaveChangesAsync();
                            
                            Console.WriteLine($"Order created with ID: {order.OrderId}");
                            orderId = order.OrderId;
                            
                            // Thêm chi tiết đơn hàng
                            foreach (var item in cart.CartItems)
                            {
                                // Log mỗi item trước khi thêm
                                Console.WriteLine($"Adding OrderDetail - MotorbikeId: {item.MotorbikeId}, Quantity: {item.Quantity}, UnitPrice: {item.Price}");
                                
                                var orderDetail = new OrderDetail
                                {
                                    OrderId = order.OrderId,
                                    MotorbikeId = item.MotorbikeId,
                                    Quantity = item.Quantity,
                                    UnitPrice = item.Price
                                };
                                _context.OrderDetails.Add(orderDetail);
                            }

                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Transaction error: {ex.Message}");
                            await transaction.RollbackAsync();
                            throw;
                        }
                    }
                });
                
                return orderId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating order: {ex.Message}");
                return 0;
            }
        }

        public async Task<IActionResult> OrderConfirmation(int id)
        {
            try
            {
                Console.WriteLine($"OrderConfirmation called with orderId: {id} - URL: {Request.Path}");
                
                // Lấy thông tin đơn hàng kèm theo chi tiết
                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.OrderId == id);
                    
                if (order == null)
                {
                    Console.WriteLine($"Order with ID {id} not found");
                    return NotFound();
                }

                Console.WriteLine($"Order found: {order.OrderId}, Customer: {order.CustomerName}");
                
                // Lấy chi tiết đơn hàng và thông tin sản phẩm
                var orderDetails = new List<OrderDetail>();
                
                // Câu lệnh SQL đã được sửa để có alias phù hợp
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = @"
                        SELECT od.detail_id, od.order_id, od.motorbike_id, od.quantity, od.unit_price,
                               m.name AS motorbike_name, m.image AS motorbike_image
                        FROM order_details od
                        JOIN motorbike m ON od.motorbike_id = m.motorbike_id
                        WHERE od.order_id = @orderId";
                        
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "@orderId";
                    parameter.Value = id;
                    command.Parameters.Add(parameter);
                    
                    if (command.Connection.State != System.Data.ConnectionState.Open)
                        await command.Connection.OpenAsync();
                        
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var detail = new OrderDetail
                            {
                                DetailId = reader.GetInt32(0),
                                OrderId = reader.GetInt32(1),
                                MotorbikeId = reader.GetInt32(2),
                                Quantity = reader.GetInt32(3),
                                UnitPrice = reader.GetDecimal(4),
                                Motorbike = new Models.Motorbike
                                {
                                    MotorbikeId = reader.GetInt32(2),
                                    MotorbikeName = reader.GetString(5),
                                    Image = !reader.IsDBNull(6) ? reader.GetString(6) : "default.jpg"
                                }
                            };
                            
                            orderDetails.Add(detail);
                            Console.WriteLine($"Added detail: Product={detail.Motorbike.MotorbikeName}, Image={detail.Motorbike.Image}, Quantity={detail.Quantity}, Price={detail.UnitPrice}");
                        }
                    }
                }
                
                Console.WriteLine($"Loaded {orderDetails.Count} order details");
                
                // Gán danh sách chi tiết vào đơn hàng
                order.OrderDetails = orderDetails;

                return View(order);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OrderConfirmation: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Trả về lỗi để dễ debug
                return Content($"Error: {ex.Message}");
            }
        }

        private CartViewModel GetCartFromSession()
        {
            try
            {
                var cartJson = HttpContext.Session.GetString(CartSessionKey);
                if (string.IsNullOrEmpty(cartJson))
                {
                    return new CartViewModel();
                }

                return JsonSerializer.Deserialize<CartViewModel>(cartJson);
            }
            catch
            {
                // Nếu có lỗi khi deserialize, trả về giỏ hàng trống
                return new CartViewModel();
            }
        }

        // Thêm phương thức này để kiểm tra kết nối
        private async Task<bool> TestDatabaseConnection()
        {
            try
            {
                return await _context.Database.CanConnectAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database connection error: {ex.Message}");
                return false;
            }
        }

        // Thêm phương thức này vào cuối class CheckoutController
        private async Task<(bool IsValid, Dictionary<string, int> InvalidItems)> ValidateStockQuantity(CartViewModel cart)
        {
            bool isValid = true;
            var invalidItems = new Dictionary<string, int>();

            foreach (var item in cart.CartItems)
            {
                // Lấy thông tin tồn kho hiện tại
                var motorbike = await _context.Motorbikes.FindAsync(item.MotorbikeId);
                if (motorbike == null)
                {
                    isValid = false;
                    invalidItems.Add(item.MotorbikeName, 0); // Sản phẩm không tồn tại
                    continue;
                }

                // Kiểm tra số lượng
                if (item.Quantity > motorbike.Stock)
                {
                    isValid = false;
                    invalidItems.Add(item.MotorbikeName, motorbike.Stock); // Số lượng tồn kho thực tế
                }
            }

            return (isValid, invalidItems);
        }
    }
}