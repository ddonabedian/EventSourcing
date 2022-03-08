// See https://aka.ms/new-console-template for more information
using EventSourcing.Console;

Console.WriteLine("Hello");
var repo = new WarehouseProductRepository();
var evtStore = await WarehouseProductEventStoreStream.Factory();

var projection = new Projection(new ProductDbContext());
repo.Subscribe(projection.ReceiveEvent);

var key = String.Empty;

while (key.ToUpper() != "X")
{
    Console.WriteLine("R: Receive Inventory");
    Console.WriteLine("S: Ship Inventory");
    Console.WriteLine("A: Adjust Inventory");
    Console.WriteLine("Q: Quantity on Hand");
    Console.WriteLine("E: Events since Snapshot");
    Console.WriteLine("P: Projection");
    Console.Write("> ");
    key = Console.ReadKey().KeyChar.ToString().ToUpperInvariant();
    Console.WriteLine();

    var sku = GetSkuFromConsole();
    //var warehouseProduct = repo.Get(sku);
    var warehouseProduct = await evtStore.Get(sku);

    switch (key.ToUpper())
    {
        case "R":
        var receiveInput = GetQuantity();
            if (receiveInput.IsValid)
            {
                warehouseProduct.ReceiveProduct(receiveInput.Quantity);
                Console.WriteLine($"{sku} Received: {receiveInput.Quantity}");
                await evtStore.Save(warehouseProduct);
                //repo.Save(warehouseProduct);
            }
            break;
        case "S":
        var shipInput = GetQuantity();
            if (shipInput.IsValid)
            {
                warehouseProduct.ShipProduct(shipInput.Quantity);
                Console.WriteLine($"{sku} Shipped: {shipInput.Quantity}");
                await evtStore.Save(warehouseProduct);
                //repo.Save(warehouseProduct);
            }
            break;
        case "A":
        var adjustmentInput = GetQuantity();
            if (adjustmentInput.IsValid)
            {
                var reason = GetAdjustmentReason();
                warehouseProduct.AdjustInventory(adjustmentInput.Quantity, reason);
                Console.WriteLine($"{sku} Adjusted: {adjustmentInput.Quantity} {reason}");
                await evtStore.Save(warehouseProduct);
                //repo.Save(warehouseProduct);
            }
            break;
        case "Q":
            var currentQuantityOnHand = warehouseProduct.GetQuantityOnHand();
            Console.WriteLine($"{sku} Quantity on Hand: {currentQuantityOnHand}");
            break;
        case "E":
            Console.WriteLine($"Events: {sku}");
            foreach (var evnt in warehouseProduct.GetEvents())
            {
                switch (evnt)
                {
                    case ProductReceived receiveProduct:
                        Console.WriteLine($"{receiveProduct.DateTime:u} {sku} Received: {receiveProduct.Quantity}");
                        break;
                    case ProductShipped shipProduct:
                        Console.WriteLine($"{shipProduct.DateTime:u} {sku} Shipped: {shipProduct.Quantity}");
                        break;
                    case InventoryAdjusted inventoryAdjusted:
                        Console.WriteLine($"{inventoryAdjusted.DateTime:u} {sku} Ajusted: {inventoryAdjusted.Quantity}");
                        break;
                    default:
                        break;
                }
            }
            break;
        case "P":
            Console.WriteLine($"Projection: {sku}");
            var productProjection = projection.GetProduct(sku);
            Console.WriteLine($"{sku} Received: {productProjection.Received}");
            Console.WriteLine($"{sku} Shipped: {productProjection.Shipped}");
            break;
        default:
            break;
    }

    Console.WriteLine();
}

string GetAdjustmentReason()
{
    Console.Write($"Reason: ");
    return Console.ReadLine();
}

ReceiveInput GetQuantity()
{
    Console.Write($"Quantity: ");
    var input = Console.ReadLine();
    if (!int.TryParse(input, out int quantity))
    {
        throw new InvalidDomainException("Quantity must be an integer!");
    }

    return new ReceiveInput { Quantity = quantity };
}

string GetSkuFromConsole()
{
    Console.Write("Sku: ");
    return Console.ReadLine();
}