using System;
namespace EventSourcing.Console
{
	public class WarehouseProductRepository
	{
		private readonly Dictionary<string, IList<IEvent>> _inMemoryStreams = new();
		
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

		public void Save(WarehouseProduct warehouseProduct)
		{
			_inMemoryStreams[warehouseProduct.Sku] = warehouseProduct.GetEvents();
		}
	}
}

