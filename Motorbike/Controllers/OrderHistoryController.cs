using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Motorbike.Data;
using Motorbike.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Motorbike.Controllers
{
    public class OrderHistoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderHistoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Kiểm tra đăng nhập
            var accountId = HttpContext.Session.GetInt32("AccountId");
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy danh sách đơn hàng của tài khoản hiện tại
            var orders = await _context.Orders
                .Where(o => o.AccountId == accountId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderHistoryItemViewModel
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    DeliveryDate = o.DeliveryDate,
                    TotalPrice = o.TotalPrice ?? 0,
                    Status = o.Status
                })
                .ToListAsync();

            var viewModel = new OrderHistoryViewModel
            {
                Orders = orders
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            // Kiểm tra đăng nhập
            var accountId = HttpContext.Session.GetInt32("AccountId");
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy thông tin chi tiết đơn hàng
            var order = await _context.Orders
                .Where(o => o.OrderId == id && o.AccountId == accountId)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound();
            }

            // Lấy các sản phẩm trong đơn hàng
            var orderItems = await _context.OrderDetails
                .Where(od => od.OrderId == id)
                .Include(od => od.Motorbike)
                .Select(od => new OrderItemViewModel
                {
                    MotorbikeId = od.MotorbikeId,
                    MotorbikeName = od.Motorbike.MotorbikeName,
                    Image = od.Motorbike.Image,
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice
                })
                .ToListAsync();

            var viewModel = new OrderDetailViewModel
            {
                OrderId = order.OrderId,
                OrderDate = order.OrderDate,
                DeliveryDate = order.DeliveryDate,
                TotalPrice = order.TotalPrice ?? 0,
                Status = order.Status,
                CustomerName = order.CustomerName,
                Address = order.Address,
                Phone = order.Phone,
                PaymentMethod = order.PaymentMethod,
                Items = orderItems
            };

            return View(viewModel);
        }
    }
}