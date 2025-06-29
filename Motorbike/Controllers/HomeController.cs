using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Motorbike.Data;
using Motorbike.Models;
using Motorbike.ViewModels;
using System.Diagnostics;

namespace Motorbike.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy danh sách thương hiệu xe máy
            ViewBag.Brands = await _context.Brands.ToListAsync();
            
            // Lấy danh sách xe máy nổi bật (có total_sold cao nhất)
            ViewBag.FeaturedMotorbikes = await _context.Motorbikes
                .Include(m => m.Brand)
                .OrderByDescending(m => m.TotalSold)
                .Take(6)
                .ToListAsync();

            // Luôn truyền model mới cho view
            return View(new ContactViewModel());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> TestConnection()
        {
            try
            {
                // Kiểm tra kết nối bằng cách truy vấn
                bool canConnect = await _context.Database.CanConnectAsync();
                ViewBag.ConnectionStatus = canConnect ? "Kết nối thành công!" : "Không thể kết nối đến cơ sở dữ liệu!";
                
                // Thử lấy danh sách motorbike
                ViewBag.Motorbikes = await _context.Motorbikes.Take(5).ToListAsync();
                
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.ConnectionStatus = "Lỗi kết nối: " + ex.Message;
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendContact(ContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int? accountId = HttpContext.Session.GetInt32("AccountId");
                    var contact = new Contact
                    {
                        AccountId = accountId,
                        Name = model.Name,
                        Email = model.Email,
                        Phone = model.Phone,
                        Subject = model.Subject,
                        Message = model.Message,
                        CreatedAt = DateTime.Now,
                        Status = "Chưa xử lý"
                    };
                    _context.Contact.Add(contact);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi trong thời gian sớm nhất.";
                    return RedirectToAction("Index");
                }
                catch
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra khi gửi tin nhắn. Vui lòng thử lại sau.");
                }
            }

            // Lấy lại dữ liệu cho trang chủ
            ViewBag.Brands = await _context.Brands.ToListAsync();
            ViewBag.FeaturedMotorbikes = await _context.Motorbikes
                .Include(m => m.Brand)
                .OrderByDescending(m => m.TotalSold)
                .Take(6)
                .ToListAsync();

            return View("Index", model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}