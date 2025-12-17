using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CleanCut.BlazorSPA.Pages.Models;

namespace CleanCut.BlazorSPA.Pages.Services;

public interface IProductApiService
{
    Task<IEnumerable<SimpleProduct>> GetAllProductsAsync();
    Task<SimpleProduct?> GetProductByIdAsync(Guid id);
    Task<SimpleProduct> CreateProductAsync(SimpleProduct product);
    Task<SimpleProduct> UpdateProductAsync(SimpleProduct product);
    Task<bool> DeleteProductAsync(Guid id);
}
