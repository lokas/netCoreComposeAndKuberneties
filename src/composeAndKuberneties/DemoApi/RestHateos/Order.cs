using System;
using System.Collections.Generic;

namespace DemoApi.RestHateos
{
    public class Order
    {
        public Guid Id { get; set; }
        public DateTime CreateAt { get; set; }
        //todo: zanima me kako ide za order items
        public List<OrderItems> Items { get; set; } //how to model this madness 
    }

    public class OrderItems
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public Decimal AtPrice { get; set; }
    }
}