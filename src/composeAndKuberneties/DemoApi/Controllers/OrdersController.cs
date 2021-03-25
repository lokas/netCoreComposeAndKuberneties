using AutoFixture;
using DemoApi.RestHateos;
using Microsoft.AspNetCore.Mvc;
using RiskFirst.Hateoas;
using System;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DemoApi.Controllers
{
    [Route("api/Customers/{customerId}/Orders")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ILinksService _linkService;

        public OrdersController(ILinksService linkService)
        {
            _linkService = linkService;
        }

        [HttpGet("{id}", Name = "GetOrderByIdRoute")]
        //[Links(Policy = "OrderFullInfoPolicy")] has to be same name for crap to work e.g. throws exception customer does not have linked policy although u want default one (the self link)
        [Links(Policy = "FullInfoPolicy")] //if you want to add parent with same service needs to have same policy name 
        public async Task<Order> Get([FromRoute]Guid customerId, Guid id)
        {
            Fixture f = new Fixture();
            var order = f.Build<Order>()
                .With(a => a.Id, id)
                .With(a=>a.ForCustomerId, customerId)
                .Without(w => w.Links)
                .Create();

            var customer = new Customer {Id = customerId};
            await _linkService.AddLinksAsync(order);
            await _linkService.AddLinksAsync(customer);
            //if you add policy with the same name in this case FullInfoPolicy,
            //then  this use self from order, so stupid its made for simple crud behaviour out of the box
            //but if you are explicit in the full info policy it all works nuts

            order.AddLink("parent",customer.Links["self"]);

            return order;
        }

       [HttpGet("{id}/Items", Name = "GetOrderItemsByIdRoute")]
       public async Task<Order> GetItems([FromRoute] Guid customerId, Guid id)
       {
           Fixture f = new Fixture();
           var order = f.Build<OrderDetails>()
               .With(a => a.Id, id)
               .With(a => a.ForCustomerId, customerId)
               .Without(w => w.Links)
               .Create();

           var parent = new Order { Id = id,ForCustomerId = customerId};
           await _linkService.AddLinksAsync(order);
           await _linkService.AddLinksAsync(parent);

           order.AddLink("parent", parent.Links["self"]);

           return order;
       }
       //and to make it nice I would need item controller
    }
}