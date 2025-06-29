using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Motorbike.Data;
using Motorbike.Models;
using Motorbike.ViewModels;
using X.PagedList;
using X.PagedList.Mvc.Core;

namespace Motorbike.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(ApplicationDbContext context, ILogger<CustomerController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Customer
        public async Task<IActionResult> Index(CustomerFilterViewModel filter, int? page)
        {
            try
            {
                _logger.LogInformation("Đang tải danh sách khách hàng");

                // Mặc định là trang 1 nếu không có tham số
                int pageNumber = page ?? filter.Page;
                int pageSize = filter.PageSize;
                var customersQuery = _context.Accounts.AsNoTracking().Where(a => a.RoleId == 2);

                // Áp dụng bộ lọc tìm kiếm
                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    string searchTerm = filter.SearchTerm.ToLower();
                    customersQuery = customersQuery.Where(a => 
                        a.Username.ToLower().Contains(searchTerm) || 
                        a.Email.ToLower().Contains(searchTerm) ||
                        (a.Phone != null && a.Phone.Contains(searchTerm)) ||
                        (a.Address != null && a.Address.ToLower().Contains(searchTerm))
                    );
                }

                // Áp dụng sắp xếp
                switch (filter.SortOrder)
                {
                    case "name_desc":
                        customersQuery = customersQuery.OrderByDescending(c => c.Username);
                        break;
                    case "email":
                        customersQuery = customersQuery.OrderBy(c => c.Email);
                        break;
                    case "email_desc":
                        customersQuery = customersQuery.OrderByDescending(c => c.Email);
                        break;
                    default:
                        customersQuery = customersQuery.OrderBy(c => c.Username);
                        break;
                }

                // Thay thế phương thức ToPagedList bằng cách tạo StaticPagedList trực tiếp
                var customersData = await customersQuery.Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize).ToListAsync();
                var totalCount = await customersQuery.CountAsync();
                var customers = new StaticPagedList<Account>(
                    customersData, pageNumber, pageSize, totalCount);

                // Lưu lại thông tin filter cho view
                ViewBag.CurrentFilter = filter;

                _logger.LogInformation("Đã tải danh sách khách hàng thành công");
                return View(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách khách hàng");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải danh sách khách hàng. Vui lòng thử lại sau.";
                var emptyList = new StaticPagedList<Account>(
                    new List<Account>(), 1, 10, 0);
                return View(emptyList);
            }
        }

        // GET: Admin/Customer/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // Lấy thông tin khách hàng
                var account = await _context.Accounts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.AccountId == id && m.RoleId == 2);

                if (account == null)
                {
                    return NotFound();
                }

                // Lấy đơn hàng của khách hàng
                var orders = await _context.Orders
                    .Where(o => o.AccountId == id)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .ToListAsync();

                // Lấy tổng số đơn hàng
                int totalOrders = await _context.Orders
                    .CountAsync(o => o.AccountId == id);

                // Lấy tổng số tiền đã chi tiêu
                decimal totalSpent = await _context.Orders
                    .Where(o => o.AccountId == id && o.TotalPrice != null)
                    .SumAsync(o => o.TotalPrice ?? 0);

                // Lấy các tin nhắn liên hệ của khách hàng
                var contacts = await _context.Contact
                    .Where(c => c.AccountId == id)
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(5)
                    .ToListAsync();

                // Lấy tổng số tin nhắn liên hệ
                int totalContacts = await _context.Contact.CountAsync(c => c.AccountId == id);

                // Tạo CustomerViewModel để hiển thị
                var customerViewModel = new CustomerViewModel
                {
                    AccountId = account.AccountId,
                    Username = account.Username,
                    Email = account.Email,
                    Phone = account.Phone,
                    Address = account.Address,
                    JoinDate = null, // Nếu có trường này trong database
                    TotalOrders = totalOrders,
                    TotalSpent = totalSpent,
                    TotalContacts = totalContacts,
                    IsActive = true, // Giả sử mặc định tài khoản đang hoạt động
                    RecentOrders = orders,
                    RecentContacts = contacts
                };

                return View(customerViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải thông tin chi tiết khách hàng {CustomerId}", id);
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải thông tin khách hàng. Vui lòng thử lại sau.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Admin/Customer/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var account = await _context.Accounts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.AccountId == id && m.RoleId == 2);

                if (account == null)
                {
                    return NotFound();
                }

                return View(account);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải thông tin khách hàng {CustomerId} để chỉnh sửa", id);
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải thông tin khách hàng. Vui lòng thử lại sau.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Customer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AccountId,Username,Email,Phone,Address,RoleId,Password")] Account account)
        {
            if (id != account.AccountId)
            {
                return NotFound();
            }

            // Bỏ qua validation cho UserRole
            ModelState.Remove("UserRole");
            
            try
            {
                if (ModelState.IsValid)
                {
                    // Lấy tài khoản từ database
                    var existingAccount = await _context.Accounts.FindAsync(id);
                    if (existingAccount == null)
                    {
                        return NotFound();
                    }

                    // Chỉ cập nhật các trường cần thiết
                    existingAccount.Username = account.Username;
                    existingAccount.Email = account.Email;
                    existingAccount.Phone = account.Phone;
                    existingAccount.Address = account.Address;
                    // Password và RoleId giữ nguyên

                    // Log thông tin trước khi lưu
                    _logger.LogInformation($"Cập nhật khách hàng ID={id}: Username={existingAccount.Username}, Email={existingAccount.Email}");

                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation($"Đã cập nhật thành công khách hàng ID={id}");
                    TempData["SuccessMessage"] = "Cập nhật thông tin khách hàng thành công!";
                    return RedirectToAction(nameof(Details), new { id });
                }
                else
                {
                    foreach (var state in ModelState)
                    {
                        if (state.Value.Errors.Count > 0)
                        {
                            _logger.LogWarning($"Lỗi validation cho {state.Key}: {string.Join(", ", state.Value.Errors.Select(e => e.ErrorMessage))}");
                        }
                    }
                    return View(account);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật thông tin khách hàng {CustomerId}", id);
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi cập nhật thông tin khách hàng: {ex.Message}";
                return View(account);
            }
        }

        // GET: Admin/Customer/Orders/5
        public async Task<IActionResult> Orders(int? id, int? page)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // Kiểm tra khách hàng có tồn tại không
                var customer = await _context.Accounts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.AccountId == id && a.RoleId == 2);

                if (customer == null)
                {
                    return NotFound();
                }

                // Lấy tất cả đơn hàng của khách hàng
                var pageNumber = page ?? 1;
                var pageSize = 10;
                
                var ordersData = await _context.Orders
                    .Where(o => o.AccountId == id)
                    .OrderByDescending(o => o.OrderDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize).ToListAsync();
                
                var totalCount = await _context.Orders
                    .Where(o => o.AccountId == id).CountAsync();
                
                var orders = new StaticPagedList<Order>(
                    ordersData, pageNumber, pageSize, totalCount);
                
                ViewBag.Customer = customer;
                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách đơn hàng của khách hàng {CustomerId}", id);
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải danh sách đơn hàng. Vui lòng thử lại sau.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // Fix for the Contacts method
        public async Task<IActionResult> Contacts(int? id, int? page)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // Kiểm tra khách hàng có tồn tại không
                var customer = await _context.Accounts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.AccountId == id && a.RoleId == 2);

                if (customer == null)
                {
                    return NotFound();
                }

                // Lấy tất cả tin nhắn liên hệ của khách hàng
                var pageNumber = page ?? 1;
                var pageSize = 10;
                
                var contactsData = await _context.Contact
                    .Where(c => c.AccountId == id)
                    .OrderByDescending(c => c.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize).ToListAsync();
                
                var totalCount = await _context.Contact
                    .Where(c => c.AccountId == id).CountAsync();
                
                var contacts = new StaticPagedList<Contact>(
                    contactsData, pageNumber, pageSize, totalCount);
                
                ViewBag.Customer = customer;
                return View(contacts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách liên hệ của khách hàng {CustomerId}", id);
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải danh sách liên hệ. Vui lòng thử lại sau.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        private bool AccountExists(int id)
        {
            return _context.Accounts.Any(e => e.AccountId == id);
        }
    }
}