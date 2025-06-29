using System;
using System.Collections.Generic;
using Motorbike.Models;

namespace Motorbike.ViewModels
{
    public class CustomerViewModel
    {
        public int AccountId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public DateTime? JoinDate { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public int TotalContacts { get; set; }
        public bool IsActive { get; set; } = true;
        public List<Order> RecentOrders { get; set; }
        public List<Contact> RecentContacts { get; set; }
    }
    
    public class CustomerFilterViewModel
    {
        public string SearchTerm { get; set; }
        public string SortOrder { get; set; }
        public bool? IsActive { get; set; }
        public int? MinOrders { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}