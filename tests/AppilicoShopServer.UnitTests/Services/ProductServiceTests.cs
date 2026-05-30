using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using AppilicoShopServer.Business.DTOs.Product;
using AppilicoShopServer.Business.Services;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Interfaces;
using AppilicoShopServer.UnitTests.Helpers;
using System.Linq.Expressions;

namespace AppilicoShopServer.UnitTests.Services;

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

    // ──────── GetByIdAsync ────────

    [Fact]
    public async Task GetByIdAsync_ExistingProduct_ReturnsSuccess()
    {
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId, Name = "Test Product", SKU = "TST-001",
            BasePrice = 29.99m, StockQuantity = 10, IsActive = true,
            CreatedBy = "test"
        };
        _productRepoMock.Setup(r => r.GetWithDetailsAsync(productId)).ReturnsAsync(product);

        var result = await _sut.GetByIdAsync(productId);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Test Product");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentProduct_ReturnsFailure()
    {
        var productId = Guid.NewGuid();
        _productRepoMock.Setup(r => r.GetWithDetailsAsync(productId)).ReturnsAsync((Product?)null);

        var result = await _sut.GetByIdAsync(productId);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCorrectSKU()
    {
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId, Name = "X", SKU = "SKU-999", BasePrice = 5m, CreatedBy = "test" };
        _productRepoMock.Setup(r => r.GetWithDetailsAsync(productId)).ReturnsAsync(product);

        var result = await _sut.GetByIdAsync(productId);

        result.Data!.SKU.Should().Be("SKU-999");
    }

    // ──────── GetBySkuAsync ────────

    [Fact]
    public async Task GetBySkuAsync_ExistingSku_ReturnsSuccess()
    {
        var product = new Product { Id = Guid.NewGuid(), Name = "By SKU", SKU = "FIND-ME", BasePrice = 10m, CreatedBy = "test" };
        _productRepoMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Product, bool>>>())).ReturnsAsync(product);

        var result = await _sut.GetBySkuAsync("FIND-ME");

        result.Success.Should().BeTrue();
        result.Data!.SKU.Should().Be("FIND-ME");
    }

    [Fact]
    public async Task GetBySkuAsync_NonExistingSku_ReturnsFail()
    {
        _productRepoMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Product, bool>>>())).ReturnsAsync((Product?)null);

        var result = await _sut.GetBySkuAsync("NOPE");

        result.Success.Should().BeFalse();
    }

    // ──────── SearchProductsAsync ────────

    [Fact]
    public async Task SearchProductsAsync_ReturnsPagedResults()
    {
        var products = new List<Product>
        {
            new() { Id = Guid.NewGuid(), Name = "P1", SKU = "S1", BasePrice = 10m, CreatedBy = "test" },
            new() { Id = Guid.NewGuid(), Name = "P2", SKU = "S2", BasePrice = 20m, CreatedBy = "test" }
        };
        _productRepoMock.Setup(r => r.SearchAsync(It.IsAny<string?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(),
                It.IsAny<decimal?>(), It.IsAny<decimal?>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<string?>(), It.IsAny<bool>()))
            .ReturnsAsync((products, 2));

        var request = new ProductSearchRequest { Page = 1, PageSize = 10 };
        var result = await _sut.SearchProductsAsync(request);

        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Pagination.Should().NotBeNull();
    }

    [Fact]
    public async Task SearchProductsAsync_EmptyResults_ReturnsEmptyList()
    {
        _productRepoMock.Setup(r => r.SearchAsync(It.IsAny<string?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(),
                It.IsAny<decimal?>(), It.IsAny<decimal?>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<string?>(), It.IsAny<bool>()))
            .ReturnsAsync((new List<Product>(), 0));

        var request = new ProductSearchRequest { SearchTerm = "nonexistent" };
        var result = await _sut.SearchProductsAsync(request);

        result.Success.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    // ──────── CreateAsync ────────

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsSuccess()
    {
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

        _productRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Product, bool>>>()))
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

        var result = await _sut.CreateAsync(request, "user1");

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("New Product");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DuplicateSKU_ReturnsFail()
    {
        _productRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Product, bool>>>()))
            .ReturnsAsync(true);

        var request = new CreateProductRequest { Name = "Dup", SKU = "EXISTING", BasePrice = 10m };
        var result = await _sut.CreateAsync(request, "user1");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("SKU already exists");
    }

    // ──────── UpdateAsync ────────

    [Fact]
    public async Task UpdateAsync_ExistingProduct_ReturnsSuccess()
    {
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId, Name = "Old", SKU = "P1", BasePrice = 10m, CreatedBy = "test" };
        _productRepoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _productRepoMock.Setup(r => r.GetWithDetailsAsync(productId)).ReturnsAsync(product);

        var request = new UpdateProductRequest { Name = "Updated", BasePrice = 20m };
        var result = await _sut.UpdateAsync(productId, request, "user1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_NonExistingProduct_ReturnsFail()
    {
        _productRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Product?)null);

        var request = new UpdateProductRequest { Name = "Updated" };
        var result = await _sut.UpdateAsync(Guid.NewGuid(), request, "user1");

        result.Success.Should().BeFalse();
    }

    // ──────── DeleteAsync ────────

    [Fact]
    public async Task DeleteAsync_ExistingProduct_ReturnsSuccess()
    {
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId, Name = "Test", SKU = "T1", CreatedBy = "test" };
        _productRepoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.DeleteAsync(productId, "user1");
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_NonExistentProduct_ReturnsFailure()
    {
        var productId = Guid.NewGuid();
        _productRepoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync((Product?)null);

        var result = await _sut.DeleteAsync(productId, "user1");

        result.Success.Should().BeFalse();
    }

    // ──────── GetFeaturedAsync ────────

    [Fact]
    public async Task GetFeaturedAsync_ReturnsFeaturedProducts()
    {
        var products = new List<Product>
        {
            new() { Id = Guid.NewGuid(), Name = "Featured 1", SKU = "F1", IsFeatured = true, BasePrice = 10m, CreatedBy = "test" },
            new() { Id = Guid.NewGuid(), Name = "Featured 2", SKU = "F2", IsFeatured = true, BasePrice = 20m, CreatedBy = "test" }
        };
        _productRepoMock.Setup(r => r.GetFeaturedAsync(It.IsAny<int>())).ReturnsAsync(products);

        var result = await _sut.GetFeaturedAsync(10);

        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetFeaturedAsync_NoFeatured_ReturnsEmptyList()
    {
        _productRepoMock.Setup(r => r.GetFeaturedAsync(It.IsAny<int>())).ReturnsAsync(new List<Product>());

        var result = await _sut.GetFeaturedAsync(10);

        result.Success.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    // ──────── AddVariantAsync ────────

    [Fact]
    public async Task AddVariantAsync_ExistingProduct_ReturnsSuccess()
    {
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId, Name = "Product", SKU = "P1", BasePrice = 10m, CreatedBy = "test",
            Variants = new List<ProductVariant>()
        };
        _productRepoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var request = new CreateProductVariantRequest
        {
            VariantName = "Large", SKU = "P1-L", Price = 12m, StockQuantity = 5
        };
        var result = await _sut.AddVariantAsync(productId, request, "user1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task AddVariantAsync_NonExistingProduct_ReturnsFail()
    {
        _productRepoMock.Setup(r => r.GetWithDetailsAsync(It.IsAny<Guid>())).ReturnsAsync((Product?)null);

        var request = new CreateProductVariantRequest { VariantName = "X", SKU = "X1", Price = 5m, StockQuantity = 1 };
        var result = await _sut.AddVariantAsync(Guid.NewGuid(), request, "user1");

        result.Success.Should().BeFalse();
    }
}
