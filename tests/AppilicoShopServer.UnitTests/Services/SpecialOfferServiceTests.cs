using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using AppilicoShopServer.Business.DTOs.Offer;
using AppilicoShopServer.Business.Services;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Enums;
using AppilicoShopServer.Domain.Interfaces;
using AppilicoShopServer.UnitTests.Helpers;

namespace AppilicoShopServer.UnitTests.Services;

public class SpecialOfferServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ISpecialOfferRepository> _offerRepoMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<SpecialOfferService>> _loggerMock;
    private readonly SpecialOfferService _sut;

    public SpecialOfferServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _offerRepoMock = new Mock<ISpecialOfferRepository>();
        _productRepoMock = new Mock<IProductRepository>();
        _mapper = TestMapperConfig.CreateMapper();
        _loggerMock = new Mock<ILogger<SpecialOfferService>>();

        _unitOfWorkMock.Setup(u => u.SpecialOffers).Returns(_offerRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepoMock.Object);

        _sut = new SpecialOfferService(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllOffers()
    {
        var offers = new List<SpecialOffer> { CreateTestOffer("Summer Sale"), CreateTestOffer("Winter Sale") };
        _offerRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(offers);

        var result = await _sut.GetAllAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingOffer_ReturnsSuccess()
    {
        var offer = CreateTestOffer("Summer Sale");
        _offerRepoMock.Setup(r => r.GetByIdAsync(offer.Id)).ReturnsAsync(offer);

        var result = await _sut.GetByIdAsync(offer.Id);

        result.Success.Should().BeTrue();
        result.Data!.Name.Should().Be("Summer Sale");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingOffer_ReturnsFail()
    {
        _offerRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((SpecialOffer?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_ValidOffer_ReturnsSuccess()
    {
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var request = new CreateSpecialOfferRequest
        {
            Name = "New Year Sale",
            Description = "30% off all items",
            OfferType = OfferType.Flash,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(30)
        };

        var result = await _sut.CreateAsync(request, "admin1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ExistingOffer_ReturnsSuccess()
    {
        var offer = CreateTestOffer("Old Name");
        _offerRepoMock.Setup(r => r.GetByIdAsync(offer.Id)).ReturnsAsync(offer);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var request = new UpdateSpecialOfferRequest { Name = "Updated Name", IsActive = true, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(60) };
        var result = await _sut.UpdateAsync(offer.Id, request, "admin1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_NonExistingOffer_ReturnsFail()
    {
        _offerRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((SpecialOffer?)null);

        var request = new UpdateSpecialOfferRequest { Name = "Updated", IsActive = true, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(60) };
        var result = await _sut.UpdateAsync(Guid.NewGuid(), request, "admin1");

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ExistingOffer_ReturnsSuccess()
    {
        var offer = CreateTestOffer("To Delete");
        _offerRepoMock.Setup(r => r.GetByIdAsync(offer.Id)).ReturnsAsync(offer);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.DeleteAsync(offer.Id, "admin1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_NonExistingOffer_ReturnsFail()
    {
        _offerRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((SpecialOffer?)null);

        var result = await _sut.DeleteAsync(Guid.NewGuid(), "admin1");

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task GetActiveAsync_ReturnsOnlyActiveOffers()
    {
        var offers = new List<SpecialOffer> { CreateTestOffer("Active Sale") };
        _offerRepoMock.Setup(r => r.GetActiveOffersAsync()).ReturnsAsync(offers);

        var result = await _sut.GetActiveAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddProductsAsync_ValidOffer_ReturnsSuccess()
    {
        var offerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var offer = CreateTestOffer("Sale", id: offerId);
        var product = new Product { Id = productId, Name = "Test Product", IsActive = true, CreatedBy = "test" };

        _offerRepoMock.Setup(r => r.GetWithProductsAsync(offerId)).ReturnsAsync(offer);
        _productRepoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var request = new AddOfferProductsRequest
        {
            Products = new List<OfferProductItem>
            {
                new() { ProductId = productId, OfferPrice = 25m }
            }
        };

        var result = await _sut.AddProductsAsync(offerId, request, "admin1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task AddProductsAsync_NonExistingOffer_ReturnsFail()
    {
        _offerRepoMock.Setup(r => r.GetWithProductsAsync(It.IsAny<Guid>())).ReturnsAsync((SpecialOffer?)null);

        var request = new AddOfferProductsRequest { Products = new List<OfferProductItem> { new() { ProductId = Guid.NewGuid(), OfferPrice = 25m } } };
        var result = await _sut.AddProductsAsync(Guid.NewGuid(), request, "admin1");

        result.Success.Should().BeFalse();
    }

    private static SpecialOffer CreateTestOffer(string name, Guid? id = null, bool isActive = true)
    {
        return new SpecialOffer
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Description = $"{name} description",
            OfferType = OfferType.Flash,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(30),
            IsActive = isActive,
            CreatedBy = "test",
            SpecialOfferProducts = new List<SpecialOfferProduct>()
        };
    }
}
