using System;
namespace EventSourcing.Console
{
	public interface IEvent {}
	public record ProductShipped(string Sku, int Quantity, DateTime DateTime) : IEvent;
	public record ProductReceived(string Sku, int Quantity, DateTime DateTime) : IEvent;
	public record InventoryAdjusted(string Sku, int Quantity, string Reason, DateTime DateTime) : IEvent;
}

