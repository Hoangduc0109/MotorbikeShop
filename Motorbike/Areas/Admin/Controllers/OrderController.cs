using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Motorbike.Data;
using Motorbike.Models;
using X.PagedList;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList.Extensions;

namespace Motorbike.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrderController> _logger;

        public OrderController(ApplicationDbContext context, ILogger<OrderController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Order
        public async Task<IActionResult> Index(string searchString, string status, int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 10;

            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStatus"] = status;

            var orders = from o in _context.Orders
                         select o;

            // Lọc theo status nếu có
            if (!string.IsNullOrEmpty(status))
            {
                orders = orders.Where(o => o.Status == status);
            }

            // Lọc theo searchString
            if (!string.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(o => o.CustomerName.Contains(searchString) || 
                                          o.Phone.Contains(searchString) || 
                                          o.OrderId.ToString().Contains(searchString));
            }

            // Lấy danh sách trạng thái để hiển thị trong dropdown
            var statusList = await _context.Orders
                .Select(o => o.Status)
                .Distinct()
                .Where(s => s != null)
                .ToListAsync();

            ViewBag.StatusList = statusList;

            // Thêm include để lấy thông tin Account
            var ordersList = await orders
                .Include(o => o.Account)
                .OrderByDescending(o => o.OrderId)
                .ToListAsync();

            return View(ordersList.ToPagedList(pageNumber, pageSize));
        }

        // GET: Admin/Order/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // Lấy thông tin đơn hàng không kèm chi tiết
                var order = await _context.Orders
                    .AsNoTracking()
                    .Include(o => o.Account)
                    .FirstOrDefaultAsync(o => o.OrderId == id);

                if (order == null)
                {
                    return NotFound();
                }

                // Tạo ViewModel mới 
                var viewModel = new OrderViewModel
                {
                    OrderId = order.OrderId,
                    OrderDate = order.OrderDate,
                    DeliveryDate = order.DeliveryDate?.ToString("yyyy-MM-dd"),
                    Status = order.Status,
                    Address = order.Address,
                    TotalPrice = order.TotalPrice ?? 0,
                    PaymentMethod = order.PaymentMethod,
                    CreatedAt = order.CreatedAt,
                    Phone = order.Phone,
                    CustomerName = order.CustomerName,
                    AccountId = order.AccountId, // Thêm dòng này
                    Account = order.Account,
                    OrderDetails = new List<OrderDetailViewModel>()
                };

                // Sử dụng SQL thuần để lấy chi tiết đơn hàng và thông tin xe máy
                var sql = @"
                    SELECT od.detail_id, od.order_id, od.motorbike_id, od.quantity, od.unit_price,
                           m.motorbike_id AS motor_id, m.name, m.brand_id, m.price, m.image, 
                           m.total_sold, m.stock, m.description
                    FROM order_details od
                    LEFT JOIN motorbike m ON od.motorbike_id = m.motorbike_id
                    WHERE od.order_id = @orderId";

                // Thực hiện truy vấn bằng ADO.NET
                using (var connection = _context.Database.GetDbConnection())
                {
                    await connection.OpenAsync();
                    
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = sql;
                        
                        var parameter = command.CreateParameter();
                        parameter.ParameterName = "@orderId";
                        parameter.Value = id;
                        command.Parameters.Add(parameter);
                        
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var detail = new OrderDetailViewModel
                                {
                                    DetailId = reader.GetInt32(reader.GetOrdinal("detail_id")),
                                    OrderId = reader.GetInt32(reader.GetOrdinal("order_id")),
                                    MotorbikeId = reader.GetInt32(reader.GetOrdinal("motorbike_id")),
                                    Quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
                                    UnitPrice = reader.GetDecimal(reader.GetOrdinal("unit_price"))
                                };
                                
                                // Xử lý trường hợp xe máy có thể đã bị xóa (LEFT JOIN)
                                if (!reader.IsDBNull(reader.GetOrdinal("motor_id")))
                                {
                                    detail.Motorbike = new Motorbike.Models.Motorbike
                                    {
                                        MotorbikeId = reader.GetInt32(reader.GetOrdinal("motor_id")),
                                        MotorbikeName = reader.IsDBNull(reader.GetOrdinal("name")) ? null : reader.GetString(reader.GetOrdinal("name")),
                                        BrandId = reader.GetInt32(reader.GetOrdinal("brand_id")),
                                        Price = reader.GetDecimal(reader.GetOrdinal("price")),
                                        Image = reader.IsDBNull(reader.GetOrdinal("image")) ? null : reader.GetString(reader.GetOrdinal("image")),
                                        TotalSold = reader.GetInt32(reader.GetOrdinal("total_sold")),
                                        Stock = reader.GetInt32(reader.GetOrdinal("stock")),
                                        Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description"))
                                    };
                                }
                                
                                viewModel.OrderDetails.Add(detail);
                            }
                        }
                    }
                }
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải chi tiết đơn hàng {OrderId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin đơn hàng. Vui lòng thử lại sau.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Admin/Order/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // Lấy thông tin đơn hàng cơ bản (không bao gồm OrderDetails)
                var order = await _context.Orders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.OrderId == id);

                if (order == null)
                {
                    return NotFound();
                }

                // Tạo OrderViewModel để sử dụng trong view
                var viewModel = new OrderViewModel
                {
                    OrderId = order.OrderId,
                    OrderDate = order.OrderDate,
                    DeliveryDate = order.DeliveryDate?.ToString("yyyy-MM-dd"),
                    Status = order.Status,
                    Address = order.Address,
                    TotalPrice = order.TotalPrice ?? 0,
                    PaymentMethod = order.PaymentMethod,
                    CreatedAt = order.CreatedAt,
                    Phone = order.Phone,
                    CustomerName = order.CustomerName,
                    AccountId = order.AccountId,
                    Account = order.Account,
                    OrderDetails = new List<OrderDetailViewModel>()
                };

                // Sử dụng SQL thuần để lấy chi tiết đơn hàng và thông tin xe máy
                var sql = @"
                    SELECT od.detail_id, od.order_id, od.motorbike_id, od.quantity, od.unit_price,
                           m.motorbike_id AS motor_id, m.name, m.brand_id, m.price, m.image, 
                           m.total_sold, m.stock, m.description
                    FROM order_details od
                    LEFT JOIN motorbike m ON od.motorbike_id = m.motorbike_id
                    WHERE od.order_id = @orderId";

                // Thực hiện truy vấn bằng ADO.NET
                using (var connection = _context.Database.GetDbConnection())
                {
                    await connection.OpenAsync();
                    
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = sql;
                        
                        var parameter = command.CreateParameter();
                        parameter.ParameterName = "@orderId";
                        parameter.Value = id;
                        command.Parameters.Add(parameter);
                        
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var detail = new OrderDetailViewModel
                                {
                                    DetailId = reader.GetInt32(reader.GetOrdinal("detail_id")),
                                    OrderId = reader.GetInt32(reader.GetOrdinal("order_id")),
                                    MotorbikeId = reader.GetInt32(reader.GetOrdinal("motorbike_id")),
                                    Quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
                                    UnitPrice = reader.GetDecimal(reader.GetOrdinal("unit_price"))
                                };
                                
                                // Xử lý trường hợp xe máy có thể đã bị xóa (LEFT JOIN)
                                if (!reader.IsDBNull(reader.GetOrdinal("motor_id")))
                                {
                                    detail.Motorbike = new Motorbike.Models.Motorbike
                                    {
                                        MotorbikeId = reader.GetInt32(reader.GetOrdinal("motor_id")),
                                        MotorbikeName = reader.IsDBNull(reader.GetOrdinal("name")) ? null : reader.GetString(reader.GetOrdinal("name")),
                                        BrandId = reader.GetInt32(reader.GetOrdinal("brand_id")),
                                        Price = reader.GetDecimal(reader.GetOrdinal("price")),
                                        Image = reader.IsDBNull(reader.GetOrdinal("image")) ? null : reader.GetString(reader.GetOrdinal("image")),
                                        TotalSold = reader.GetInt32(reader.GetOrdinal("total_sold")),
                                        Stock = reader.GetInt32(reader.GetOrdinal("stock")),
                                        Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description"))
                                    };
                                }
                                
                                viewModel.OrderDetails.Add(detail);
                            }
                        }
                    }
                }

                // Tạo SelectList cho trạng thái đơn hàng
                ViewBag.OrderStatuses = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Chờ xác nhận", Value = "Chờ xác nhận" },
                    new SelectListItem { Text = "Đã xác nhận", Value = "Đã xác nhận" },
                    new SelectListItem { Text = "Đang giao hàng", Value = "Đang giao hàng" },
                    new SelectListItem { Text = "Đã giao hàng", Value = "Đã giao hàng" },
                    new SelectListItem { Text = "Đã hủy", Value = "Đã hủy" }
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải thông tin đơn hàng {OrderId} để chỉnh sửa", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin đơn hàng. Vui lòng thử lại sau.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Order/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OrderViewModel model)
        {
            if (id != model.OrderId)
            {
                return Json(new { success = false, message = "ID đơn hàng không hợp lệ." });
            }

            try
            {
                // Lấy đơn hàng hiện tại từ database
                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
                }

                // Cập nhật các trường
                order.Status = model.Status;
                
                // Xử lý ngày giao hàng
                if (string.IsNullOrEmpty(model.DeliveryDate))
                {
                    order.DeliveryDate = null;
                }
                else if (DateTime.TryParse(model.DeliveryDate, out DateTime deliveryDate))
                {
                    order.DeliveryDate = deliveryDate;
                }
                
                await _context.SaveChangesAsync();
                
                // Đảm bảo đúng định dạng và Content-Type
                Response.ContentType = "application/json";
                return new JsonResult(new { success = true, message = "Cập nhật đơn hàng thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật đơn hàng {OrderId}", id);
                return Json(new { success = false, message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }

        // GET: Admin/Order/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Account)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Motorbike)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Admin/Order/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                // Sử dụng connection trực tiếp từ context
                using (var connection = _context.Database.GetDbConnection())
                {
                    await connection.OpenAsync();

                    // Tạo transaction để đảm bảo tính nhất quán
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Kiểm tra xem đơn hàng có tồn tại không
                            using (var command = connection.CreateCommand())
                            {
                                command.Transaction = transaction;
                                command.CommandText = "SELECT COUNT(*) FROM orders WHERE order_id = @orderId";
                                
                                var parameter = command.CreateParameter();
                                parameter.ParameterName = "@orderId";
                                parameter.Value = id;
                                command.Parameters.Add(parameter);
                                
                                var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                                if (count == 0)
                                {
                                    return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
                                }
                            }

                            // Xóa chi tiết đơn hàng trước
                            using (var command = connection.CreateCommand())
                            {
                                command.Transaction = transaction;
                                command.CommandText = "DELETE FROM order_details WHERE order_id = @orderId";
                                
                                var parameter = command.CreateParameter();
                                parameter.ParameterName = "@orderId";
                                parameter.Value = id;
                                command.Parameters.Add(parameter);
                                
                                await command.ExecuteNonQueryAsync();
                            }

                            // Sau đó xóa đơn hàng
                            using (var command = connection.CreateCommand())
                            {
                                command.Transaction = transaction;
                                command.CommandText = "DELETE FROM orders WHERE order_id = @orderId";
                                
                                var parameter = command.CreateParameter();
                                parameter.ParameterName = "@orderId";
                                parameter.Value = id;
                                command.Parameters.Add(parameter);
                                
                                await command.ExecuteNonQueryAsync();
                            }

                            // Commit transaction nếu mọi thứ đều OK
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            // Rollback trong trường hợp lỗi
                            transaction.Rollback();
                            _logger.LogError(ex, "Lỗi khi xóa đơn hàng {OrderId}", id);
                            return Json(new { success = false, message = $"Đã xảy ra lỗi: {ex.Message}" });
                        }
                    }
                }
                
                return Json(new { success = true, message = "Xóa đơn hàng thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa đơn hàng {OrderId}", id);
                return Json(new { success = false, message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }

        // POST: Admin/Order/DeleteAjax/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            try
            {
                // Sử dụng connection trực tiếp từ context
                using (var connection = _context.Database.GetDbConnection())
                {
                    await connection.OpenAsync();

                    // Tạo transaction để đảm bảo tính nhất quán
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Kiểm tra xem đơn hàng có tồn tại không
                            using (var command = connection.CreateCommand())
                            {
                                command.Transaction = transaction;
                                command.CommandText = "SELECT COUNT(*) FROM orders WHERE order_id = @orderId";
                                
                                var parameter = command.CreateParameter();
                                parameter.ParameterName = "@orderId";
                                parameter.Value = id;
                                command.Parameters.Add(parameter);
                                
                                var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                                if (count == 0)
                                {
                                    return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
                                }
                            }

                            // Xóa chi tiết đơn hàng trước
                            using (var command = connection.CreateCommand())
                            {
                                command.Transaction = transaction;
                                command.CommandText = "DELETE FROM order_details WHERE order_id = @orderId";
                                
                                var parameter = command.CreateParameter();
                                parameter.ParameterName = "@orderId";
                                parameter.Value = id;
                                command.Parameters.Add(parameter);
                                
                                await command.ExecuteNonQueryAsync();
                            }

                            // Sau đó xóa đơn hàng
                            using (var command = connection.CreateCommand())
                            {
                                command.Transaction = transaction;
                                command.CommandText = "DELETE FROM orders WHERE order_id = @orderId";
                                
                                var parameter = command.CreateParameter();
                                parameter.ParameterName = "@orderId";
                                parameter.Value = id;
                                command.Parameters.Add(parameter);
                                
                                await command.ExecuteNonQueryAsync();
                            }

                            // Commit transaction nếu mọi thứ đều OK
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            // Rollback trong trường hợp lỗi
                            transaction.Rollback();
                            _logger.LogError(ex, "Lỗi khi xóa đơn hàng {OrderId}", id);
                            return Json(new { success = false, message = $"Đã xảy ra lỗi: {ex.Message}" });
                        }
                    }
                }
                
                return Json(new { success = true, message = "Xóa đơn hàng thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa đơn hàng {OrderId}", id);
                return Json(new { success = false, message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }

        // POST: Admin/Order/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
                }

                order.Status = status;
                await _context.SaveChangesAsync();
                
                // Đảm bảo đúng định dạng và Content-Type
                Response.ContentType = "application/json";
                return new JsonResult(new { success = true, message = "Cập nhật trạng thái đơn hàng thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái đơn hàng");
                return Json(new { success = false, message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }
    }

    // Class phụ trợ để nhận kết quả truy vấn SQL
    public class OrderDetailWithMotorbike
    {
        public int detail_id { get; set; }
        public int order_id { get; set; }
        public int motorbike_id { get; set; }
        public int quantity { get; set; }
        public decimal unit_price { get; set; }
        
        // Thông tin xe máy
        public int MotorbikeId { get; set; }
        public string MotorbikeName { get; set; }
        public int brand_id { get; set; }
        public decimal price { get; set; }
        public string image { get; set; }
        public int total_sold { get; set; }
        public int stock { get; set; }
        public string description { get; set; }
    }

    // Thêm các class tạm để sử dụng trong view
    public class OrderViewModel
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        
        // Thay đổi kiểu dữ liệu DateTime? thành string để tránh lỗi parsing
        public string DeliveryDate { get; set; }
        
        public string Status { get; set; }
        public string Address { get; set; }
        public decimal TotalPrice { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string Phone { get; set; }
        public string CustomerName { get; set; }
        public int? AccountId { get; set; }
        public Account Account { get; set; }
        public List<OrderDetailViewModel> OrderDetails { get; set; }
    }

    public class OrderDetailViewModel
    {
        public int DetailId { get; set; }
        public int OrderId { get; set; }
        public int MotorbikeId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public Motorbike.Models.Motorbike Motorbike { get; set; }
    }
}