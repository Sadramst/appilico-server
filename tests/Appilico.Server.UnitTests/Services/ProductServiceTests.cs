using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Appilico.Server.Business.DTOs.Product;
using Appilico.Server.Business.Services;
using Appilico.Server.Domain.Entities;
using Appilico.Server.Domain.Interfaces;
using Appilico.Server.UnitTests.Helpers;

namespace Appilico.Server.UnitTests.Services;

public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<ProductService>> _loggerMock;
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productRepoMock = new Mock<IProductRepository>();
        _mapper = TestMapperConfig.CreateMapper();
        _loggerMock = new Mock<ILogger<ProductService>>();

        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepoMock.Object);

        _sut = new ProductService(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingProduct_ReturnsSuccess()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId, Name = "Test Product", SKU = "TST-001",
            BasePrice = 29.99m, StockQuantity = 10, IsActive = true,
            CreatedBy = "test"
        };
        _productRepoMock.Setup(r => r.GetWithDetailsAsync(productId)).ReturnsAsync(product);

        // Act
        var result = await _sut.GetByIdAsync(productId);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Test Product");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentProduct_ReturnsFailure()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _productRepoMock.Setup(r => r.GetWithDetailsAsync(productId)).ReturnsAsync((Product?)null);

        // Act
        var result = await _sut.GetByIdAsync(productId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "New Product",
            SKU = "NP-001",
            CategoryId = Guid.NewGuid(),
            BasePrice = 49.99m,
            CostPrice = 20m,
            StockQuantity = 100,
            MinStockLevel = 10
        };

        _productRepoMock.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .ReturnsAsync(false);
        _productRepoMock.Setup(r => r.AddAsync(It.IsAny<Product>()))
            .ReturnsAsync((Product p) => p);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _productRepoMock.Setup(r => r.GetWithDetailsAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => new Product
            {
                Id = id, Name = "New Product", SKU = "NP-001",
                BasePrice = 49.99m, CostPrice = 20m, StockQuantity = 100,
                MinStockLevel = 10, CreatedBy = "user1"
            });

        // Act
        var result = await _sut.CreateAsync(request, "user1");

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("New Product");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ExistingProduct_ReturnsSuccess()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId, Name = "Test", SKU = "T1", CreatedBy = "test" };
        _productRepoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _sut.DeleteAsync(productId, "user1");

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_NonExistentProduct_ReturnsFailure()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _productRepoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync((Product?)null);

        // Act
        var result = await _sut.DeleteAsync(productId, "user1");

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task GetBySkuAsync_ExistingSku_ReturnsSuccess()
    {
        // Arrange
        var product = new Product { Id = Guid.NewGuid(), Name = "SKU Product", SKU = "SKU-001", CreatedBy = "test" };
        _productRepoMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .ReturnsAsync(product);

        // Act
        var result = await _sut.GetBySkuAsync("SKU-001");

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.SKU.Should().Be("SKU-001");
    }
}
