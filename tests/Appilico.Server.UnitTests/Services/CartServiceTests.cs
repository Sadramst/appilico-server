using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
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
}
