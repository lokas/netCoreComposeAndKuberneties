using System;
using RiskFirst.Hateoas.Models;

namespace DemoApi.RestHateos
{
    public class Customer : LinkContainer
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
    }
}