using System;
using System.Collections.Generic;

namespace Motorbike.ViewModels
{
    public class OrderHistoryViewModel
    {
        public List<OrderHistoryItemViewModel> Orders { get; set; } = new List<OrderHistoryItemViewModel>();
    }

    public class OrderHistoryItemViewModel
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
    }
}