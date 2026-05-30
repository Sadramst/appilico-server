using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using AppilicoShopServer.Business.DTOs.Brand;
using AppilicoShopServer.Business.Services;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Interfaces;
using AppilicoShopServer.UnitTests.Helpers;

namespace AppilicoShopServer.UnitTests.Services;

public class BrandServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IBrandRepository> _brandRepoMock;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<BrandService>> _loggerMock;
    private readonly BrandService _sut;

    public BrandServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _brandRepoMock = new Mock<IBrandRepository>();
        _mapper = TestMapperConfig.CreateMapper();
        _loggerMock = new Mock<ILogger<BrandService>>();

        _unitOfWorkMock.Setup(u => u.Brands).Returns(_brandRepoMock.Object);

        _sut = new BrandService(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingBrand_ReturnsSuccess()
    {
        var brandId = Guid.NewGuid();
        var brand = new Brand { Id = brandId, Name = "TechPro", IsActive = true, CreatedBy = "test" };
        _brandRepoMock.Setup(r => r.GetByIdAsync(brandId)).ReturnsAsync(brand);

        var result = await _sut.GetByIdAsync(brandId);

        result.Success.Should().BeTrue();
        result.Data!.Name.Should().Be("TechPro");
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsSuccess()
    {
        var request = new CreateBrandRequest { Name = "NewBrand", Description = "A new brand" };
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.CreateAsync(request, "user1");

        result.Success.Should().BeTrue();
        result.Data!.Name.Should().Be("NewBrand");
    }

    [Fact]
    public async Task DeleteAsync_ExistingBrand_ReturnsSuccess()
    {
        var brandId = Guid.NewGuid();
        var brand = new Brand { Id = brandId, Name = "Test", CreatedBy = "test" };
        _brandRepoMock.Setup(r => r.GetByIdAsync(brandId)).ReturnsAsync(brand);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.DeleteAsync(brandId, "user1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllBrands()
    {
        var brands = new List<Brand>
        {
            new() { Id = Guid.NewGuid(), Name = "Brand1", CreatedBy = "test" },
            new() { Id = Guid.NewGuid(), Name = "Brand2", CreatedBy = "test" }
        };
        _brandRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(brands);

        var result = await _sut.GetAllAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_EmptyList_ReturnsEmptyList()
    {
        _brandRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Brand>());

        var result = await _sut.GetAllAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingBrand_ReturnsFail()
    {
        _brandRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Brand?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Brand not found");
    }

    [Fact]
    public async Task UpdateAsync_ExistingBrand_ReturnsSuccess()
    {
        var brandId = Guid.NewGuid();
        var brand = new Brand { Id = brandId, Name = "Old Name", CreatedBy = "test" };
        _brandRepoMock.Setup(r => r.GetByIdAsync(brandId)).ReturnsAsync(brand);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var request = new UpdateBrandRequest { Name = "Updated Name", Description = "Updated desc" };
        var result = await _sut.UpdateAsync(brandId, request, "user1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_NonExistingBrand_ReturnsFail()
    {
        _brandRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Brand?)null);

        var request = new UpdateBrandRequest { Name = "Updated" };
        var result = await _sut.UpdateAsync(Guid.NewGuid(), request, "user1");

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_NonExistingBrand_ReturnsFail()
    {
        _brandRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Brand?)null);

        var result = await _sut.DeleteAsync(Guid.NewGuid(), "user1");

        result.Success.Should().BeFalse();
    }
}
