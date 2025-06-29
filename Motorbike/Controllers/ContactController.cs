using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Motorbike.Data;
using Motorbike.Models;
using Motorbike.ViewModels;
using System;
using System.Threading.Tasks;

namespace Motorbike.Controllers
{
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ContactController> _logger;

        public ContactController(ApplicationDbContext context, ILogger<ContactController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Contact/Index
        public IActionResult Index()
        {
            // Tự động điền thông tin nếu người dùng đã đăng nhập
            ContactViewModel model = new ContactViewModel();
            int? accountId = HttpContext.Session.GetInt32("AccountId");
            
            if (accountId.HasValue)
            {
                var user = _context.Accounts.Find(accountId.Value);
                if (user != null)
                {
                    model.Name = user.Username;
                    model.Email = user.Email;
                    model.Phone = user.Phone;
                }
            }
            
            return View(model);
        }

        // POST: Contact/SendMessage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(ContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy thông tin tài khoản nếu người dùng đã đăng nhập
                    int? accountId = HttpContext.Session.GetInt32("AccountId");

                    // Tạo đối tượng Contact từ ViewModel
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

                    // Thêm vào cơ sở dữ liệu
                    _context.Contact.Add(contact);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"New contact message from {model.Name}, Email: {model.Email}");
                    
                    // Chuyển hướng đến trang thành công với thông báo
                    TempData["SuccessMessage"] = "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi trong thời gian sớm nhất.";
                    return RedirectToAction("Success");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error submitting contact form");
                    ModelState.AddModelError("", "Có lỗi xảy ra khi gửi tin nhắn. Vui lòng thử lại sau.");
                }
            }

            return View("Index", model);
        }

        // GET: Contact/Success
        public IActionResult Success()
        {
            if (TempData["SuccessMessage"] == null)
            {
                return RedirectToAction("Index");
            }
            
            return View();
        }

        // GET: Contact/History
        [HttpGet]
        public async Task<IActionResult> History()
        {
            // Kiểm tra đăng nhập
            int? accountId = HttpContext.Session.GetInt32("AccountId");
            if (!accountId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy danh sách các liên hệ của người dùng hiện tại
            var contacts = await _context.Contact
                .Where(c => c.AccountId == accountId.Value)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
                
            return View(contacts);
        }
    }
}