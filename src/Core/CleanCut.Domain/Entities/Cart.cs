using CleanCut.Domain.Common;

namespace CleanCut.Domain.Entities;

public class Cart : BaseEntity
{
    private readonly List<CartItem> _items = new();

    public Guid OwnerId { get; private set; } // CustomerId or anonymous session id represented as Guid

    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    private Cart() { }

    public Cart(Guid ownerId)
    {
        if (ownerId == Guid.Empty) throw new ArgumentException("OwnerId cannot be empty", nameof(ownerId));
        OwnerId = ownerId;
    }

    public void AddItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        if (productId == Guid.Empty) throw new ArgumentException("ProductId cannot be empty", nameof(productId));
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive", nameof(quantity));

        var existing = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existing != null)
        {
            existing.UpdateQuantity(existing.Quantity + quantity);
        }
        else
        {
            _items.Add(new CartItem(Id, productId, productName, unitPrice, quantity));
        }
        SetUpdatedAt();
    }

    public void RemoveItem(Guid productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            _items.Remove(item);
            SetUpdatedAt();
        }
    }

    public void UpdateQuantity(Guid productId, int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive", nameof(quantity));
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null) throw new InvalidOperationException("Product not found in cart");
        item.UpdateQuantity(quantity);
        SetUpdatedAt();
    }

    public void Clear()
    {
        _items.Clear();
        SetUpdatedAt();
    }

    public decimal GetTotal() => _items.Sum(i => i.UnitPrice * i.Quantity);
}
