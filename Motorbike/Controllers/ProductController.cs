using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Motorbike.Data;
using Motorbike.Models;
using Motorbike.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Motorbike.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Product
        public async Task<IActionResult> Index(int brandId = 0, string sortOrder = "", string searchTerm = "", int page = 1)
        {
            int pageSize = 8; // Số sản phẩm mỗi trang
            
            // Lấy danh sách thương hiệu để hiển thị bộ lọc
            var brands = await _context.Brands.OrderBy(b => b.BrandName).ToListAsync();
            ViewBag.Brands = brands;
            ViewBag.CurrentBrandId = brandId;
            ViewBag.CurrentSortOrder = sortOrder;
            ViewBag.CurrentPage = page;
            ViewBag.SearchTerm = searchTerm;

            // Khởi tạo truy vấn
            IQueryable<Models.Motorbike> query = _context.Motorbikes
                .Include(m => m.Brand);

            // Tìm kiếm nếu có từ khóa
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(m => m.MotorbikeName.ToLower().Contains(searchTerm) ||
                                        (m.Brand.BrandName != null && m.Brand.BrandName.ToLower().Contains(searchTerm)) ||
                                        (m.Description != null && m.Description.ToLower().Contains(searchTerm)));
            }

            // Lọc theo thương hiệu nếu có
            if (brandId > 0)
            {
                query = query.Where(m => m.BrandId == brandId);
            }

            // Sắp xếp
            switch (sortOrder)
            {
                case "price_asc":
                    query = query.OrderBy(m => m.Price);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(m => m.Price);
                    break;
                case "name_asc":
                    query = query.OrderBy(m => m.MotorbikeName);
                    break;
                case "name_desc":
                    query = query.OrderByDescending(m => m.MotorbikeName);
                    break;
                case "popular":
                    query = query.OrderByDescending(m => m.TotalSold);
                    break;
                default:
                    query = query.OrderByDescending(m => m.MotorbikeId); // Mặc định sắp xếp theo ID giảm dần (mới nhất)
                    break;
            }

            // Tính tổng số trang
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Đảm bảo page không vượt quá tổng số trang
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            // Phân trang
            var motorbikes = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Tạo model để hiển thị
            var model = new ProductListViewModel
            {
                Motorbikes = motorbikes,
                CurrentPage = page,
                TotalPages = totalPages,
                BrandId = brandId,
                SortOrder = sortOrder,
                SearchTerm = searchTerm
            };

            return View(model);
        }
        
        // GET: /Product/Details/5
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

            // Lấy sản phẩm liên quan (cùng thương hiệu)
            var relatedProducts = await _context.Motorbikes
                .Include(m => m.Brand)
                .Where(m => m.BrandId == motorbike.BrandId && m.MotorbikeId != motorbike.MotorbikeId)
                .Take(4)
                .ToListAsync();

            var model = new ProductDetailViewModel
            {
                Motorbike = motorbike,
                RelatedProducts = relatedProducts
            };

            return View(model);
        }
    }
}