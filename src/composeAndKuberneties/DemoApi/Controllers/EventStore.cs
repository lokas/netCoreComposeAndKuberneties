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
            return Ok(await GetEvents($"{nameof(PersonaDetail)}-{id}"));
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

        private async Task<AggregateState> GetState(string streamName)
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

            var events = streamEvents
                .Select(e => Newtonsoft.Json.JsonConvert.DeserializeObject(
                    UTF8Encoding.UTF8.GetString(e.Event.Data), Type.GetType(e.Event.EventType)))
                .Cast<Event>()
                .ToList();

            return GetState(events, new AggregateState());
        }

        private static AggregateState GetState(List<Event> eventsData, AggregateState state)
        {
            if (eventsData.Count == 0)
                return state;
            var @event = eventsData.First();

            void AddNameInfo(string name, string last)
            {
                state.Name = name;
                state.LastName = last;
            }

            switch (@event)
            {
                case PersonalDetailsChanged pdc:
                    AddNameInfo(pdc.Name, pdc.Last);
                    break;
                case PersonalDetailsCreated pd:
                    AddNameInfo(pd.Name, pd.Last);
                    break;
                case PhoneInfoCreated pi:
                    state.PhoneInfo = pi.PhoneInfo;
                    break;
                case PhoneInfoChanged pc:
                    state.PhoneInfo = pc.PhoneInfo; //now abstraction comes handy
                    break;
            }

            eventsData.RemoveAt(0);
            return GetState(eventsData, state);
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

        [HttpPost]
        public async Task Post()
        {
            var agr = new PersonaDetail();
            var seed = Guid.NewGuid();
            agr.AddPhoneInfo($"PhoneInfo_{seed}");
            agr.AddPersonalDetails($"Name_{seed}", $"lastName_{seed}");
            //how we ensure version here 
            await AppendEvents(agr);
        }

        private async Task AppendEvents(PersonaDetail agr)
        {
            List<EventData> eventData = new List<EventData>();

            agr.Internals(events =>
            {
                eventData.AddRange(
                    events
                        .Select(@event =>
                            new EventData(eventId: @event.Id,
                                type: @event.GetType().FullName,
                                isJson: true,
                                data: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event, @event.GetType())),
                                metadata: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new
                                { AggregateId = agr.Id, Version = "?" })))
                        ));
            });


            using var conn = GetEventStoreConnection;
            await conn.ConnectAsync();

            var streamName = $"{nameof(PersonaDetail)}-{agr.Id}";
            var resA = await conn.AppendToStreamAsync(streamName, ExpectedVersion.Any, eventData);

            _logger.LogInformation($"Event data stored for:{agr.Id}");
        }


        // POST api/<EventStore>
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] PostData data = null)
        {
            if (data == null)
                return BadRequest("No data id!");
            var stream = $"{nameof(PersonaDetail)}-{data.Id}";

            var agr = new PersonaDetail(data.Id, await GetState(stream));

            var seed = Guid.NewGuid();
            agr.AddPhoneInfo($"PhoneInfo_{seed}");
            agr.AddPersonalDetails($"Name_{seed}", $"lastName_{seed}");

            await AppendEvents(agr);

            return Ok(new
            {
                Info = "Updated",
                data.Id
            });
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