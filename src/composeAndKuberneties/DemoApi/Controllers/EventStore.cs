using EventStore.ClientAPI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SharpCompress.Archives;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DemoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventStore : ControllerBase
    {
        private readonly ILogger<EventStore> _logger;

        public EventStore(ILogger<EventStore> logger)
        {
            _logger = logger;
        }

        // GET: api/<EventStore>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new[] { "value1", "value2" };
        }

        // GET: api/<EventStore>
        [HttpGet]
        public IEnumerable<string> Get(string typeStream)
        {
            return new[] { "value1", "value2" };
        }


        public class TypeA
        {
            public Guid Id { get; set; }
            public string PhoneInfo { get; set; }

            public static TypeA Create
            {
                get
                {
                    Guid g = Guid.NewGuid();
                    return new TypeA
                    {
                        Id = g,
                        PhoneInfo = $"PhoneInfo_{g}"
                    };
                }
            }

            public const string StreamName = nameof(TypeA) + "_Aggregate";
        }

        public class TypeB
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Last { get; set; }

            public const string StreamName = nameof(TypeB) + "_Aggregate";

            public static TypeB Create
            {
                get
                {
                    Guid g = Guid.NewGuid();
                    return new TypeB
                    {
                        Id = g,
                        Name = $"Name_{g}",
                        Last = $"Last_{g}"
                    };
                }
            }
        }

        // POST api/<EventStore>
        [HttpPost]
        public async Task Post()
        {
            var a = TypeA.Create;
            var eventPayloadA = new EventData(eventId: a.Id, type: nameof(TypeA), isJson: true,
                data: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(a)),
                metadata: Encoding.UTF8.GetBytes("{}"));

            var b = TypeB.Create;
            var eventPayloadB = new EventData(eventId: b.Id, type: nameof(TypeB), isJson: true,
                data: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(b)),
                metadata: Encoding.UTF8.GetBytes("{}"));
            var uri = new Uri(EnvVariable.GetValue(EnvVariable.EventStore, _logger));
            var consetting = ConnectionSettings.Create().DisableTls().Build();
            using var conn = EventStoreConnection.Create(consetting, uri);
            //    var conn = EventStoreConnection.Create(uri);
            conn.Connected += Conn_Connected;
            conn.AuthenticationFailed += Conn_AuthenticationFailed;
            conn.Closed += Conn_Closed;
            await conn.ConnectAsync();


            var resA = await conn.AppendToStreamAsync(TypeA.StreamName, ExpectedVersion.Any, eventPayloadA);
            var resB = await conn.AppendToStreamAsync(TypeB.StreamName, ExpectedVersion.Any, eventPayloadB);

            _logger.LogInformation($"Info written to stream A:{resA} and B:{resB}");
            _logger.LogInformation($"Log info A:{resA.LogPosition} and B:{resB.LogPosition}");
            _logger.LogInformation($"Next versions A:{resA.NextExpectedVersion} and B:{resB.NextExpectedVersion}");
        }

        private void Conn_Closed(object sender, ClientClosedEventArgs e)
        {
            _logger.LogInformation("Conn_Closed! " + e.Reason);
        }

        private void Conn_AuthenticationFailed(object sender, ClientAuthenticationFailedEventArgs e)
        {
            _logger.LogInformation("AuthenticationFailed!");

        }

        private void Conn_Connected(object sender, ClientConnectionEventArgs e)
        {
            _logger.LogInformation("Connected to store!");
        }
    }
}
