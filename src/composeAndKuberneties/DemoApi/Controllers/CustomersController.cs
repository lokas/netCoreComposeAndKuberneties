using AutoFixture;
using DemoApi.RestHateos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using RiskFirst.Hateoas;
using RiskFirst.Hateoas.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

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
        [HttpGet(Name = "GetAllValuesRoute")]
        public async Task<ItemsLinkContainer<Customer>> Get([FromQuery] int skip = 0)
        {
            const int pageSize = 5;
            var f = new Fixture();
            var cust = f.Build<Customer>()
                .Without(w => w.Links)
                .CreateMany(56)
                .ToList();

            foreach (var customer in cust)
            {
                await _linkService.AddLinksAsync(customer);
            }

            var result = new ItemsLinkContainer<Customer>
            {
                Items = cust.Skip(skip).Take(pageSize).ToList()
            };

            await _linkService.AddLinksAsync(result);
            var self = result.Links["self"];

            if (skip != 0)
            {
                result.AddLink("first", new Link
                {
                    Href = $"{self.Href}",
                    Rel = self.Rel,
                    Method = HttpMethod.Get.ToString()
                });
            }

            if (skip + pageSize > cust.Count)
            {
                result.AddLink("next", new Link
                {
                    Href = $"{self.Href}?skip={skip + pageSize}",
                    Rel = self.Rel,
                    Method = HttpMethod.Get.ToString()
                });
            }

            if (skip >= pageSize * 2)
            {
                result.AddLink("previous", new Link
                {
                    Href = $"{self.Href}?skip={skip - pageSize}",
                    Rel = self.Rel,
                    Method = HttpMethod.Get.ToString()
                });
            }

            result.AddLink("last", new Link
            {
                Href = $"{self.Href}?skip={cust.Count - (cust.Count % pageSize) }",
                Rel = self.Rel,
                Method = HttpMethod.Get.ToString()
            });

            return result;
        }

        // GET api/<CustomerController>/5
        [HttpGet("{id}", Name = "GetValueByIdRoute")]
        [Links(Policy = "FullInfoPolicy")]
        public async Task<Customer> Get(Guid id)
        {
            Fixture f = new Fixture();
            var c = f.Build<Customer>()
                .With(a => a.Id, id)
                .Without(w => w.Links)
                .Create();
            await _linkService.AddLinksAsync(c);
            return c;
        }

        // POST api/<CustomerController>
        [HttpPost(Name = "InsertValueRoute")]
        public async Task<IActionResult> Post([FromBody] Customer value)
        {
            value.Id = Guid.NewGuid();
            await _linkService.AddLinksAsync(value);

            return Accepted(new Uri($"{value.Links["self"].Href}/{value.Id}"));
        }

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