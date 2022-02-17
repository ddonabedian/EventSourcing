using System;
namespace EventSourcing.Console
{
	public class WarehouseProductRepository
	{
		private readonly Dictionary<string, IList<IEvent>> _inMemoryStreams = new();
		IList<Action<IEvent>> _subscribers = new List<Action<IEvent>>();

		public WarehouseProduct Get(string sku)
		{
			var warehouseProduct = new WarehouseProduct(sku);

			if (_inMemoryStreams.ContainsKey(sku))
			{
				foreach (var evt in _inMemoryStreams[sku])
				{
					warehouseProduct.AddEvent(evt);
				}
			}

			return warehouseProduct;
		}

        internal void Subscribe(Action<IEvent> action)
        {
			_subscribers.Add(action);
        }

		public void Save(WarehouseProduct warehouseProduct)
		{
			var productEvents = warehouseProduct.GetEvents();
			_inMemoryStreams[warehouseProduct.Sku] = productEvents;

			var existingEvents =_inMemoryStreams[warehouseProduct.Sku];
			var lastEvent = existingEvents.Last();
			var offset = productEvents.IndexOf(lastEvent);
			var newEvents = productEvents.Skip(offset).ToList();

			foreach (var evt in newEvents)
            {
                foreach (var subscriber in _subscribers)
                {
					subscriber.Invoke(evt);
                }
            }
		}
	}
}

