using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Order_Management_System.Models
{
    public class OrderViewModel
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public long? MobileNumber { get; set; }
        public string ShippingAddress { get; set; }
        public string OrderStatus { get; set; }
        public string ProductName { get; set; }
        public int? OrderStatusId { get; set; }
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
    }
}