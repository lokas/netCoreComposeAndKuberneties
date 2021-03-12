using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

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

            return new MongoClient(new MongoUrl(uri))
               .GetDatabase("MongoPlay")
               .GetCollection<DummyClass>("HowMuchDummy");
        }

        public class DummyClass
        {
            [BsonId]
            public ObjectId Key { get; set; }
            public string Info { get; set; }
            public string Details { get; set; }
        }

        // POST api/<MongoPlay>
        [HttpPost]
        public void Post()
        {
            var collection = GetCollection();
            var id = Guid.NewGuid();
            collection.InsertOne(new DummyClass
            {
                Info = "Info_" + id,
                Details = "Details_" + id
            });
        }
    }
}
