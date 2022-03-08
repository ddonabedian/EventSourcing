namespace EventSourcing.Console
{
    public class Snapshot
    {
        public long Version { get; set; } = 0;
        public WarehouseProductState State { get; set; } = new();
    }
} 