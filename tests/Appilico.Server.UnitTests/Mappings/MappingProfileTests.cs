using FluentAssertions;
using AutoMapper;
using Appilico.Server.Business.DTOs.Product;
using Appilico.Server.Business.DTOs.Category;
using Appilico.Server.Business.DTOs.Brand;
using Appilico.Server.Business.Mappings;
using Appilico.Server.Domain.Entities;

namespace Appilico.Server.UnitTests.Mappings;

public class MappingProfileTests
{
    private readonly IMapper _mapper;

    public MappingProfileTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
    }

    [Fact]
    public void Product_To_ProductDto_Maps_Correctly()
    {
        var product = new Product
        {
            Id = Guid.NewGuid(), Name = "Test", SKU = "T-001",
            BasePrice = 10m, StockQuantity = 5, CreatedBy = "test"
        };

        var dto = _mapper.Map<ProductDto>(product);

        dto.Name.Should().Be("Test");
        dto.SKU.Should().Be("T-001");
        dto.BasePrice.Should().Be(10m);
    }

    [Fact]
    public void Category_To_CategoryDto_Maps_Correctly()
    {
        var category = new Category { Id = Guid.NewGuid(), Name = "Electronics", CreatedBy = "test" };

        var dto = _mapper.Map<CategoryDto>(category);

        dto.Name.Should().Be("Electronics");
    }

    [Fact]
    public void Brand_To_BrandDto_Maps_Correctly()
    {
        var brand = new Brand { Id = Guid.NewGuid(), Name = "TechPro", CreatedBy = "test" };

        var dto = _mapper.Map<BrandDto>(brand);

        dto.Name.Should().Be("TechPro");
    }
}
