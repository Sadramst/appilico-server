using AutoMapper;
using Microsoft.Extensions.Logging;
using Appilico.Server.Business.DTOs.Cart;
using Appilico.Server.Business.DTOs.Common;
using Appilico.Server.Business.Interfaces;
using Appilico.Server.Domain.Entities;
using Appilico.Server.Domain.Interfaces;

namespace Appilico.Server.Business.Services;

/// <summary>Cart service implementation.</summary>
public class CartService : ICartService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CartService> _logger;

    /// <summary>Initializes a new instance of CartService.</summary>
    public CartService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CartService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<CartDto>> GetCartAsync(Guid customerId)
    {
        var cart = await _unitOfWork.Carts.GetActiveCartAsync(customerId);
        if (cart == null)
        {
            cart = new Cart { CustomerId = customerId, IsActive = true };
            await _unitOfWork.Carts.AddAsync(cart);
            await _unitOfWork.SaveChangesAsync();
        }

        cart = await _unitOfWork.Carts.GetWithItemsAsync(cart.Id);
        return ApiResponse<CartDto>.SuccessResponse(_mapper.Map<CartDto>(cart));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<CartDto>> AddItemAsync(Guid customerId, AddToCartRequest request)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId);
        if (product == null)
            return ApiResponse<CartDto>.FailResponse("Product not found");

        if (product.StockQuantity < request.Quantity)
            return ApiResponse<CartDto>.FailResponse("Insufficient stock");

        var cart = await _unitOfWork.Carts.GetActiveCartAsync(customerId);
        if (cart == null)
        {
            cart = new Cart { CustomerId = customerId, IsActive = true };
            await _unitOfWork.Carts.AddAsync(cart);
            await _unitOfWork.SaveChangesAsync();
        }

        cart = await _unitOfWork.Carts.GetWithItemsAsync(cart.Id);

        var existingItem = cart!.Items.FirstOrDefault(i =>
            i.ProductId == request.ProductId && i.VariantId == request.VariantId);

        if (existingItem != null)
        {
            existingItem.Quantity += request.Quantity;
        }
        else
        {
            var cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = request.ProductId,
                VariantId = request.VariantId,
                Quantity = request.Quantity,
                UnitPrice = product.BasePrice
            };
            cart.Items.Add(cartItem);
        }

        _unitOfWork.Carts.Update(cart);
        await _unitOfWork.SaveChangesAsync();

        cart = await _unitOfWork.Carts.GetWithItemsAsync(cart.Id);
        return ApiResponse<CartDto>.SuccessResponse(_mapper.Map<CartDto>(cart), "Item added to cart");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<CartDto>> UpdateItemAsync(Guid customerId, Guid cartItemId, UpdateCartItemRequest request)
    {
        var cart = await _unitOfWork.Carts.GetActiveCartAsync(customerId);
        if (cart == null)
            return ApiResponse<CartDto>.FailResponse("Cart not found");

        cart = await _unitOfWork.Carts.GetWithItemsAsync(cart.Id);
        var item = cart!.Items.FirstOrDefault(i => i.Id == cartItemId);
        if (item == null)
            return ApiResponse<CartDto>.FailResponse("Cart item not found");

        item.Quantity = request.Quantity;
        _unitOfWork.Carts.Update(cart);
        await _unitOfWork.SaveChangesAsync();

        cart = await _unitOfWork.Carts.GetWithItemsAsync(cart.Id);
        return ApiResponse<CartDto>.SuccessResponse(_mapper.Map<CartDto>(cart), "Cart updated");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<CartDto>> RemoveItemAsync(Guid customerId, Guid cartItemId)
    {
        var cart = await _unitOfWork.Carts.GetActiveCartAsync(customerId);
        if (cart == null)
            return ApiResponse<CartDto>.FailResponse("Cart not found");

        cart = await _unitOfWork.Carts.GetWithItemsAsync(cart.Id);
        var item = cart!.Items.FirstOrDefault(i => i.Id == cartItemId);
        if (item == null)
            return ApiResponse<CartDto>.FailResponse("Cart item not found");

        cart.Items.Remove(item);
        _unitOfWork.Carts.Update(cart);
        await _unitOfWork.SaveChangesAsync();

        cart = await _unitOfWork.Carts.GetWithItemsAsync(cart.Id);
        return ApiResponse<CartDto>.SuccessResponse(_mapper.Map<CartDto>(cart), "Item removed from cart");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<bool>> ClearCartAsync(Guid customerId)
    {
        var cart = await _unitOfWork.Carts.GetActiveCartAsync(customerId);
        if (cart == null)
            return ApiResponse<bool>.SuccessResponse(true, "Cart is already empty");

        cart.IsActive = false;
        _unitOfWork.Carts.Update(cart);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "Cart cleared");
    }
}
