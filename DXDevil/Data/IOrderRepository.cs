using System.Collections.Generic;
using System.Threading.Tasks;

namespace DXDevil.Data
{
    /// <summary>
    /// Lightweight repository interface used by the DX UI layer.
    /// </summary>
    public interface IOrderRepository
    {
        Task<List<OrderListGridItem>> GetOrdersAsync();
    }
}
