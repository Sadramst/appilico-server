using AutoMapper;
using Microsoft.Extensions.Logging;
using Appilico.Server.Business.DTOs.Common;
using Appilico.Server.Business.DTOs.Order;
using Appilico.Server.Business.Interfaces;
using Appilico.Server.Domain.Entities;
using Appilico.Server.Domain.Enums;
using Appilico.Server.Domain.Interfaces;

namespace Appilico.Server.Business.Services;

/// <summary>Order service implementation.</summary>
public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<OrderService> _logger;

    /// <summary>Initializes a new instance of OrderService.</summary>
    public OrderService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<OrderService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<List<OrderDto>>> GetAllAsync(int page = 1, int pageSize = 10)
    {
        var (items, totalCount) = await _unitOfWork.Orders.GetPagedAsync(page, pageSize,
            orderBy: q => q.OrderByDescending(o => o.OrderDate),
            includes: o => o.Customer);

        var dtos = _mapper.Map<List<OrderDto>>(items);
        var pagination = PaginationMeta.Create(page, pageSize, totalCount);

        return ApiResponse<List<OrderDto>>.SuccessResponse(dtos, pagination);
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<OrderDto>> GetByIdAsync(Guid id)
    {
        var order = await _unitOfWork.Orders.GetWithDetailsAsync(id);
        if (order == null)
            return ApiResponse<OrderDto>.FailResponse("Order not found");

        return ApiResponse<OrderDto>.SuccessResponse(_mapper.Map<OrderDto>(order));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<List<OrderDto>>> GetByCustomerAsync(Guid customerId, int page = 1, int pageSize = 10)
    {
        var (items, totalCount) = await _unitOfWork.Orders.GetPagedAsync(page, pageSize,
            predicate: o => o.CustomerId == customerId,
            orderBy: q => q.OrderByDescending(o => o.OrderDate));

        var dtos = _mapper.Map<List<OrderDto>>(items);
        var pagination = PaginationMeta.Create(page, pageSize, totalCount);

        return ApiResponse<List<OrderDto>>.SuccessResponse(dtos, pagination);
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<OrderDto>> CreateFromCartAsync(Guid customerId, CreateOrderRequest request, string userId)
    {
        var cart = await _unitOfWork.Carts.GetActiveCartAsync(customerId);
        if (cart == null || !cart.Items.Any())
            return ApiResponse<OrderDto>.FailResponse("Cart is empty");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var orderNumber = await _unitOfWork.Orders.GenerateOrderNumberAsync();

            var order = new Order
            {
                CustomerId = customerId,
                OrderNumber = orderNumber,
                ShippingAddressId = request.ShippingAddressId,
                BillingAddressId = request.BillingAddressId,
                PaymentMethod = request.PaymentMethod,
                VoucherCode = request.VoucherCode,
                Notes = request.Notes,
                OrderDate = DateTime.UtcNow,
                OrderStatus = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                CreatedBy = userId
            };

            decimal subTotal = 0;
            var orderItems = new List<OrderItem>();

            foreach (var cartItem in cart.Items)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(cartItem.ProductId);
                if (product == null) continue;

                if (product.StockQuantity < cartItem.Quantity)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ApiResponse<OrderDto>.FailResponse($"Insufficient stock for {product.Name}");
                }

                var orderItem = new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    VariantId = cartItem.VariantId,
                    ProductName = product.Name,
                    UnitPrice = cartItem.UnitPrice,
                    Quantity = cartItem.Quantity,
                    TotalPrice = cartItem.UnitPrice * cartItem.Quantity,
                    CreatedBy = userId
                };

                orderItems.Add(orderItem);
                subTotal += orderItem.TotalPrice;

                // Reduce stock
                product.StockQuantity -= cartItem.Quantity;
                _unitOfWork.Products.Update(product);
            }

            order.SubTotal = subTotal;
            order.TotalAmount = subTotal - order.DiscountAmount + order.TaxAmount + order.ShippingAmount;
            order.Items = orderItems;

            await _unitOfWork.Orders.AddAsync(order);

            // Clear cart
            cart.IsActive = false;
            _unitOfWork.Carts.Update(cart);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Order {OrderNumber} created for customer {CustomerId}", orderNumber, customerId);

            var created = await _unitOfWork.Orders.GetWithDetailsAsync(order.Id);
            return ApiResponse<OrderDto>.SuccessResponse(_mapper.Map<OrderDto>(created), "Order created successfully");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Failed to create order for customer {CustomerId}", customerId);
            return ApiResponse<OrderDto>.FailResponse("Failed to create order");
        }
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<OrderDto>> UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request, string userId)
    {
        var order = await _unitOfWork.Orders.GetWithDetailsAsync(id);
        if (order == null)
            return ApiResponse<OrderDto>.FailResponse("Order not found");

        var history = new OrderStatusHistory
        {
            OrderId = id,
            OldStatus = order.OrderStatus,
            NewStatus = request.NewStatus,
            Notes = request.Notes,
            ChangedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        order.OrderStatus = request.NewStatus;
        order.UpdatedBy = userId;

        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Order {OrderId} status changed to {Status} by {UserId}", id, request.NewStatus, userId);
        return ApiResponse<OrderDto>.SuccessResponse(_mapper.Map<OrderDto>(order), "Order status updated successfully");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<List<OrderStatusHistoryDto>>> GetStatusHistoryAsync(Guid orderId)
    {
        var history = await _unitOfWork.Orders.GetStatusHistoryAsync(orderId);
        return ApiResponse<List<OrderStatusHistoryDto>>.SuccessResponse(_mapper.Map<List<OrderStatusHistoryDto>>(history));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<bool>> CancelAsync(Guid id, string userId)
    {
        var order = await _unitOfWork.Orders.GetWithDetailsAsync(id);
        if (order == null)
            return ApiResponse<bool>.FailResponse("Order not found");

        if (order.OrderStatus != OrderStatus.Pending && order.OrderStatus != OrderStatus.Confirmed)
            return ApiResponse<bool>.FailResponse("Order cannot be cancelled at this stage");

        order.OrderStatus = OrderStatus.Cancelled;
        order.UpdatedBy = userId;

        // Restore stock
        foreach (var item in order.Items)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
            if (product != null)
            {
                product.StockQuantity += item.Quantity;
                _unitOfWork.Products.Update(product);
            }
        }

        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Order {OrderId} cancelled by {UserId}", id, userId);
        return ApiResponse<bool>.SuccessResponse(true, "Order cancelled successfully");
    }
}
