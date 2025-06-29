using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Motorbike.Data;
using Motorbike.Models;
using Motorbike.ViewModels;
using X.PagedList;

namespace Motorbike.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ContactController> _logger;

        public ContactController(ApplicationDbContext context, ILogger<ContactController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Contact
        public async Task<IActionResult> Index(ContactFilterViewModel filter, int? page)
        {
            try
            {
                _logger.LogInformation("Đang tải danh sách liên hệ");

                // Mặc định là trang 1 nếu không có tham số
                int pageNumber = page ?? filter.Page;
                int pageSize = filter.PageSize;

                // Bắt đầu truy vấn
                var contactsQuery = _context.Contact.AsNoTracking();

                // Áp dụng bộ lọc tìm kiếm
                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    string searchTerm = filter.SearchTerm.ToLower();
                    contactsQuery = contactsQuery.Where(c => 
                        c.Name.ToLower().Contains(searchTerm) || 
                        c.Email.ToLower().Contains(searchTerm) ||
                        c.Subject.ToLower().Contains(searchTerm) ||
                        (c.Phone != null && c.Phone.Contains(searchTerm))
                    );
                }

                // Lọc theo trạng thái
                if (!string.IsNullOrEmpty(filter.Status))
                {
                    contactsQuery = contactsQuery.Where(c => c.Status == filter.Status);
                }

                // Lọc theo khoảng thời gian
                if (filter.FromDate.HasValue)
                {
                    contactsQuery = contactsQuery.Where(c => c.CreatedAt >= filter.FromDate.Value);
                }

                if (filter.ToDate.HasValue)
                {
                    // Đảm bảo kết thúc ngày là cuối ngày
                    var endDate = filter.ToDate.Value.AddDays(1).AddTicks(-1);
                    contactsQuery = contactsQuery.Where(c => c.CreatedAt <= endDate);
                }

                // Áp dụng sắp xếp
                switch (filter.SortOrder)
                {
                    case "date_asc":
                        contactsQuery = contactsQuery.OrderBy(c => c.CreatedAt);
                        break;
                    case "name":
                        contactsQuery = contactsQuery.OrderBy(c => c.Name);
                        break;
                    case "name_desc":
                        contactsQuery = contactsQuery.OrderByDescending(c => c.Name);
                        break;
                    case "status":
                        contactsQuery = contactsQuery.OrderBy(c => c.Status);
                        break;
                    default:
                        contactsQuery = contactsQuery.OrderByDescending(c => c.CreatedAt);
                        break;
                }

                // Lấy danh sách trạng thái cho dropdown
                ViewBag.StatusList = await _context.Contact
                    .Select(c => c.Status)
                    .Distinct()
                    .Where(s => s != null)
                    .ToListAsync();

                // Lấy danh sách liên hệ với phân trang
                var contactsData = await contactsQuery
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                    
                var totalCount = await contactsQuery.CountAsync();
                var contacts = new StaticPagedList<Contact>(
                    contactsData, pageNumber, pageSize, totalCount);

                // Lưu lại thông tin filter cho view
                ViewBag.CurrentFilter = filter;

                _logger.LogInformation("Đã tải danh sách liên hệ thành công");
                return View(contacts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách liên hệ");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải danh sách liên hệ. Vui lòng thử lại sau.";
                var emptyList = new StaticPagedList<Contact>(
                    new List<Contact>(), 1, 10, 0);
                return View(emptyList);
            }
        }

        // GET: Admin/Contact/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var contact = await _context.Contact
                    .Include(c => c.Account)
                    .FirstOrDefaultAsync(c => c.ContactId == id);

                if (contact == null)
                {
                    return NotFound();
                }

                return View(contact);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xem chi tiết liên hệ {ContactId}", id);
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải thông tin liên hệ. Vui lòng thử lại sau.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Admin/Contact/Respond/5
        public async Task<IActionResult> Respond(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var contact = await _context.Contact
                    .Include(c => c.Account)
                    .FirstOrDefaultAsync(c => c.ContactId == id);

                if (contact == null)
                {
                    return NotFound();
                }

                var viewModel = new ContactResponseViewModel
                {
                    ContactId = contact.ContactId,
                    Response = contact.Response,
                    Status = contact.Status
                };

                ViewBag.Contact = contact;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải form phản hồi liên hệ {ContactId}", id);
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải thông tin liên hệ. Vui lòng thử lại sau.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Contact/Respond/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Respond(int id, ContactResponseViewModel viewModel)
        {
            if (id != viewModel.ContactId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var contact = await _context.Contact.FindAsync(id);
                    if (contact == null)
                    {
                        return NotFound();
                    }

                    contact.Response = viewModel.Response;
                    contact.Status = viewModel.Status;
                    contact.ResponseAt = DateTime.Now;

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Phản hồi liên hệ thành công!";
                    return RedirectToAction(nameof(Details), new { id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi phản hồi liên hệ {ContactId}", id);
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi lưu phản hồi. Vui lòng thử lại.");
                }
            }

            try
            {
                var contact = await _context.Contact
                    .Include(c => c.Account)
                    .FirstOrDefaultAsync(c => c.ContactId == id);
                ViewBag.Contact = contact;
            }
            catch
            {
                ViewBag.Contact = null;
            }

            return View(viewModel);
        }

        // POST: Admin/Contact/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            try
            {
                _logger.LogInformation("Đang cập nhật trạng thái cho liên hệ ID={ContactId} thành {Status}", id, status);
                
                var contact = await _context.Contact.FindAsync(id);
                if (contact == null)
                {
                    _logger.LogWarning("Không tìm thấy liên hệ ID={ContactId}", id);
                    return Json(new { success = false, message = "Không tìm thấy liên hệ." });
                }

                // Lưu trạng thái cũ để log
                var oldStatus = contact.Status;
                
                // Cập nhật trạng thái mới
                contact.Status = status;
                
                // Nếu chuyển sang trạng thái "Đã xử lý" mà chưa có phản hồi
                if (status == "Đã xử lý" && string.IsNullOrEmpty(contact.Response))
                {
                    contact.Response = "Đã xử lý yêu cầu của bạn.";
                    contact.ResponseAt = DateTime.Now;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã cập nhật trạng thái liên hệ ID={ContactId} từ {OldStatus} thành {NewStatus}", 
                    id, oldStatus, status);
                
                return Json(new { success = true, message = "Cập nhật trạng thái thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái liên hệ {ContactId}", id);
                return Json(new { success = false, message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }

        // POST: Admin/Contact/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                _logger.LogInformation("Đang xóa liên hệ ID={ContactId}", id);
                
                var contact = await _context.Contact.FindAsync(id);
                if (contact == null)
                {
                    _logger.LogWarning("Không tìm thấy liên hệ ID={ContactId} để xóa", id);
                    return Json(new { success = false, message = "Không tìm thấy liên hệ." });
                }

                _context.Contact.Remove(contact);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Đã xóa thành công liên hệ ID={ContactId}", id);
                return Json(new { success = true, message = "Xóa liên hệ thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa liên hệ {ContactId}", id);
                return Json(new { success = false, message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }
    }
}