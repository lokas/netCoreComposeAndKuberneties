using RiskFirst.Hateoas.Models;
using System;
using System.Collections.Generic;

namespace DemoApi.RestHateos
{
    public class Order : LinkContainer
    {
        public Order()
        {
            Id = Guid.NewGuid();
            CreateAt = DateTime.Now;
        }

        public Guid Id { get; set; }
        public DateTime CreateAt { get; set; }

        public Guid ForCustomerId { get; set; }
        public string OrderName { get; set; }
    }

    public class OrderDetails : Order
    {
        public List<OrderItem> Items { get; set; } //how to model this madness   
    }

    public class OrderItem : LinkContainer
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public Decimal AtPrice { get; set; }
    }
}