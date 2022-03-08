using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing.Console
{
    public class WarehouseProductEventStoreStream
    {
        private const int SnapshotInterval = 4;
        private readonly IEventStoreConnection _connection;

        public WarehouseProductEventStoreStream(IEventStoreConnection conn)
        {
            this._connection = conn;
        }

        public static async Task<WarehouseProductEventStoreStream> Factory()
        {
            var connectionSettings = ConnectionSettings.Create()
                .KeepReconnecting()
                .KeepRetrying()
                .WithConnectionTimeoutOf(TimeSpan.FromSeconds(10))
                .SetGossipTimeout(TimeSpan.FromSeconds(10))
                .SetTimeoutCheckPeriodTo(TimeSpan.FromSeconds(10))
                .SetHeartbeatTimeout(TimeSpan.FromSeconds(20))
                .SetHeartbeatInterval(TimeSpan.FromSeconds(10))
                .SetDefaultUserCredentials(new UserCredentials("admin", "changeit"))
                .DisableTls()
                .DisableServerCertificateValidation()
              
                .Build();

            var conn = EventStoreConnection.Create(connectionSettings, new Uri("tcp://localhost:1113"));
            //var conn = EventStoreConnection.Create(connectionSettings, new Uri("tcp://admin:changeit@localhost:1113"));
            await conn.ConnectAsync();

            return new WarehouseProductEventStoreStream(conn);
        }

        private string GetSnapshotStreamName(string sku)
        {
            return $"product-snapshot-{sku}";
        }

        private string GetStreamName(string sku)
        {
            return $"product-{sku}";
        }

        public async Task<WarehouseProduct> Get(string sku)
        {
            var streamName = GetStreamName(sku);

            var snapshot = await GetSnapshot(sku);
            var warehouseProduct = new WarehouseProduct(sku, snapshot.State);

            StreamEventsSlice currentSlice;
            var nextSliceStart = snapshot.Version + 1;

            do
            {
                currentSlice = await _connection.ReadStreamEventsForwardAsync(
                    streamName,
                    nextSliceStart,
                    200,
                    false
                );

                nextSliceStart = currentSlice.NextEventNumber;

                foreach (var evnt in currentSlice.Events)
                {
                    var eventObj = DeserializeEvent(evnt);
                    warehouseProduct.ApplyEvent(eventObj);
                }
            } while (!currentSlice.IsEndOfStream);

            return warehouseProduct;
        }

        public async Task Save(WarehouseProduct warehouseProduct)
        {
            var streamName = GetStreamName(warehouseProduct.Sku);

            var newEvents = warehouseProduct.GetUncommittedEvents();

            long version = 0;
            foreach (var evnt in newEvents)
            {
                var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(evnt));
                var metadata = Encoding.UTF8.GetBytes("{}");
                var evt = new EventData(Guid.NewGuid(), evnt.EventType, true, data, metadata);
                var result = await _connection.AppendToStreamAsync(streamName, ExpectedVersion.Any, evt);
                version = result.NextExpectedVersion;
            }

            if((version - 1) % SnapshotInterval == 0)
            {
                await AppendSnapshot(warehouseProduct, version);
            }
        }

        private async Task AppendSnapshot(WarehouseProduct warehouseProduct, long version)
        {
            var streamName = GetSnapshotStreamName(warehouseProduct.Sku);
            var state = warehouseProduct.GetState();

            var snapshot = new Snapshot
            {
                State = state,
                Version = version
            };

            var metadata = Encoding.UTF8.GetBytes("{}");
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(snapshot));
            var evt = new EventData(Guid.NewGuid(), "snapshot", true, data, metadata);
            await _connection.AppendToStreamAsync(streamName,ExpectedVersion.Any, evt);
        }

        private IEvent DeserializeEvent(ResolvedEvent evnt)
        {
            var evtType = evnt.Event.EventType;
            var json = Encoding.UTF8.GetString(evnt.Event.Data.ToArray());
            switch (evtType)
            {
                case "ProductReceived":
                    return JsonConvert.DeserializeObject<ProductReceived>(json);
                case "ProductShipped":
                    return JsonConvert.DeserializeObject<ProductShipped>(json);
                case "InventoryAdjusted":
                    return JsonConvert.DeserializeObject<InventoryAdjusted>(json);
                default:
                    throw new InvalidDomainException("Event type unknown");
            }
        }

        private async Task<Snapshot> GetSnapshot(string sku)
        {
            var streamName = GetSnapshotStreamName(sku);
            var slice = await _connection.ReadStreamEventsBackwardAsync(streamName, StreamPosition.End, 1, false);
            if (slice.Events.Any())
            {
                var evnt = slice.Events.First();
                var json = Encoding.UTF8.GetString(evnt.Event.Data);
                return JsonConvert.DeserializeObject<Snapshot>(json);
            }

            return new Snapshot();
        }

        
    }
}
