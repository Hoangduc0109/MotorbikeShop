using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Motorbike.Data;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;
using X.PagedList.Extensions;
using X.PagedList.Mvc.Core;
using MotorbikeModel = Motorbike.Models.Motorbike;
using System.Text;

namespace Motorbike.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MotorbikeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<MotorbikeController> _logger;

        public MotorbikeController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, ILogger<MotorbikeController> logger)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        // GET: Admin/Motorbike
        public async Task<IActionResult> Index(string searchString, int? page, int? brandId)
        {
            int pageNumber = page ?? 1;
            int pageSize = 10;

            var motorbikesQuery = _context.Motorbikes
                .Include(m => m.Brand)
                .AsQueryable();

            // Tìm kiếm theo tên xe
            if (!string.IsNullOrEmpty(searchString))
            {
                motorbikesQuery = motorbikesQuery.Where(m => m.MotorbikeName.Contains(searchString));
                ViewData["CurrentFilter"] = searchString;
            }

            // Lọc theo thương hiệu
            if (brandId.HasValue)
            {
                motorbikesQuery = motorbikesQuery.Where(m => m.BrandId == brandId);
                ViewData["SelectedBrand"] = brandId;
            }

            // Lấy danh sách thương hiệu cho dropdown lọc
            ViewBag.Brands = await _context.Brands.ToListAsync();

            var motorbikes = motorbikesQuery.OrderByDescending(m => m.MotorbikeId)
                .ToPagedList(pageNumber, pageSize);

            return View(motorbikes);
        }

        // GET: Admin/Motorbike/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var motorbike = await _context.Motorbikes
                .Include(m => m.Brand)
                .FirstOrDefaultAsync(m => m.MotorbikeId == id);

            if (motorbike == null)
            {
                return NotFound();
            }

            return View(motorbike);
        }

        // GET: Admin/Motorbike/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Brands = new SelectList(await _context.Brands.ToListAsync(), "BrandId", "BrandName");
            return View();
        }

        // POST: Admin/Motorbike/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MotorbikeName,BrandId,Price,Stock,TotalSold,Description")] MotorbikeModel motorbike, IFormFile imageFile)
        {
            // Bỏ qua validation cho cả Image và imageFile
            ModelState.Remove("Image");
            ModelState.Remove("imageFile"); // Thêm dòng này
            
            try
            {
                if (ModelState.IsValid)
                {
                    // Đảm bảo TotalSold không null
                    motorbike.TotalSold = motorbike.TotalSold ?? 0;
                    
                    // Xử lý upload hình ảnh
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }

                        motorbike.Image = uniqueFileName;
                    }
                    else
                    {
                        // Gán ảnh mặc định
                        motorbike.Image = "default.jpg";
                    }

                    // Đảm bảo encoding UTF-8 cho Description
                    if (motorbike.Description != null)
                    {
                        
                        motorbike.Description = motorbike.Description.Trim();
                    }

                    _context.Add(motorbike);
                    await _context.SaveChangesAsync();
                    
                    // Đảm bảo message luôn có giá trị rõ ràng
                    return Json(new { 
                        success = true, 
                        message = "Thêm xe máy thành công!", 
                        id = motorbike.MotorbikeId // Thêm ID để frontend có thể sử dụng
                    });
                }
                else // Thêm else ở đây để đảm bảo mọi đường dẫn đều có return
                {
                    // Lấy chi tiết lỗi từ mỗi trường
                    var errorDetails = new Dictionary<string, string[]>();
                    foreach (var key in ModelState.Keys)
                    {
                        var errors = ModelState[key].Errors.Select(e => e.ErrorMessage).ToArray();
                        if (errors.Any())
                        {
                            errorDetails[key] = errors;
                        }
                    }
                    
                    // Log chi tiết lỗi để điều tra
                    var errorMessages = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    _logger.LogWarning($"ModelState không hợp lệ: {errorMessages}");
                    
                    return Json(new { 
                        success = false, 
                        message = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.",
                        errors = errorDetails
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm xe máy");
                return Json(new { success = false, message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
            
        }

        // GET: Admin/Motorbike/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var motorbike = await _context.Motorbikes.FindAsync(id);
            if (motorbike == null)
            {
                return NotFound();
            }
            
            ViewBag.Brands = new SelectList(await _context.Brands.ToListAsync(), "BrandId", "BrandName", motorbike.BrandId);
            return View(motorbike);
        }

        // POST: Admin/Motorbike/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MotorbikeId,MotorbikeName,BrandId,Price,Stock,TotalSold,Description,Image")] MotorbikeModel motorbike, IFormFile imageFile)
        {
            if (id != motorbike.MotorbikeId)
            {
                return Json(new { success = false, message = "Không tìm thấy xe máy." });
            }

                // Bỏ qua validation cho cả Image và imageFile
            ModelState.Remove("Image");
            ModelState.Remove("imageFile"); // Thêm dòng này

            try
            {
                if (ModelState.IsValid)
                {
                    // Lấy xe máy hiện tại từ database
                    var existingMotorbike = await _context.Motorbikes.AsNoTracking()
                        .FirstOrDefaultAsync(m => m.MotorbikeId == id);
                        
                    if (existingMotorbike == null)
                    {
                        return Json(new { success = false, message = "Không tìm thấy xe máy." });
                    }

                    // Xử lý upload hình ảnh mới nếu có
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }

                        // Xóa ảnh cũ nếu không phải ảnh mặc định
                        if (!string.IsNullOrEmpty(existingMotorbike.Image) && existingMotorbike.Image != "default.jpg")
                        {
                            string oldFilePath = Path.Combine(uploadsFolder, existingMotorbike.Image);
                            try
                            {
                                if (System.IO.File.Exists(oldFilePath))
                                {
                                    // Thử đổi tên file thay vì xóa trực tiếp
                                    string tempFileName = $"temp_{Guid.NewGuid()}_{Path.GetFileName(oldFilePath)}";
                                    string tempFilePath = Path.Combine(uploadsFolder, tempFileName);
                                    
                                    System.IO.File.Move(oldFilePath, tempFilePath);
                                    
                                    Task.Run(async () => {
                                        try {
                                            await Task.Delay(5000); // chờ 5 giây
                                            if (System.IO.File.Exists(tempFilePath))
                                            {
                                                System.IO.File.Delete(tempFilePath);
                                            }
                                        } catch (Exception ex) {
                                            _logger.LogError(ex, "Không thể xóa file tạm");
                                        }
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning($"Không thể xử lý file cũ: {ex.Message}. Tiếp tục với tên file mới.");
                                // Không báo lỗi, chỉ tiếp tục với file mới
                            }
                        }

                        motorbike.Image = uniqueFileName;
                    }
                    else
                    {
                        // Giữ nguyên hình ảnh cũ
                        motorbike.Image = existingMotorbike.Image;
                    }

                    // Đảm bảo encoding UTF-8 cho Description
                    if (motorbike.Description != null)
                    {
                        // Thay vì chuyển đổi từ Encoding.Default sang UTF-8, giữ nguyên chuỗi
                        // Vì Javascript đã encode trước khi gửi
                        // byte[] bytes = Encoding.Default.GetBytes(motorbike.Description);
                        // motorbike.Description = Encoding.UTF8.GetString(bytes);
                        
                        // Đơn giản là giữ nguyên giá trị
                        motorbike.Description = motorbike.Description.Trim();
                    }

                    _context.Update(motorbike);
                    await _context.SaveChangesAsync();
                    
                    // Thêm thông báo vào TempData
                    TempData["SuccessMessage"] = "Cập nhật thông tin xe máy thành công!";
                    
                    return Json(new { success = true, message = "Cập nhật thông tin xe máy thành công!" });
                }
                
                // Log chi tiết lỗi
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                _logger.LogWarning($"ModelState không hợp lệ: {errors}");
                
                return Json(new { success = false, message = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật xe máy");
                return Json(new { success = false, message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }

        // GET: Admin/Motorbike/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var motorbike = await _context.Motorbikes
                .Include(m => m.Brand)
                .FirstOrDefaultAsync(m => m.MotorbikeId == id);
                
            if (motorbike == null)
            {
                return NotFound();
            }

            return View(motorbike);
        }

        // POST: Admin/Motorbike/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var motorbike = await _context.Motorbikes.FindAsync(id);
                
                if (motorbike == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy xe máy!";
                    return RedirectToAction(nameof(Index));
                }
                
                // Kiểm tra xem xe có trong order_details không
                var hasOrders = await _context.OrderDetails.AnyAsync(od => od.MotorbikeId == id);
                if (hasOrders)
                {
                    TempData["ErrorMessage"] = "Không thể xóa xe máy này vì đã có trong đơn hàng!";
                    return RedirectToAction(nameof(Index));
                }

                // Xóa hình ảnh kèm theo nếu không phải ảnh mặc định
                if (!string.IsNullOrEmpty(motorbike.Image) && motorbike.Image != "default.jpg")
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
                    string filePath = Path.Combine(uploadsFolder, motorbike.Image);
                    
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Motorbikes.Remove(motorbike);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Xóa xe máy thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa xe máy ID={Id}", id);
                TempData["ErrorMessage"] = $"Lỗi khi xóa xe máy: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Motorbike/DeleteAjax/5
        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> DeleteAjax(int id)
        {
            try
            {
                _logger.LogInformation("Đang xóa xe máy ID={Id}", id);
                
                var motorbike = await _context.Motorbikes.FindAsync(id);
                if (motorbike == null)
                {
                    _logger.LogWarning("Không tìm thấy xe máy ID={Id}", id);
                    return Json(new { success = false, message = "Không tìm thấy xe máy!" });
                }

                // Kiểm tra xem xe có trong order_details không
                var hasOrders = await _context.OrderDetails.AnyAsync(od => od.MotorbikeId == id);
                if (hasOrders)
                {
                    _logger.LogWarning("Không thể xóa xe máy ID={Id} vì đã có trong đơn hàng", id);
                    return Json(new { success = false, message = "Không thể xóa xe máy này vì đã có trong đơn hàng!" });
                }

                // Xóa hình ảnh kèm theo nếu không phải ảnh mặc định
                if (!string.IsNullOrEmpty(motorbike.Image) && motorbike.Image != "default.jpg")
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
                    string filePath = Path.Combine(uploadsFolder, motorbike.Image);
                    
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Motorbikes.Remove(motorbike);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Đã xóa thành công xe máy ID={Id}", id);
                return Json(new { success = true, message = "Xóa xe máy thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa xe máy ID={Id}: {Message}", id, ex.Message);
                return Json(new { success = false, message = $"Lỗi khi xóa xe máy: {ex.Message}" });
            }
        }

        private bool MotorbikeExists(int id)
        {
            return _context.Motorbikes.Any(e => e.MotorbikeId == id);
        }
    }
}