using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using DemoApi.RestHateos;
using Microsoft.AspNetCore.Mvc;
using RiskFirst.Hateoas;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DemoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ILinksService _linkService;

        public CustomersController(ILinksService linkService)
        {
            _linkService = linkService;
        }

        // GET: api/<CustomerController>
        [HttpGet(Name = "all")]
        public async Task<IEnumerable<Customer>> Get()
        {
            //TODO: how to do pagging 
            Fixture f = new Fixture();
            var cust = f.Build<Customer>().Without(w => w.Links).CreateMany(20);

            foreach (var c in cust)
            {
                await _linkService.AddLinksAsync(c);
            }

            return cust;

        }

        // GET api/<CustomerController>/5
        [HttpGet("{id}", Name = "self")]
        public async Task<Customer> Get(Guid id)
        {
            Fixture f = new Fixture();
            var c = f.Build<Customer>()
                .With(a => a.Id, id)
                .Without(w=>w.Links)
                .Create();
            await _linkService.AddLinksAsync(c);
            return c;
        }

        //// POST api/<CustomerController>
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{

        //}

        //// PUT api/<CustomerController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{

        //}

        //// DELETE api/<CustomerController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}