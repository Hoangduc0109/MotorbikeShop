using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Motorbike.Data;
using Motorbike.Models;
using X.PagedList;
using X.PagedList.Mvc.Core;
using System.IO;
using X.PagedList.Extensions;

namespace Motorbike.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BrandController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<BrandController> _logger;

        public BrandController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, ILogger<BrandController> logger)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        // Update the problematic line in the Index method
        public async Task<IActionResult> Index(string searchString, int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 10;

            ViewData["CurrentFilter"] = searchString;

            // Thêm Include để tải Motorbikes cùng với Brand
            var brands = from b in _context.Brands.Include(b => b.Motorbikes)
                         select b;

            if (!string.IsNullOrEmpty(searchString))
            {
                brands = brands.Where(b => b.BrandName.Contains(searchString));
            }

            var pagedBrands = await brands.OrderBy(b => b.BrandId).ToListAsync();
            return View(pagedBrands.ToPagedList(pageNumber, pageSize));
        }

        // GET: Admin/Brand/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var brand = await _context.Brands
                .Include(b => b.Motorbikes)
                .FirstOrDefaultAsync(m => m.BrandId == id);
                
            if (brand == null)
            {
                return NotFound();
            }

            return View(brand);
        }

        // GET: Admin/Brand/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Brand/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BrandName")] Brand brand, IFormFile logoFile)
        {
            ModelState.Remove("Logo");
            
            try
            {
                if (ModelState.IsValid)
                {
                    if (logoFile != null && logoFile.Length > 0)
                    {
                        // Thay đổi từ "brands" thành "category"
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "category");
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + logoFile.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await logoFile.CopyToAsync(fileStream);
                        }

                        brand.Logo = uniqueFileName;
                    }
                    else
                    {
                        // Thay đổi từ "default_brand.png" thành "default.jpg"
                        brand.Logo = "default.jpg";
                    }

                    _context.Add(brand);
                    await _context.SaveChangesAsync();
                    
                    // Đảm bảo trả về đúng định dạng
                    return new JsonResult(new { 
                        success = true, 
                        message = "Thêm thương hiệu thành công!",
                        id = brand.BrandId 
                    });
                }
                else
                {
                    var errorDetails = new Dictionary<string, string[]>();
                    foreach (var key in ModelState.Keys)
                    {
                        var errors = ModelState[key].Errors.Select(e => e.ErrorMessage).ToArray();
                        if (errors.Any())
                        {
                            errorDetails[key] = errors;
                        }
                    }
                    
                    var errorMessages = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    _logger.LogWarning($"ModelState không hợp lệ: {errorMessages}");
                    
                    return new JsonResult(new { 
                        success = false, 
                        message = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.",
                        errors = errorDetails
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm thương hiệu");
                return new JsonResult(new { 
                    success = false, 
                    message = $"Đã xảy ra lỗi: {ex.Message}" 
                });
            }
        }

        // GET: Admin/Brand/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                return NotFound();
            }
            
            return View(brand);
        }

        // POST: Admin/Brand/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BrandId,BrandName,Logo")] Brand brand, IFormFile logoFile)
        {
            if (id != brand.BrandId)
            {
                return Json(new { success = false, message = "Không tìm thấy thương hiệu." });
            }

            ModelState.Remove("Logo");
            ModelState.Remove("logoFile");

            try
            {
                if (ModelState.IsValid)
                {
                    var existingBrand = await _context.Brands.AsNoTracking()
                        .FirstOrDefaultAsync(b => b.BrandId == id);
                        
                    if (existingBrand == null)
                    {
                        return Json(new { success = false, message = "Không tìm thấy thương hiệu." });
                    }

                    if (logoFile != null && logoFile.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "category");
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + logoFile.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await logoFile.CopyToAsync(fileStream);
                        }

                        if (!string.IsNullOrEmpty(existingBrand.Logo) && existingBrand.Logo != "default.jpg")
                        {
                            string oldFilePath = Path.Combine(uploadsFolder, existingBrand.Logo);
                            try
                            {
                                if (System.IO.File.Exists(oldFilePath))
                                {
                                    string tempFileName = $"temp_{Guid.NewGuid()}_{Path.GetFileName(oldFilePath)}";
                                    string tempFilePath = Path.Combine(uploadsFolder, tempFileName);
                                    
                                    System.IO.File.Move(oldFilePath, tempFilePath);
                                    
                                    Task.Run(async () => {
                                        try {
                                            await Task.Delay(5000);
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
                                _logger.LogWarning($"Không thể xử lý file logo cũ: {ex.Message}. Tiếp tục với tên file mới.");
                            }
                        }

                        brand.Logo = uniqueFileName;
                    }
                    else
                    {
                        brand.Logo = existingBrand.Logo;
                    }

                    _context.Update(brand);
                    await _context.SaveChangesAsync();
                    
                    return Json(new { 
                        success = true, 
                        message = "Cập nhật thương hiệu thành công!" 
                    });
                }
                else
                {
                    var errorDetails = new Dictionary<string, string[]>();
                    foreach (var key in ModelState.Keys)
                    {
                        var errors = ModelState[key].Errors.Select(e => e.ErrorMessage).ToArray();
                        if (errors.Any())
                        {
                            errorDetails[key] = errors;
                        }
                    }
                    
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
                _logger.LogError(ex, "Lỗi khi cập nhật thương hiệu");
                return Json(new { success = false, message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }

        // GET: Admin/Brand/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var brand = await _context.Brands
                .FirstOrDefaultAsync(m => m.BrandId == id);
                
            if (brand == null)
            {
                return NotFound();
            }

            return View(brand);
        }

        // POST: Admin/Brand/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var brand = await _context.Brands.FindAsync(id);
                if (brand == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thương hiệu." });
                }

                // Kiểm tra xem thương hiệu có đang được sử dụng không
                var hasMotorbikes = await _context.Motorbikes.AnyAsync(m => m.BrandId == id);
                if (hasMotorbikes)
                {
                    return Json(new { 
                        success = false, 
                        message = "Không thể xóa thương hiệu này vì đang có xe máy sử dụng!" 
                    });
                }

                // Xóa file logo nếu có
                if (!string.IsNullOrEmpty(brand.Logo) && brand.Logo != "default.jpg")
                {
                    string logoPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "category", brand.Logo);
                    if (System.IO.File.Exists(logoPath))
                    {
                        System.IO.File.Delete(logoPath);
                    }
                }

                _context.Brands.Remove(brand);
                await _context.SaveChangesAsync();
                
                return Json(new { success = true, message = "Xóa thương hiệu thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa thương hiệu");
                return Json(new { success = false, message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }

        // AJAX action để xóa thương hiệu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            try
            {
                var brand = await _context.Brands.FindAsync(id);
                if (brand == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thương hiệu." });
                }

                // Kiểm tra xem thương hiệu có đang được sử dụng không
                var hasMotorbikes = await _context.Motorbikes.AnyAsync(m => m.BrandId == id);
                if (hasMotorbikes)
                {
                    return Json(new { 
                        success = false, 
                        message = "Không thể xóa thương hiệu này vì đang có xe máy sử dụng!" 
                    });
                }

                // Xóa file logo nếu có
                if (!string.IsNullOrEmpty(brand.Logo) && brand.Logo != "default.jpg")
                {
                    string logoPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "category", brand.Logo);
                    if (System.IO.File.Exists(logoPath))
                    {
                        System.IO.File.Delete(logoPath);
                    }
                }

                _context.Brands.Remove(brand);
                await _context.SaveChangesAsync();
                
                return Json(new { success = true, message = "Xóa thương hiệu thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa thương hiệu");
                return Json(new { success = false, message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }
    }
}