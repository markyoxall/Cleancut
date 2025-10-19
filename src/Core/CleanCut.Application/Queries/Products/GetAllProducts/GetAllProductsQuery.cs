using CleanCut.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanCut.Application.Queries.Products.GetAllProducts;

public record GetAllProductsQuery() : IRequest<IReadOnlyList<ProductDto>>;
