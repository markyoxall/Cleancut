using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CleanCut.Application.DTOs;
using CleanCut.WinApp.Views.Orders;
using CleanCut.WinApp.MVP;
using MediatR;
using Microsoft.Extensions.Logging;
using CleanCut.Application.Queries.Orders.GetAllOrders;
using CleanCut.Infrastructure.Caching.Constants;

namespace CleanCut.WinApp.Presenters;

public class OrderListPresenter : BasePresenter<IOrderListView>
{
    private readonly Services.Factories.IViewFactory<IOrderLineItemListView> _lineItemListViewFactory;
    private readonly Services.Factories.IViewFactory<IOrderLineItemEditView> _lineItemEditViewFactory;
    private readonly IMediator _mediator;
    private readonly ILogger<OrderListPresenter> _logger;
    private readonly CleanCut.Application.Common.Interfaces.ICacheService? _cacheService;

    private List<OrderInfo> _orders = new();

    // Named handler for safe subscribe/unsubscribe
    private void OnViewOrderLineItemsRequestedHandler(object? sender, Guid id) => ShowOrderLineItems(id);

    public OrderListPresenter(
        IOrderListView view,
        Services.Factories.IViewFactory<IOrderLineItemListView> lineItemListViewFactory,
        Services.Factories.IViewFactory<IOrderLineItemEditView> lineItemEditViewFactory,
        IMediator mediator,
        ILogger<OrderListPresenter> logger,
        CleanCut.Application.Common.Interfaces.ICacheService? cacheService = null)
        : base(view)
    {
        _lineItemListViewFactory = lineItemListViewFactory;
        _lineItemEditViewFactory = lineItemEditViewFactory;
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cacheService = cacheService;
    }

    public override void Initialize()
    {
        base.Initialize();
        // Subscribe to view events
        View.ViewOrderLineItemsRequested += OnViewOrderLineItemsRequestedHandler;

        // Load initial orders
        _ = LoadInitialDataAsync();
    }

    public override void Cleanup()
    {
        // Unsubscribe
        View.ViewOrderLineItemsRequested -= OnViewOrderLineItemsRequestedHandler;
        base.Cleanup();
    }

    private async Task LoadInitialDataAsync()
    {
        await ExecuteAsync(async () =>
        {
            _logger.LogInformation("Loading orders for OrderList view");
            var query = new GetAllOrdersQuery();

            IReadOnlyList<OrderInfo>? orders = null;
            if (_cacheService != null)
            {
                var cacheKey = CacheKeys.AllOrders();
                var cached = await _cacheService.GetAsync<List<OrderInfo>>(cacheKey);
                if (cached != null)
                {
                    orders = cached;
                    _logger.LogInformation("Loaded orders from cache");
                }
            }

            if (orders == null)
            {
                orders = await _mediator.Send(query);
                if (_cacheService != null && orders != null)
                {
                    await _cacheService.SetAsync(CacheKeys.AllOrders(), orders.ToList(), CacheTimeouts.Orders);
                    _logger.LogInformation("Loaded orders from database and cached");
                }
            }

            var ordersList = orders ?? Array.Empty<OrderInfo>();

            // Project to OrderListGridItem for grid columns (only valid properties)
            var displayOrders = ordersList.Select(o => new OrderListGridItem
            {
                Id = o.Id,
                CustomerName = o.CustomerName,
                OrderDate = o.OrderDate,
                Status = o.Status.ToString(),
                TotalAmount = o.TotalAmount
            }).ToList();
            View.DisplayOrders(displayOrders);
            _orders = ordersList.ToList();

            _logger.LogInformation("Loaded {OrderCount} orders", _orders.Count);
        });
    }

    /// <summary>
    /// Loads the provided orders and projects them to a presentation model for the grid.
    /// </summary>
    public void Load(IEnumerable<OrderInfo> orders)
    {
        var displayOrders = orders.Select(o => new OrderListGridItem
        {
            Id = o.Id,
            CustomerName = o.CustomerName,
            OrderDate = o.OrderDate,
            Status = o.Status.ToString(),
            TotalAmount = o.TotalAmount
        }).ToList();
        View.DisplayOrders(displayOrders);
        _orders = orders.ToList();
    }

    private void ShowOrderLineItems(Guid orderId)
    {
        var order = _orders.Find(x => x.Id == orderId);
        if (order == null) return;
        var lineItemListView = _lineItemListViewFactory.Create();
        var presenter = new OrderLineItemListPresenter(lineItemListView, _lineItemEditViewFactory);
        presenter.Load(order.OrderLineItems);

        if (lineItemListView is Form form)
        {
            form.ShowDialog();
        }
        // Optionally update order.OrderLineItems if changed
    }
}
