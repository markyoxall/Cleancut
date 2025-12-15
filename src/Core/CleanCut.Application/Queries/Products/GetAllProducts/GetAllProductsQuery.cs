using CleanCut.Application.DTOs;
using CleanCut.Application.Common.Interfaces;
using System;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanCut.Application.Queries.Products.GetAllProducts;

public record GetAllProductsQuery() : IRequest<IReadOnlyList<ProductInfo>>, ICacheableQuery
{
    public string CacheKey => "products:all";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
}
