using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace DemoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RabbitPlay : ControllerBase
    {
        private const string Rabbitplay = "RabbitPlay";
        private const string Rabbitplayqueue = Rabbitplay + "Queue";
        private readonly ILogger<RabbitPlay> _logger;

        public RabbitPlay(ILogger<RabbitPlay> logger)
        {
            _logger = logger;
        }

        // GET: api/<RabbitPlay>
        [HttpGet]
        public IEnumerable<Message> Get()
        {
            List<Message> messages = new List<Message>();

            _logger.LogInformation("Reading rabbit!");
            using var con = Connection;
            using var model = con.CreateModel();
            model.QueueBind(Rabbitplayqueue, Rabbitplay, Rabbitplay, null);

            var result = model.BasicGet(Rabbitplayqueue, true);
            while (result != null)
            {
                var json = UTF8Encoding.UTF8.GetString(result.Body.ToArray());
                messages.Add(JsonSerializer.Deserialize<Message>(json));
                result = model.BasicGet(Rabbitplayqueue, true);
            }

            return messages;
        }

        private IConnection Connection
        {
            get
            {
                var uri = EnvVariable.GetValue(EnvVariable.Rabbit, _logger);
                return new ConnectionFactory
                {
                    Uri = new Uri(uri)

                }.CreateConnection();
            }
        }

        public class Message
        {
            public string Id { get; set; }
            public string Details { get; set; }
        }

        private static bool excQueueBind = false;
        // POST api/<RabbitPlay>
        [HttpPost]
        public void Post()
        {
            _logger.LogInformation("Sending to rabbit!");
            string append = Guid.NewGuid().ToString();
            var msg = new Message
            {
                Details = "Details_" + append,
                Id = "Id_" + append
            };

            using var con = Connection;
            using var model = con.CreateModel();

            if (!excQueueBind)
            {
                model.ExchangeDeclare(Rabbitplay, ExchangeType.Direct);
                model.QueueDeclare(Rabbitplayqueue, true, false, false);
                model.QueueBind(Rabbitplayqueue, Rabbitplay, Rabbitplay, null);
                excQueueBind = true;
            }

            model.BasicPublish(Rabbitplay, Rabbitplay, null,
                JsonSerializer.SerializeToUtf8Bytes(msg));
        }
    }
}