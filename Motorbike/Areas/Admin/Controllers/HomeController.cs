using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Motorbike.Areas.Admin.Models;
using Motorbike.Data;
using Motorbike.Models; // Thêm dòng này
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Motorbike.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Home
        public async Task<IActionResult> Index()
        {
            // Kiểm tra quyền admin
            var roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId == null || roleId != 1)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            // Lấy thông tin tổng quan
            var totalOrders = await _context.Orders.CountAsync();
            var totalProducts = await _context.Motorbikes.CountAsync();
            var totalCustomers = await _context.Accounts.CountAsync(a => a.RoleId == 2); // Role 2 là customer
            
            // Lấy danh sách đơn hàng gần đây
            // Thêm logging để kiểm tra SQL query
            _logger.LogInformation("Đang lấy dữ liệu đơn hàng từ CSDL");

            List<Order> recentOrders = new List<Order>();
            List<Contact> recentContacts = new List<Contact>();

            try
            {
                // Lấy đơn hàng mới
                recentOrders = await _context.Orders
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .ToListAsync();
                
                _logger.LogInformation($"Đã lấy được {recentOrders.Count} đơn hàng");
                
                // Lấy tổng số liên hệ trước khi truy vấn danh sách
                int totalContactCount = await _context.Contact.CountAsync();
                _logger.LogInformation($"Tổng số liên hệ trong CSDL: {totalContactCount}");
                
                // Thử truy vấn trực tiếp không sắp xếp
                var allContacts = await _context.Contact.ToListAsync();
                _logger.LogInformation($"Truy vấn không sắp xếp: Lấy được {allContacts.Count} liên hệ");
                
                // Lấy liên hệ mới, thử cách khác để sắp xếp
                recentContacts = await _context.Contact
                    .OrderByDescending(c => c.ContactId) // Sắp xếp theo ID thay vì ngày tạo
                    .Take(5)
                    .ToListAsync();
                
                _logger.LogInformation($"Đã lấy được {recentContacts.Count} liên hệ theo ID");
                
                // Nếu vẫn không có kết quả, gán danh sách liên hệ từ truy vấn trực tiếp
                if (recentContacts.Count == 0 && allContacts.Count > 0)
                {
                    _logger.LogWarning("Sử dụng kết quả từ truy vấn trực tiếp");
                    recentContacts = allContacts.OrderByDescending(c => c.ContactId).Take(5).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi truy vấn dữ liệu: {Message}", ex.Message);
                // Tiếp tục với danh sách rỗng
            }

            var viewModel = new DashboardViewModel
            {
                TotalOrders = totalOrders,
                TotalProducts = totalProducts,
                TotalCustomers = totalCustomers,
                RecentOrders = recentOrders,
                RecentContacts = recentContacts
            };

            return View(viewModel);
        }

        // Thêm phương thức này để kiểm tra
        public async Task<IActionResult> TestContact()
        {
            try
            {
                var contacts = await _context.Contact.ToListAsync();
                var result = new
                {
                    Count = contacts.Count,
                    Data = contacts.Select(c => new 
                    {
                        Id = c.ContactId,
                        Name = c.Name,
                        Email = c.Email,
                        CreatedAt = c.CreatedAt,
                        Status = c.Status
                    }).Take(5)
                };
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { Error = ex.Message, StackTrace = ex.StackTrace });
            }
        }
    }
}