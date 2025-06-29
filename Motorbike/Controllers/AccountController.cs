using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Motorbike.Data;
using Motorbike.Models;
using Motorbike.ViewModels;
using System.Security.Cryptography;
using System.Text;

namespace Motorbike.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(ApplicationDbContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Tìm tài khoản theo username - không sử dụng Include
                var user = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.Username == model.Username);

                if (user != null && VerifyPassword(model.Password, user.Password))
                {
                    // Lưu thông tin vào session
                    var roleId = user.RoleId;
                    HttpContext.Session.SetInt32("AccountId", user.AccountId);
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetInt32("RoleId", roleId ?? 0);
                    
                    _logger.LogInformation($"User {model.Username} logged in successfully");
                    
                    // Chuyển hướng dựa trên vai trò
                    if (roleId == 1) // Admin
                    {
                        return RedirectToAction("Index", "Home", new { area = "Admin" });
                    }
                    else // Khách hàng
                    {
                        return RedirectToAction("Index", "Home", new { area = "" });
                    }
                }

                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng");
                _logger.LogWarning($"Failed login attempt for username: {model.Username}");
            }

            return View(model);
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra username đã tồn tại chưa
                if (await _context.Accounts.AnyAsync(a => a.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã được sử dụng");
                    return View(model);
                }

                // Kiểm tra email đã tồn tại chưa
                if (await _context.Accounts.AnyAsync(a => a.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email đã được sử dụng");
                    return View(model);
                }

                // Mặc định role là user (role_id = 2)
                int defaultRoleId = 2; // Thông thường 1 là Admin, 2 là User
                
                // Tạo tài khoản mới
                var newAccount = new Account
                {
                    Username = model.Username,
                    Password = HashPassword(model.Password),
                    Email = model.Email,
                    Phone = model.Phone,
                    Address = model.Address, // Thêm địa chỉ
                    RoleId = defaultRoleId
                };

                _context.Accounts.Add(newAccount);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"New user registered: {model.Username}");
                
                // Tự động đăng nhập sau khi đăng ký
                HttpContext.Session.SetInt32("AccountId", newAccount.AccountId);
                HttpContext.Session.SetString("Username", newAccount.Username);
                HttpContext.Session.SetInt32("RoleId", newAccount.RoleId ?? 0); // Sử dụng RoleId

                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            // Xóa session
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // Phương thức mã hóa mật khẩu
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        // Phương thức kiểm tra mật khẩu
        private bool VerifyPassword(string inputPassword, string storedPassword)
        {
            return storedPassword == HashPassword(inputPassword);
        }
    }
}