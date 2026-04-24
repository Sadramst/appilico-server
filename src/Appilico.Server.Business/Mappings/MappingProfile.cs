using AutoMapper;
using Appilico.Server.Business.DTOs.Auth;
using Appilico.Server.Business.DTOs.Brand;
using Appilico.Server.Business.DTOs.Cart;
using Appilico.Server.Business.DTOs.Category;
using Appilico.Server.Business.DTOs.Customer;
using Appilico.Server.Business.DTOs.Discount;
using Appilico.Server.Business.DTOs.Inventory;
using Appilico.Server.Business.DTOs.Offer;
using Appilico.Server.Business.DTOs.Order;
using Appilico.Server.Business.DTOs.Payment;
using Appilico.Server.Business.DTOs.Product;
using Appilico.Server.Business.DTOs.Review;
using Appilico.Server.Business.DTOs.Settings;
using Appilico.Server.Business.DTOs.Voucher;
using Appilico.Server.Business.DTOs.Wishlist;
using Appilico.Server.Domain.Entities;

namespace Appilico.Server.Business.Mappings;

/// <summary>
/// AutoMapper profile for all entity-to-DTO mappings.
/// </summary>
public class MappingProfile : Profile
{
    /// <summary>Initializes all mappings.</summary>
    public MappingProfile()
    {
        // Product mappings
        CreateMap<Domain.Entities.Product, ProductDto>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category != null ? s.Category.Name : null))
            .ForMember(d => d.BrandName, o => o.MapFrom(s => s.Brand != null ? s.Brand.Name : null))
            .ForMember(d => d.PrimaryImageUrl, o => o.MapFrom(s => s.Images.FirstOrDefault(i => i.IsPrimary) != null ? s.Images.FirstOrDefault(i => i.IsPrimary)!.ImageUrl : s.Images.FirstOrDefault() != null ? s.Images.FirstOrDefault()!.ImageUrl : null));
        CreateMap<CreateProductRequest, Domain.Entities.Product>();
        CreateMap<UpdateProductRequest, Domain.Entities.Product>();
        CreateMap<ProductImage, ProductImageDto>();
        CreateMap<ProductVariant, ProductVariantDto>();
        CreateMap<CreateProductVariantRequest, ProductVariant>();

        // Category mappings
        CreateMap<Domain.Entities.Category, CategoryDto>();
        CreateMap<CreateCategoryRequest, Domain.Entities.Category>();
        CreateMap<UpdateCategoryRequest, Domain.Entities.Category>();

        // Brand mappings
        CreateMap<Domain.Entities.Brand, BrandDto>();
        CreateMap<CreateBrandRequest, Domain.Entities.Brand>();
        CreateMap<UpdateBrandRequest, Domain.Entities.Brand>();

        // Customer mappings
        CreateMap<Domain.Entities.Customer, CustomerDto>()
            .ForMember(d => d.FirstName, o => o.MapFrom(s => s.User != null ? s.User.FirstName : string.Empty))
            .ForMember(d => d.LastName, o => o.MapFrom(s => s.User != null ? s.User.LastName : string.Empty))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.User != null ? s.User.Email : null))
            .ForMember(d => d.PhoneNumber, o => o.MapFrom(s => s.User != null ? s.User.PhoneNumber : null));
        CreateMap<CustomerAddress, CustomerAddressDto>();

        // Auth mappings
        CreateMap<AppUser, UserDto>();

        // Order mappings
        CreateMap<Domain.Entities.Order, OrderDto>()
            .ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Customer != null && s.Customer.User != null ? $"{s.Customer.User.FirstName} {s.Customer.User.LastName}" : null));
        CreateMap<OrderItem, OrderItemDto>();
        CreateMap<OrderStatusHistory, OrderStatusHistoryDto>();

        // Cart mappings
        CreateMap<Domain.Entities.Cart, CartDto>()
            .ForMember(d => d.Total, o => o.MapFrom(s => s.Items.Sum(i => i.UnitPrice * i.Quantity)));
        CreateMap<CartItem, CartItemDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : string.Empty))
            .ForMember(d => d.ImageUrl, o => o.MapFrom(s => s.Product != null && s.Product.Images.Any() ? s.Product.Images.FirstOrDefault()!.ImageUrl : null))
            .ForMember(d => d.VariantName, o => o.MapFrom(s => s.Variant != null ? s.Variant.VariantName : null))
            .ForMember(d => d.LineTotal, o => o.MapFrom(s => s.UnitPrice * s.Quantity));

        // Discount mappings
        CreateMap<Domain.Entities.Discount, DiscountDto>();
        CreateMap<CreateDiscountRequest, Domain.Entities.Discount>();
        CreateMap<UpdateDiscountRequest, Domain.Entities.Discount>();

        // Voucher mappings
        CreateMap<Domain.Entities.Voucher, VoucherDto>();
        CreateMap<CreateVoucherRequest, Domain.Entities.Voucher>();
        CreateMap<UpdateVoucherRequest, Domain.Entities.Voucher>();

        // Offer mappings
        CreateMap<SpecialOffer, SpecialOfferDto>()
            .ForMember(d => d.Products, o => o.MapFrom(s => s.SpecialOfferProducts));
        CreateMap<SpecialOfferProduct, SpecialOfferProductDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : string.Empty))
            .ForMember(d => d.OriginalPrice, o => o.MapFrom(s => s.Product != null ? s.Product.BasePrice : 0));
        CreateMap<CreateSpecialOfferRequest, SpecialOffer>();
        CreateMap<UpdateSpecialOfferRequest, SpecialOffer>();

        // Payment mappings
        CreateMap<Domain.Entities.Payment, PaymentDto>();
        CreateMap<CreatePaymentRequest, Domain.Entities.Payment>();
        CreateMap<Refund, RefundDto>();

        // Review mappings
        CreateMap<ProductReview, ReviewDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : null))
            .ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Customer != null && s.Customer.User != null ? $"{s.Customer.User.FirstName} {s.Customer.User.LastName}" : null));
        CreateMap<CreateReviewRequest, ProductReview>();
        CreateMap<UpdateReviewRequest, ProductReview>();

        // Wishlist mappings
        CreateMap<Domain.Entities.Wishlist, WishlistDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : string.Empty))
            .ForMember(d => d.Price, o => o.MapFrom(s => s.Product != null ? s.Product.BasePrice : 0))
            .ForMember(d => d.ImageUrl, o => o.MapFrom(s => s.Product != null && s.Product.Images.Any() ? s.Product.Images.FirstOrDefault()!.ImageUrl : null));

        // Inventory mappings
        CreateMap<InventoryTransaction, InventoryTransactionDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : null));
        CreateMap<Domain.Entities.Product, LowStockProductDto>();

        // Settings mappings
        CreateMap<AppSetting, AppSettingDto>();
    }
}
