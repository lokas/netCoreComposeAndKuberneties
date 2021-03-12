using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace DemoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MongoPlay : ControllerBase
    {
        private readonly ILogger<MongoPlay> _logger;

        public MongoPlay(ILogger<MongoPlay> logger)
        {
            _logger = logger;
        }

        // GET: api/<MongoPlay>
        [HttpGet]
        public IEnumerable<DummyClass> Get()
        {
            _logger.LogInformation("Getting from mongo information!");
            return GetCollection()
                .FindSync(f => true)
                .ToList();
        }

        private IMongoCollection<DummyClass> GetCollection()
        {
            var uri = Environment.GetEnvironmentVariable("MONGO_URI");
            _logger.LogInformation($"Mongo Uri: {uri}");
            if (string.IsNullOrEmpty(uri))
            {
                _logger.LogCritical("Missing mongoURI");
                throw new MongoConfigurationException("URI -> NOT SET");
            }
            return new MongoClient()
                .GetDatabase("MongoPlay")
                .GetCollection<DummyClass>("HowMuchDummy");
        }

        public class DummyClass
        {
            public DummyClass()
            {
                Info = "Info_" + Guid.NewGuid();
                Details = "Details_" + Guid.NewGuid();
            }

            public string Info { get; }
            public string Details { get; }
        }

        // POST api/<MongoPlay>
        [HttpPost]
        public void Post()
        {
            var collection = GetCollection();
            collection.InsertOne(new DummyClass());
        }
    }
}
