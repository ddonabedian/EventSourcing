namespace EventSourcing.Console
{
    public class WarehouseProduct
    {
        public string Sku { get; set; }
        private IList<IEvent> _allEvents = new List<IEvent>();
        private IList<IEvent> _uncommittedEvents = new List<IEvent>();

        // Projection (Current State)
        private readonly WarehouseProductState _warehouseProductState;

        public WarehouseProduct(string sku, WarehouseProductState state)
        {
            this.Sku = sku;
            this._warehouseProductState = GetState();
        }

        public WarehouseProductState GetState()
        {
            return new WarehouseProductState();
        }

        public void ShipProduct(int quantity)
        {
            if(quantity > _warehouseProductState.QuantityOnHand)
            {
                throw new InvalidDomainException("Not enough product to ship!");
            }

            AddEvent(new ProductShipped(this.Sku, quantity, DateTime.UtcNow));
        }

        public void ReceiveProduct(int quantity)
        {
            AddEvent(new ProductReceived(this.Sku, quantity, DateTime.UtcNow));
        }

        public void AdjustInventory(int quantity, string reason)
        {
            if(_warehouseProductState.QuantityOnHand + quantity < 0)
            {
                throw new InvalidDomainException("Cannot adjust to a negative quantity");
            }
            AddEvent(new InventoryAdjusted(this.Sku, quantity, reason, DateTime.UtcNow));
        }

        internal void AddEvent(IEvent evnt)
        {
            switch (evnt)
            {
                case ProductShipped shipProduct:
                    Apply(shipProduct);
                    break;
                case ProductReceived receiveProduct:
                    Apply(receiveProduct);
                    break;
                case InventoryAdjusted adjustInventory:
                    Apply(adjustInventory);
                    break;
                default:
                    throw new InvalidOperationException("Event not supported");
            }

            _uncommittedEvents.Add(evnt);
        }

        private void Apply(ProductReceived evnt)
        {
            _warehouseProductState.QuantityOnHand += evnt.Quantity;
        }

        internal void ApplyEvent(IEvent eventObj)
        {
            switch (eventObj.GetType().Name)
            {
                case "ProductReceived":
                    Apply((ProductReceived)eventObj);
                    break;
                case "ProductShipped":
                    Apply((ProductShipped)eventObj);
                    break;
                case "InventoryAdjusted":
                    Apply((InventoryAdjusted)eventObj);
                    break;
                default:
                    throw new InvalidOperationException("Unknown event type");
            }

            this._allEvents.Add(eventObj);
        }

        private void Apply(ProductShipped evnt)
        {
            _warehouseProductState.QuantityOnHand -= evnt.Quantity;
        }

        private void Apply(InventoryAdjusted evnt)
        {
            _warehouseProductState.QuantityOnHand = evnt.Quantity;
        }

        internal IList<IEvent> GetUncommittedEvents()
        {
            return _uncommittedEvents;
        }

        public IList<IEvent> GetEvents()
        {
            return this._allEvents;
        }

        public int GetQuantityOnHand()
        {
            return this._warehouseProductState.QuantityOnHand;
        }

        
    }
}