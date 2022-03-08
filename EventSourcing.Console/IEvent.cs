using System;
namespace EventSourcing.Console
{
    public interface IEvent
    {
        string EventType { get; }
    }
    public record ProductShipped(string Sku, int Quantity, DateTime DateTime) : IEvent
    {
        public string EventType => "ProductShipped";
    }

    public record ProductReceived(string Sku, int Quantity, DateTime DateTime) : IEvent
    {
        public string EventType => "ProductReceived";
    }

    public record InventoryAdjusted(string Sku, int Quantity, string Reason, DateTime DateTime) : IEvent
    {
        public string EventType => "InventoryAdjusted";
    }
}

