using Motorbike.Models;
using System.Collections.Generic;

namespace Motorbike.Areas.Admin.Models
{
    public class DashboardViewModel
    {
        public int TotalOrders { get; set; }
        public int TotalProducts { get; set; }
        public int TotalCustomers { get; set; }
        public List<Order> RecentOrders { get; set; }
        public List<Contact> RecentContacts { get; set; }
    }
}