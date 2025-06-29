using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Motorbike.Data;
using Motorbike.Models;
using Motorbike.ViewModels;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Motorbike.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(ApplicationDbContext context, ILogger<ProfileController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Profile/Index
        public async Task<IActionResult> Index()
        {
            // Kiểm tra đăng nhập
            var accountId = HttpContext.Session.GetInt32("AccountId");
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy thông tin tài khoản
            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null)
            {
                return NotFound();
            }

            // Map dữ liệu cho Combined ViewModel
            var model = new CombinedProfileViewModel
            {
                ProfileInfo = new ProfileViewModel
                {
                    Username = account.Username,
                    Email = account.Email,
                    Phone = account.Phone,
                    Address = account.Address
                },
                Password = new ChangePasswordViewModel()
            };

            return View(model);
        }

        // POST: /Profile/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(CombinedProfileViewModel model)
        {
            // Lấy accountId ngay từ đầu phương thức
            var accountId = HttpContext.Session.GetInt32("AccountId");
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Bỏ qua toàn bộ validation để debug
            foreach (var key in ModelState.Keys.ToList())
            {
                ModelState.Remove(key);
            }

            try
            {
                // In ra tất cả giá trị truyền vào để debug
                _logger.LogInformation($"Updating profile: Username={model.ProfileInfo?.Username}, Email={model.ProfileInfo?.Email}, Phone={model.ProfileInfo?.Phone}, Address={model.ProfileInfo?.Address}");
                
                // Lấy account hiện tại
                var account = await _context.Accounts.FindAsync(accountId);
                if (account == null)
                {
                    TempData["ProfileErrorMessage"] = "Không tìm thấy tài khoản!";
                    goto PrepareModelAndReturnView;
                }
                
                // Cập nhật trực tiếp thông qua SQL - cách đơn giản nhất
                using (var connection = _context.Database.GetDbConnection())
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "UPDATE account SET email = @email, phone = @phone, address = @address WHERE account_id = @accountId";
                        
                        var emailParam = command.CreateParameter();
                        emailParam.ParameterName = "@email";
                        emailParam.Value = model.ProfileInfo?.Email ?? account.Email;
                        command.Parameters.Add(emailParam);
                        
                        var phoneParam = command.CreateParameter();
                        phoneParam.ParameterName = "@phone";
                        phoneParam.Value = model.ProfileInfo?.Phone ?? account.Phone;
                        command.Parameters.Add(phoneParam);
                        
                        var addressParam = command.CreateParameter();
                        addressParam.ParameterName = "@address";
                        addressParam.Value = model.ProfileInfo?.Address ?? account.Address;
                        command.Parameters.Add(addressParam);
                        
                        var accountIdParam = command.CreateParameter();
                        accountIdParam.ParameterName = "@accountId";
                        accountIdParam.Value = accountId;
                        command.Parameters.Add(accountIdParam);
                        
                        int result = await command.ExecuteNonQueryAsync();
                        
                        _logger.LogInformation($"Update profile SQL result: {result} rows affected");
                        // Sau khi cập nhật thành công, lưu message vào TempData
                        if (result > 0)
                        {
                            TempData["ProfileSuccessMessage"] = "Cập nhật thông tin thành công!";
                        }
                        else
                        {
                            TempData["ProfileErrorMessage"] = "Không có thay đổi nào được lưu!";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                TempData["ProfileErrorMessage"] = "Lỗi khi cập nhật thông tin: " + ex.Message;
            }

        PrepareModelAndReturnView:
            // Lấy thông tin mới nhất - biến accountId đã được khởi tạo ở đầu phương thức
            var refreshAccount = await _context.Accounts.FindAsync(accountId);
            
            var refreshedModel = new CombinedProfileViewModel
            {
                ProfileInfo = new ProfileViewModel
                {
                    Username = refreshAccount?.Username,
                    Email = refreshAccount?.Email,
                    Phone = refreshAccount?.Phone,
                    Address = refreshAccount?.Address
                },
                Password = new ChangePasswordViewModel()
            };

            return RedirectToAction("Index");
        }

        // POST: /Profile/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(CombinedProfileViewModel model)
        {
            // Kiểm tra đăng nhập
            var accountId = HttpContext.Session.GetInt32("AccountId");
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Log để debug
                _logger.LogInformation("Changing password, input data: CurrentPassword={CurrentPassword}, NewPassword={NewPassword}, ConfirmPassword={ConfirmPassword}",
                    model.Password?.CurrentPassword?.Length > 0 ? "PROVIDED" : "MISSING",
                    model.Password?.NewPassword?.Length > 0 ? "PROVIDED" : "MISSING",
                    model.Password?.ConfirmPassword?.Length > 0 ? "PROVIDED" : "MISSING");

                // Validate thủ công
                if (model.Password == null || 
                    string.IsNullOrEmpty(model.Password.CurrentPassword) ||
                    string.IsNullOrEmpty(model.Password.NewPassword) ||
                    string.IsNullOrEmpty(model.Password.ConfirmPassword))
                {
                    TempData["PasswordErrorMessage"] = "Vui lòng nhập đầy đủ thông tin mật khẩu";
                    return RedirectToAction("Index");
                }

                if (model.Password.NewPassword.Length < 6)
                {
                    TempData["PasswordErrorMessage"] = "Mật khẩu mới phải có ít nhất 6 ký tự";
                    return RedirectToAction("Index");
                }

                if (model.Password.NewPassword != model.Password.ConfirmPassword)
                {
                    TempData["PasswordErrorMessage"] = "Mật khẩu xác nhận không khớp";
                    return RedirectToAction("Index");
                }

                // Lấy thông tin tài khoản
                var account = await _context.Accounts.FindAsync(accountId);
                if (account == null)
                {
                    TempData["PasswordErrorMessage"] = "Không tìm thấy tài khoản!";
                    return RedirectToAction("Index");
                }

                // Kiểm tra mật khẩu hiện tại
                bool passwordVerified = false;
                var hashedInputPassword = HashPasswordSHA256(model.Password.CurrentPassword);
                
                _logger.LogInformation("Verifying password for account {AccountId}", accountId);

                if (account.Password == hashedInputPassword)
                {
                    passwordVerified = true;
                    _logger.LogInformation("Password verified using SHA256 hash");
                }
                else if (account.Password == model.Password.CurrentPassword)
                {
                    passwordVerified = true;
                    _logger.LogInformation("Password verified using plain text");
                }

                if (!passwordVerified)
                {
                    TempData["PasswordErrorMessage"] = "Mật khẩu hiện tại không đúng!";
                    return RedirectToAction("Index");
                }

                // Cập nhật mật khẩu mới - sử dụng cách trực tiếp để tránh lỗi tracking
                using (var connection = _context.Database.GetDbConnection())
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "UPDATE account SET password = @password WHERE account_id = @accountId";
                        
                        var passwordParam = command.CreateParameter();
                        passwordParam.ParameterName = "@password";
                        passwordParam.Value = HashPasswordSHA256(model.Password.NewPassword);
                        command.Parameters.Add(passwordParam);
                        
                        var accountIdParam = command.CreateParameter();
                        accountIdParam.ParameterName = "@accountId";
                        accountIdParam.Value = accountId;
                        command.Parameters.Add(accountIdParam);
                        
                        int result = await command.ExecuteNonQueryAsync();
                        
                        _logger.LogInformation($"Update password SQL result: {result} rows affected");
                        if (result > 0)
                        {
                            TempData["PasswordSuccessMessage"] = "Đổi mật khẩu thành công!";
                        }
                        else
                        {
                            TempData["PasswordErrorMessage"] = "Không thể cập nhật mật khẩu!";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while changing password for account {AccountId}", accountId);
                TempData["PasswordErrorMessage"] = "Lỗi khi đổi mật khẩu: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        private string HashPasswordSHA256(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}