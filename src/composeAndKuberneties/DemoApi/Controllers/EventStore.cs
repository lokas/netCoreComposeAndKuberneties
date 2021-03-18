using EventStore.ClientAPI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DemoApi.Domain;

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

        //create projection 
        //create 

        // GET: api/<EventStore>
        [HttpGet]
        public async Task<OkObjectResult> Get([FromQuery] string typeStream)
        {
            return typeStream switch
            {
                nameof(TypeA) => Ok(await GetEvents<TypeA>(TypeA.StreamName)),
                nameof(TypeB) => Ok(await GetEvents<TypeB>(TypeB.StreamName)),
                _ => throw new InvalidOperationException($"Unknown type provided:{typeStream}")
            };
        }

        private async Task<IEnumerable<T>> GetEvents<T>(string streamName)
        {
            var streamEvents = new List<ResolvedEvent>();
            using var connection = GetEventStoreConnection;
            await connection.ConnectAsync();

            StreamEventsSlice currentSlice;
            long nextSliceStart = StreamPosition.Start;
            do
            {
                currentSlice = await connection.ReadStreamEventsForwardAsync(
                    streamName, nextSliceStart, 200, false);

                nextSliceStart = currentSlice.NextEventNumber;

                streamEvents.AddRange(currentSlice.Events);

            } while (!currentSlice.IsEndOfStream);

            return streamEvents
                .Select(e => JsonSerializer.Deserialize<T>(e.Event.Data));
        }


        private IEventStoreConnection GetEventStoreConnection
        {
            get
            {
                //    var conn = EventStoreConnection.Create(uri); bug on local host with tls
                var uri = new Uri(EnvVariable.GetValue(EnvVariable.EventStore, _logger));
                var settings = ConnectionSettings.Create().DisableTls().Build();
                var con = EventStoreConnection.Create(settings, uri);
                con.Connected += Conn_Connected;
                con.AuthenticationFailed += Conn_AuthenticationFailed;
                con.Closed += Conn_Closed;
                return con;
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

            using var conn = GetEventStoreConnection;
          
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