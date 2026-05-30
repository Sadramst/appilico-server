using FluentAssertions;
using AutoMapper;
using AppilicoShopServer.Business.DTOs.Product;
using AppilicoShopServer.Business.DTOs.Category;
using AppilicoShopServer.Business.DTOs.Brand;
using AppilicoShopServer.Business.DTOs.Order;
using AppilicoShopServer.Business.DTOs.Discount;
using AppilicoShopServer.Business.DTOs.Voucher;
using AppilicoShopServer.Business.DTOs.Review;
using AppilicoShopServer.Business.DTOs.Customer;
using AppilicoShopServer.Business.DTOs.Offer;
using AppilicoShopServer.Business.DTOs.Payment;
using AppilicoShopServer.Business.Mappings;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;

namespace AppilicoShopServer.UnitTests.Mappings;

public class MappingProfileTests
{
    private readonly IMapper _mapper;

    public MappingProfileTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance);
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

    [Fact]
    public void Order_To_OrderDto_Maps_Correctly()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(), OrderNumber = "ORD-100",
            SubTotal = 100m, TaxAmount = 10m, TotalAmount = 110m,
            OrderStatus = OrderStatus.Pending,
            PaymentStatus = PaymentStatus.Pending,
            PaymentMethod = PaymentMethod.CreditCard,
            OrderDate = DateTime.UtcNow,
            CreatedBy = "test",
            Items = new List<OrderItem>()
        };

        var dto = _mapper.Map<OrderDto>(order);

        dto.OrderNumber.Should().Be("ORD-100");
        dto.TotalAmount.Should().Be(110m);
    }

    [Fact]
    public void Discount_To_DiscountDto_Maps_Correctly()
    {
        var discount = new Discount
        {
            Id = Guid.NewGuid(), Code = "SAVE20", Name = "Save 20",
            DiscountType = DiscountType.Percentage, Value = 20m,
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(30),
            IsActive = true, CreatedBy = "test"
        };

        var dto = _mapper.Map<DiscountDto>(discount);

        dto.Code.Should().Be("SAVE20");
        dto.Value.Should().Be(20m);
    }

    [Fact]
    public void Voucher_To_VoucherDto_Maps_Correctly()
    {
        var voucher = new Voucher
        {
            Id = Guid.NewGuid(), Code = "GIFT50",
            VoucherType = VoucherType.Gift, Value = 50m,
            ValueType = VoucherValueType.Fixed,
            StartDate = DateTime.UtcNow, ExpiryDate = DateTime.UtcNow.AddDays(90),
            IsActive = true, CreatedBy = "test"
        };

        var dto = _mapper.Map<VoucherDto>(voucher);

        dto.Code.Should().Be("GIFT50");
        dto.Value.Should().Be(50m);
    }

    [Fact]
    public void ProductReview_To_ReviewDto_Maps_Correctly()
    {
        var review = new ProductReview
        {
            Id = Guid.NewGuid(), ProductId = Guid.NewGuid(),
            Rating = 5, Title = "Great product", Comment = "Loved it",
            IsApproved = true, CreatedBy = "test"
        };

        var dto = _mapper.Map<ReviewDto>(review);

        dto.Rating.Should().Be(5);
        dto.Title.Should().Be("Great product");
    }

    [Fact]
    public void Customer_To_CustomerDto_Maps_Correctly()
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(), CustomerCode = "CUST-100",
            MembershipTier = MembershipTier.Gold,
            LoyaltyPoints = 500, TotalPurchases = 2000m,
            JoinDate = DateTime.UtcNow, CreatedBy = "test",
            Addresses = new List<CustomerAddress>()
        };

        var dto = _mapper.Map<CustomerDto>(customer);

        dto.CustomerCode.Should().Be("CUST-100");
    }

    [Fact]
    public void SpecialOffer_To_SpecialOfferDto_Maps_Correctly()
    {
        var offer = new SpecialOffer
        {
            Id = Guid.NewGuid(), Name = "Flash Sale",
            OfferType = OfferType.Flash,
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(7),
            IsActive = true, CreatedBy = "test",
            SpecialOfferProducts = new List<SpecialOfferProduct>()
        };

        var dto = _mapper.Map<SpecialOfferDto>(offer);

        dto.Name.Should().Be("Flash Sale");
    }

    [Fact]
    public void Payment_To_PaymentDto_Maps_Correctly()
    {
        var payment = new Payment
        {
            Id = Guid.NewGuid(), OrderId = Guid.NewGuid(),
            Amount = 100m, Status = PaymentStatus.Paid,
            PaymentMethod = PaymentMethod.CreditCard,
            CreatedBy = "test"
        };

        var dto = _mapper.Map<PaymentDto>(payment);

        dto.Amount.Should().Be(100m);
    }

    [Fact]
    public void CreateBrandRequest_To_Brand_Maps_Correctly()
    {
        var request = new CreateBrandRequest { Name = "NewBrand", Description = "A brand" };

        var brand = _mapper.Map<Brand>(request);

        brand.Name.Should().Be("NewBrand");
    }

    [Fact]
    public void CreateCategoryRequest_To_Category_Maps_Correctly()
    {
        var request = new CreateCategoryRequest { Name = "Electronics", SortOrder = 1 };

        var category = _mapper.Map<Category>(request);

        category.Name.Should().Be("Electronics");
    }
}
