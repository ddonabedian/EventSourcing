namespace EventSourcing.Console
{
    public class WarehouseProduct
    {
        public string Sku { get; set; }
        private IList<IEvent> _events = new List<IEvent>();

        // Projection (Current State)
        private readonly CurrentState _currentState = new();

        public WarehouseProduct(string sku)
        {
            this.Sku = sku;
        }

        public void ShipProduct(int quantity)
        {
            if(quantity > _currentState.QuantityOnHand)
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
            if(_currentState.QuantityOnHand + quantity < 0)
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

            _events.Add(evnt);
        }

        private void Apply(ProductReceived evnt)
        {
            _currentState.QuantityOnHand += evnt.Quantity;
        }

        private void Apply(ProductShipped evnt)
        {
            _currentState.QuantityOnHand -= evnt.Quantity;
        }

        private void Apply(InventoryAdjusted evnt)
        {
            _currentState.QuantityOnHand = evnt.Quantity;
        }

        public IList<IEvent> GetEvents()
        {
            return this._events;
        }

        public int GetQuantityOnHand()
        {
            return this._currentState.QuantityOnHand;
        }
    }
}