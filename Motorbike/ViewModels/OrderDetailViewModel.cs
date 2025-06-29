using System;
using System.Collections.Generic;

namespace Motorbike.ViewModels
{
    public class OrderDetailViewModel
    {
        public int OrderId { get; set; }
        public string OrderCode => $"#ORD{OrderId.ToString("D3")}";
        public DateTime OrderDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public string StatusClass
        {
            get
            {
                return Status?.ToLower() switch
                {
                    "hoàn thành" => "text-success",
                    "đang xử lý" => "text-warning", 
                    "đang giao" => "text-primary",
                    "đã hủy" => "text-danger",
                    _ => ""
                };
            }
        }
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string PaymentMethod { get; set; }
        
        public List<OrderItemViewModel> Items { get; set; } = new List<OrderItemViewModel>();
    }

    public class OrderItemViewModel
    {
        public int MotorbikeId { get; set; }
        public string MotorbikeName { get; set; }
        public string Image { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
    }
}