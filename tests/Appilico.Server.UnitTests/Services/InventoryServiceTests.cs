using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Appilico.Server.Business.DTOs.Inventory;
using Appilico.Server.Business.Services;
using Appilico.Server.Domain.Entities;
using Appilico.Server.Domain.Enums;
using Appilico.Server.Domain.Interfaces;
using Appilico.Server.UnitTests.Helpers;
using System.Linq.Expressions;

namespace Appilico.Server.UnitTests.Services;

public class InventoryServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IInventoryRepository> _inventoryRepoMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<InventoryService>> _loggerMock;
    private readonly InventoryService _sut;

    public InventoryServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _inventoryRepoMock = new Mock<IInventoryRepository>();
        _productRepoMock = new Mock<IProductRepository>();
        _mapper = TestMapperConfig.CreateMapper();
        _loggerMock = new Mock<ILogger<InventoryService>>();

        _unitOfWorkMock.Setup(u => u.Inventory).Returns(_inventoryRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepoMock.Object);

        _sut = new InventoryService(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);
    }

    [Fact]
    public async Task GetTransactionsAsync_ReturnsPagedTransactions()
    {
        var productId = Guid.NewGuid();
        var transactions = new List<InventoryTransaction>
        {
            new() { Id = Guid.NewGuid(), ProductId = productId, Quantity = 10, TransactionType = InventoryTransactionType.StockIn, CreatedBy = "admin" },
            new() { Id = Guid.NewGuid(), ProductId = productId, Quantity = 2, TransactionType = InventoryTransactionType.StockOut, CreatedBy = "system" }
        };
        _inventoryRepoMock.Setup(r => r.GetPagedAsync(
                It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<Expression<Func<InventoryTransaction, bool>>>(),
                It.IsAny<Func<IQueryable<InventoryTransaction>, IOrderedQueryable<InventoryTransaction>>>(),
                It.IsAny<Expression<Func<InventoryTransaction, object>>[]>()))
            .ReturnsAsync((transactions.AsReadOnly() as IReadOnlyList<InventoryTransaction>, 2));

        var result = await _sut.GetTransactionsAsync(productId);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task AdjustAsync_StockIn_IncreasesStock()
    {
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId, Name = "Test", StockQuantity = 50, CreatedBy = "test" };
        _productRepoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var request = new AdjustInventoryRequest { ProductId = productId, Quantity = 10, TransactionType = InventoryTransactionType.StockIn, Notes = "Restocking" };
        var result = await _sut.AdjustAsync(request, "admin1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task AdjustAsync_Return_IncreasesStock()
    {
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId, Name = "Test", StockQuantity = 50, CreatedBy = "test" };
        _productRepoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var request = new AdjustInventoryRequest { ProductId = productId, Quantity = 5, TransactionType = InventoryTransactionType.Return };
        var result = await _sut.AdjustAsync(request, "admin1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task AdjustAsync_StockOut_DecreasesStock()
    {
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId, Name = "Test", StockQuantity = 50, CreatedBy = "test" };
        _productRepoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var request = new AdjustInventoryRequest { ProductId = productId, Quantity = 10, TransactionType = InventoryTransactionType.StockOut };
        var result = await _sut.AdjustAsync(request, "admin1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task AdjustAsync_NonExistingProduct_ReturnsFail()
    {
        _productRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Product?)null);

        var request = new AdjustInventoryRequest { ProductId = Guid.NewGuid(), Quantity = 10, TransactionType = InventoryTransactionType.StockIn };
        var result = await _sut.AdjustAsync(request, "admin1");

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task AdjustAsync_InsufficientStock_ReturnsFail()
    {
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId, Name = "Test", StockQuantity = 5, CreatedBy = "test" };
        _productRepoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);

        var request = new AdjustInventoryRequest { ProductId = productId, Quantity = 10, TransactionType = InventoryTransactionType.StockOut };
        var result = await _sut.AdjustAsync(request, "admin1");

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task GetLowStockAsync_ReturnsLowStockProducts()
    {
        var products = new List<Product>
        {
            new() { Id = Guid.NewGuid(), Name = "Low Stock Product", StockQuantity = 2, CreatedBy = "test" }
        };
        _inventoryRepoMock.Setup(r => r.GetLowStockProductsAsync()).ReturnsAsync(products);

        var result = await _sut.GetLowStockAsync();

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetLowStockAsync_NoLowStock_ReturnsEmptyList()
    {
        _inventoryRepoMock.Setup(r => r.GetLowStockProductsAsync()).ReturnsAsync(new List<Product>());

        var result = await _sut.GetLowStockAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }
}
