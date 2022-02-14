public class ReceiveInput
{
    public int Quantity { get; set; }
    public bool IsValid { get { return Quantity > 0; } }
}