using DemoApi.Domain;
using EventStore.ClientAPI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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

        //create projection -> so we do have server side and client side (catch-up and persisten stream subscriptions)   

        // GET: api/<EventStore>
        [HttpGet]
        public async Task<OkObjectResult> Get([FromQuery] Guid id)
        {
            return Ok(await GetEvents($"{nameof(Aggregate)}-{id}"));
        }

        private async Task<IEnumerable<object>> GetEvents(string streamName)
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
                .Select(e => Newtonsoft.Json.JsonConvert.DeserializeObject(
                     UTF8Encoding.UTF8.GetString(e.Event.Data), Type.GetType(e.Event.EventType)));
        }


        private IEventStoreConnection GetEventStoreConnection
        {
            get
            {
                //    var conn = EventStoreConnection.Create(uri); not working on local host with tls
                var uri = new Uri(EnvVariable.GetValue(EnvVariable.EventStore, _logger));
                var settings = ConnectionSettings.Create().DisableTls().Build();
                var con = EventStoreConnection.Create(settings, uri);
                con.Connected += Conn_Connected;
                con.AuthenticationFailed += Conn_AuthenticationFailed;
                con.Closed += Conn_Closed;
                return con;
            }
        }

        public class PostData
        {
            public Guid Id { get; set; }
        }

        [HttpPut]
        public async Task Put()
        {
            var agr = new Aggregate();
            var seed = Guid.NewGuid();
            agr.AddPhoneInfo($"PhoneInfo_{seed}");
            agr.AddPersonalDetails($"Name_{seed}", $"lastName_{seed}");
            List<EventData> eventData = new List<EventData>();
            //how we ensure version here 

            agr.Internals(events =>
            {
                var x = JsonSerializer.Serialize(events[0], Type.GetType(events[0].GetType().FullName));

                eventData.AddRange(
                    events
                        .Select(@event =>
                            new EventData(eventId: @event.Id,
                                type: @event.GetType().FullName,
                                isJson: true,
                                data: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event,@event.GetType())),
                                metadata: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { AggregateId = agr.Id, Version = "?" })))
                        ));
            });


            using var conn = GetEventStoreConnection;
            await conn.ConnectAsync();

            var streamName = $"{nameof(Aggregate)}-{agr.Id}";
            var resA = await conn.AppendToStreamAsync(streamName, ExpectedVersion.Any, eventData);

            _logger.LogInformation($"Event data stored for:{agr.Id}");
        }


        // POST api/<EventStore>
        [HttpPost]
        public async Task Post([FromBody] PostData data = null)
        {
            var agr = new Aggregate();
            var seed = Guid.NewGuid();
            agr.AddPhoneInfo($"PhoneInfo_{seed}");
            agr.AddPersonalDetails($"Name_{seed}", $"lastName_{seed}");
            List<EventData> eventData = new List<EventData>();
            //how we ensure version here 

            agr.Internals(events =>
            {
                var x = JsonSerializer.Serialize(events[0], Type.GetType(events[0].GetType().FullName));
                eventData.AddRange(
                    events
                        .Select(@event =>
                        new EventData(eventId: @event.Id,
                            type: @event.GetType().FullName,
                            isJson: true,
                            data: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event, Type.GetType(@event.GetType().FullName))),
                            metadata: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { AggregateId = agr.Id, Version = "?" })))
                    ));
            });


            using var conn = GetEventStoreConnection;
            await conn.ConnectAsync();

            var streamName = $"{nameof(Aggregate)}-{agr.Id}";
            var resA = await conn.AppendToStreamAsync(streamName, ExpectedVersion.Any, eventData);

            _logger.LogInformation($"Event data stored for:{agr.Id}");
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