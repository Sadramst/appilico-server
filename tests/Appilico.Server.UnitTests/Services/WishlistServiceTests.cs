using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Appilico.Server.Business.Services;
using Appilico.Server.Domain.Entities;
using Appilico.Server.Domain.Interfaces;
using Appilico.Server.UnitTests.Helpers;
using System.Linq.Expressions;

namespace Appilico.Server.UnitTests.Services;

public class WishlistServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IWishlistRepository> _wishlistRepoMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<WishlistService>> _loggerMock;
    private readonly WishlistService _sut;

    public WishlistServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _wishlistRepoMock = new Mock<IWishlistRepository>();
        _productRepoMock = new Mock<IProductRepository>();
        _mapper = TestMapperConfig.CreateMapper();
        _loggerMock = new Mock<ILogger<WishlistService>>();

        _unitOfWorkMock.Setup(u => u.Wishlists).Returns(_wishlistRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepoMock.Object);

        _sut = new WishlistService(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByCustomerAsync_ReturnsWishlistItems()
    {
        var customerId = Guid.NewGuid();
        var items = new List<Wishlist>
        {
            new() { Id = Guid.NewGuid(), CustomerId = customerId, ProductId = Guid.NewGuid(), CreatedBy = "test" },
            new() { Id = Guid.NewGuid(), CustomerId = customerId, ProductId = Guid.NewGuid(), CreatedBy = "test" }
        };
        _wishlistRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Wishlist, bool>>>())).ReturnsAsync(items);

        var result = await _sut.GetByCustomerAsync(customerId);

        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByCustomerAsync_EmptyWishlist_ReturnsEmptyList()
    {
        _wishlistRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Wishlist, bool>>>())).ReturnsAsync(new List<Wishlist>());

        var result = await _sut.GetByCustomerAsync(Guid.NewGuid());

        result.Success.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task AddAsync_NewProduct_ReturnsSuccess()
    {
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId, Name = "Test Product", IsActive = true, CreatedBy = "test" };

        _productRepoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);
        _wishlistRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Wishlist, bool>>>())).ReturnsAsync(false);
        // Mock GetQueryable for soft-deleted check - return async-compatible empty queryable
        _wishlistRepoMock.Setup(r => r.GetQueryable()).Returns(new TestAsyncEnumerable<Wishlist>(new List<Wishlist>()));
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.AddAsync(customerId, productId);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task AddAsync_AlreadyInWishlist_ReturnsFail()
    {
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId, Name = "Test Product", IsActive = true, CreatedBy = "test" };

        _productRepoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);
        _wishlistRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Wishlist, bool>>>())).ReturnsAsync(true);

        var result = await _sut.AddAsync(customerId, productId);

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_NonExistingProduct_ReturnsFail()
    {
        _productRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Product?)null);

        var result = await _sut.AddAsync(Guid.NewGuid(), Guid.NewGuid());

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveAsync_ExistingItem_ReturnsSuccess()
    {
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var item = new Wishlist { Id = Guid.NewGuid(), CustomerId = customerId, ProductId = productId, CreatedBy = "test" };

        _wishlistRepoMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Wishlist, bool>>>())).ReturnsAsync(item);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.RemoveAsync(customerId, productId);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveAsync_NotInWishlist_ReturnsFail()
    {
        _wishlistRepoMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Wishlist, bool>>>())).ReturnsAsync((Wishlist?)null);

        var result = await _sut.RemoveAsync(Guid.NewGuid(), Guid.NewGuid());

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task IsInWishlistAsync_ProductInWishlist_ReturnsTrue()
    {
        _wishlistRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Wishlist, bool>>>())).ReturnsAsync(true);

        var result = await _sut.IsInWishlistAsync(Guid.NewGuid(), Guid.NewGuid());

        result.Success.Should().BeTrue();
        result.Data.Should().BeTrue();
    }

    [Fact]
    public async Task IsInWishlistAsync_ProductNotInWishlist_ReturnsFalse()
    {
        _wishlistRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Wishlist, bool>>>())).ReturnsAsync(false);

        var result = await _sut.IsInWishlistAsync(Guid.NewGuid(), Guid.NewGuid());

        result.Success.Should().BeTrue();
        result.Data.Should().BeFalse();
    }
}
