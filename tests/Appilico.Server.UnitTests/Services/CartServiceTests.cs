using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Appilico.Server.Business.DTOs.Cart;
using Appilico.Server.Business.Services;
using Appilico.Server.Domain.Entities;
using Appilico.Server.Domain.Interfaces;
using Appilico.Server.UnitTests.Helpers;

namespace Appilico.Server.UnitTests.Services;

public class CartServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICartRepository> _cartRepoMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<CartService>> _loggerMock;
    private readonly CartService _sut;

    public CartServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _cartRepoMock = new Mock<ICartRepository>();
        _productRepoMock = new Mock<IProductRepository>();
        _mapper = TestMapperConfig.CreateMapper();
        _loggerMock = new Mock<ILogger<CartService>>();

        _unitOfWorkMock.Setup(u => u.Carts).Returns(_cartRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepoMock.Object);

        _sut = new CartService(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);
    }

    // ──────── GetCartAsync ────────

    [Fact]
    public async Task GetCartAsync_ExistingCart_ReturnsSuccess()
    {
        var customerId = Guid.NewGuid();
        var cart = new Cart
        {
            Id = Guid.NewGuid(), CustomerId = customerId,
            Items = new List<CartItem>(), CreatedBy = "test"
        };
        _cartRepoMock.Setup(r => r.GetActiveCartAsync(customerId)).ReturnsAsync(cart);
        _cartRepoMock.Setup(r => r.GetWithItemsAsync(cart.Id)).ReturnsAsync(cart);

        var result = await _sut.GetCartAsync(customerId);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCartAsync_NoCart_CreatesNewCart()
    {
        var customerId = Guid.NewGuid();
        _cartRepoMock.Setup(r => r.GetActiveCartAsync(customerId)).ReturnsAsync((Cart?)null);
        _cartRepoMock.Setup(r => r.AddAsync(It.IsAny<Cart>()))
            .ReturnsAsync((Cart c) => { c.Id = Guid.NewGuid(); return c; });
        _cartRepoMock.Setup(r => r.GetWithItemsAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => new Cart { Id = id, CustomerId = customerId, Items = new List<CartItem>(), CreatedBy = "test" });
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.GetCartAsync(customerId);

        result.Success.Should().BeTrue();
        _cartRepoMock.Verify(r => r.AddAsync(It.IsAny<Cart>()), Times.Once);
    }

    // ──────── AddItemAsync ────────

    [Fact]
    public async Task AddItemAsync_ValidProduct_ReturnsSuccess()
    {
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var cartId = Guid.NewGuid();
        var product = new Product { Id = productId, Name = "Test", BasePrice = 10m, StockQuantity = 50, CreatedBy = "test" };
        var cart = new Cart { Id = cartId, CustomerId = customerId, Items = new List<CartItem>(), CreatedBy = "test" };

        _productRepoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);
        _cartRepoMock.Setup(r => r.GetActiveCartAsync(customerId)).ReturnsAsync(cart);
        _cartRepoMock.Setup(r => r.GetWithItemsAsync(cartId)).ReturnsAsync(cart);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var request = new AddToCartRequest { ProductId = productId, Quantity = 2 };
        var result = await _sut.AddItemAsync(customerId, request);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task AddItemAsync_ProductNotFound_ReturnsFail()
    {
        _productRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Product?)null);

        var request = new AddToCartRequest { ProductId = Guid.NewGuid(), Quantity = 1 };
        var result = await _sut.AddItemAsync(Guid.NewGuid(), request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Product not found");
    }

    [Fact]
    public async Task AddItemAsync_InsufficientStock_ReturnsFail()
    {
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId, Name = "Test", BasePrice = 10m, StockQuantity = 2, CreatedBy = "test" };
        _productRepoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);

        var request = new AddToCartRequest { ProductId = productId, Quantity = 10 };
        var result = await _sut.AddItemAsync(Guid.NewGuid(), request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Insufficient stock");
    }

    [Fact]
    public async Task AddItemAsync_ExistingItem_UpdatesQuantity()
    {
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var cartId = Guid.NewGuid();
        var product = new Product { Id = productId, Name = "Test", BasePrice = 10m, StockQuantity = 50, CreatedBy = "test" };
        var existingItem = new CartItem { Id = Guid.NewGuid(), ProductId = productId, Quantity = 3, UnitPrice = 10m, CreatedBy = "test" };
        var cart = new Cart { Id = cartId, CustomerId = customerId, Items = new List<CartItem> { existingItem }, CreatedBy = "test" };

        _productRepoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);
        _cartRepoMock.Setup(r => r.GetActiveCartAsync(customerId)).ReturnsAsync(cart);
        _cartRepoMock.Setup(r => r.GetWithItemsAsync(cartId)).ReturnsAsync(cart);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var request = new AddToCartRequest { ProductId = productId, Quantity = 2 };
        var result = await _sut.AddItemAsync(customerId, request);

        result.Success.Should().BeTrue();
        existingItem.Quantity.Should().Be(5);
    }

    [Fact]
    public async Task AddItemAsync_NoExistingCart_CreatesNewCart()
    {
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId, Name = "Test", BasePrice = 10m, StockQuantity = 50, CreatedBy = "test" };

        _productRepoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);
        _cartRepoMock.Setup(r => r.GetActiveCartAsync(customerId)).ReturnsAsync((Cart?)null);
        _cartRepoMock.Setup(r => r.AddAsync(It.IsAny<Cart>()))
            .ReturnsAsync((Cart c) => { c.Id = Guid.NewGuid(); return c; });
        _cartRepoMock.Setup(r => r.GetWithItemsAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => new Cart { Id = id, CustomerId = customerId, Items = new List<CartItem>(), CreatedBy = "test" });
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var request = new AddToCartRequest { ProductId = productId, Quantity = 1 };
        var result = await _sut.AddItemAsync(customerId, request);

        result.Success.Should().BeTrue();
    }

    // ──────── UpdateItemAsync ────────

    [Fact]
    public async Task UpdateItemAsync_ExistingItem_ReturnsSuccess()
    {
        var customerId = Guid.NewGuid();
        var cartId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var item = new CartItem { Id = itemId, ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 10m, CreatedBy = "test" };
        var cart = new Cart { Id = cartId, CustomerId = customerId, Items = new List<CartItem> { item }, CreatedBy = "test" };

        _cartRepoMock.Setup(r => r.GetActiveCartAsync(customerId)).ReturnsAsync(cart);
        _cartRepoMock.Setup(r => r.GetWithItemsAsync(cartId)).ReturnsAsync(cart);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var request = new UpdateCartItemRequest { Quantity = 5 };
        var result = await _sut.UpdateItemAsync(customerId, itemId, request);

        result.Success.Should().BeTrue();
        item.Quantity.Should().Be(5);
    }

    [Fact]
    public async Task UpdateItemAsync_NoCart_ReturnsFail()
    {
        _cartRepoMock.Setup(r => r.GetActiveCartAsync(It.IsAny<Guid>())).ReturnsAsync((Cart?)null);

        var request = new UpdateCartItemRequest { Quantity = 5 };
        var result = await _sut.UpdateItemAsync(Guid.NewGuid(), Guid.NewGuid(), request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Cart not found");
    }

    [Fact]
    public async Task UpdateItemAsync_ItemNotFound_ReturnsFail()
    {
        var customerId = Guid.NewGuid();
        var cartId = Guid.NewGuid();
        var cart = new Cart { Id = cartId, CustomerId = customerId, Items = new List<CartItem>(), CreatedBy = "test" };

        _cartRepoMock.Setup(r => r.GetActiveCartAsync(customerId)).ReturnsAsync(cart);
        _cartRepoMock.Setup(r => r.GetWithItemsAsync(cartId)).ReturnsAsync(cart);

        var request = new UpdateCartItemRequest { Quantity = 5 };
        var result = await _sut.UpdateItemAsync(customerId, Guid.NewGuid(), request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Cart item not found");
    }

    // ──────── RemoveItemAsync ────────

    [Fact]
    public async Task RemoveItemAsync_ExistingItem_ReturnsSuccess()
    {
        var customerId = Guid.NewGuid();
        var cartId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var item = new CartItem { Id = itemId, ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 10m, CreatedBy = "test" };
        var cart = new Cart { Id = cartId, CustomerId = customerId, Items = new List<CartItem> { item }, CreatedBy = "test" };

        _cartRepoMock.Setup(r => r.GetActiveCartAsync(customerId)).ReturnsAsync(cart);
        _cartRepoMock.Setup(r => r.GetWithItemsAsync(cartId)).ReturnsAsync(cart);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.RemoveItemAsync(customerId, itemId);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveItemAsync_NoCart_ReturnsFail()
    {
        _cartRepoMock.Setup(r => r.GetActiveCartAsync(It.IsAny<Guid>())).ReturnsAsync((Cart?)null);

        var result = await _sut.RemoveItemAsync(Guid.NewGuid(), Guid.NewGuid());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Cart not found");
    }

    [Fact]
    public async Task RemoveItemAsync_ItemNotFound_ReturnsFail()
    {
        var customerId = Guid.NewGuid();
        var cartId = Guid.NewGuid();
        var cart = new Cart { Id = cartId, CustomerId = customerId, Items = new List<CartItem>(), CreatedBy = "test" };

        _cartRepoMock.Setup(r => r.GetActiveCartAsync(customerId)).ReturnsAsync(cart);
        _cartRepoMock.Setup(r => r.GetWithItemsAsync(cartId)).ReturnsAsync(cart);

        var result = await _sut.RemoveItemAsync(customerId, Guid.NewGuid());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Cart item not found");
    }

    // ──────── ClearCartAsync ────────

    [Fact]
    public async Task ClearCartAsync_ExistingCart_ReturnsSuccess()
    {
        var customerId = Guid.NewGuid();
        var cart = new Cart
        {
            Id = Guid.NewGuid(), CustomerId = customerId,
            Items = new List<CartItem>
            {
                new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 10, CreatedBy = "test" }
            },
            CreatedBy = "test"
        };
        _cartRepoMock.Setup(r => r.GetActiveCartAsync(customerId)).ReturnsAsync(cart);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.ClearCartAsync(customerId);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ClearCartAsync_NoCart_StillReturnsSuccess()
    {
        _cartRepoMock.Setup(r => r.GetActiveCartAsync(It.IsAny<Guid>())).ReturnsAsync((Cart?)null);

        var result = await _sut.ClearCartAsync(Guid.NewGuid());

        result.Success.Should().BeTrue();
    }
}
