using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using AppilicoShopServer.Business.DTOs.Review;
using AppilicoShopServer.Business.Services;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Interfaces;
using AppilicoShopServer.UnitTests.Helpers;
using System.Linq.Expressions;

namespace AppilicoShopServer.UnitTests.Services;

public class ReviewServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IReviewRepository> _reviewRepoMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<IOrderRepository> _orderRepoMock;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<ReviewService>> _loggerMock;
    private readonly ReviewService _sut;

    public ReviewServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _reviewRepoMock = new Mock<IReviewRepository>();
        _productRepoMock = new Mock<IProductRepository>();
        _orderRepoMock = new Mock<IOrderRepository>();
        _mapper = TestMapperConfig.CreateMapper();
        _loggerMock = new Mock<ILogger<ReviewService>>();

        _unitOfWorkMock.Setup(u => u.Reviews).Returns(_reviewRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Orders).Returns(_orderRepoMock.Object);

        _sut = new ReviewService(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByProductAsync_ReturnsPagedReviews()
    {
        var productId = Guid.NewGuid();
        var reviews = new List<ProductReview>
        {
            new() { Id = Guid.NewGuid(), ProductId = productId, Rating = 5, Title = "Great!", Comment = "Loved it", IsApproved = true, CreatedBy = "test" }
        };
        _reviewRepoMock.Setup(r => r.GetPagedAsync(
                It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<Expression<Func<ProductReview, bool>>>(),
                It.IsAny<Func<IQueryable<ProductReview>, IOrderedQueryable<ProductReview>>>(),
                It.IsAny<Expression<Func<ProductReview, object>>[]>()))
            .ReturnsAsync((reviews.AsReadOnly() as IReadOnlyList<ProductReview>, 1));

        var result = await _sut.GetByProductAsync(productId);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingReview_ReturnsSuccess()
    {
        var reviewId = Guid.NewGuid();
        var review = new ProductReview { Id = reviewId, Rating = 5, Title = "Excellent", Comment = "Best product", CreatedBy = "test" };
        _reviewRepoMock.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync(review);

        var result = await _sut.GetByIdAsync(reviewId);

        result.Success.Should().BeTrue();
        result.Data!.Rating.Should().Be(5);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingReview_ReturnsFail()
    {
        _reviewRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ProductReview?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_NewReview_ReturnsSuccess()
    {
        var productId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        // No existing review for this product by this customer
        _reviewRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<ProductReview, bool>>>())).ReturnsAsync(false);
        // Check verified purchase
        _orderRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Order, bool>>>())).ReturnsAsync(true);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var request = new CreateReviewRequest { ProductId = productId, Rating = 5, Title = "Amazing", Comment = "Very good" };
        var result = await _sut.CreateAsync(customerId, request, "user1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_DuplicateReview_ReturnsFail()
    {
        var productId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        // Customer already reviewed this product
        _reviewRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<ProductReview, bool>>>())).ReturnsAsync(true);

        var request = new CreateReviewRequest { ProductId = productId, Rating = 4, Title = "Good" };
        var result = await _sut.CreateAsync(customerId, request, "user1");

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ExistingReview_ReturnsSuccess()
    {
        var reviewId = Guid.NewGuid();
        var review = new ProductReview { Id = reviewId, Rating = 3, Title = "OK", Comment = "Average", CreatedBy = "user1" };
        _reviewRepoMock.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync(review);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var request = new UpdateReviewRequest { Rating = 4, Title = "Better now", Comment = "Updated" };
        var result = await _sut.UpdateAsync(reviewId, request, "user1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_NonExistingReview_ReturnsFail()
    {
        _reviewRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ProductReview?)null);

        var request = new UpdateReviewRequest { Rating = 4, Title = "Updated" };
        var result = await _sut.UpdateAsync(Guid.NewGuid(), request, "user1");

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ExistingReview_ReturnsSuccess()
    {
        var reviewId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var review = new ProductReview { Id = reviewId, ProductId = productId, Rating = 3, CreatedBy = "user1" };
        _reviewRepoMock.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync(review);
        _productRepoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(new Product { Id = productId, Name = "P1", CreatedBy = "test" });
        _reviewRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ProductReview, bool>>>())).ReturnsAsync(new List<ProductReview>());
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.DeleteAsync(reviewId, "user1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_NonExistingReview_ReturnsFail()
    {
        _reviewRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ProductReview?)null);

        var result = await _sut.DeleteAsync(Guid.NewGuid(), "user1");

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task ApproveAsync_PendingReview_ReturnsSuccess()
    {
        var reviewId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var review = new ProductReview { Id = reviewId, ProductId = productId, Rating = 5, IsApproved = false, CreatedBy = "user1" };
        _reviewRepoMock.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync(review);
        _productRepoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(new Product { Id = productId, Name = "P1", CreatedBy = "test" });
        _reviewRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ProductReview, bool>>>()))
            .ReturnsAsync(new List<ProductReview> { review });
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.ApproveAsync(reviewId, "admin1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ApproveAsync_NonExistingReview_ReturnsFail()
    {
        _reviewRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ProductReview?)null);

        var result = await _sut.ApproveAsync(Guid.NewGuid(), "admin1");

        result.Success.Should().BeFalse();
    }
}
