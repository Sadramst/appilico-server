using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using AppilicoShopServer.Business.DTOs.Category;
using AppilicoShopServer.Business.Services;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Interfaces;
using AppilicoShopServer.UnitTests.Helpers;

namespace AppilicoShopServer.UnitTests.Services;

public class CategoryServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICategoryRepository> _categoryRepoMock;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<CategoryService>> _loggerMock;
    private readonly CategoryService _sut;

    public CategoryServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _categoryRepoMock = new Mock<ICategoryRepository>();
        _mapper = TestMapperConfig.CreateMapper();
        _loggerMock = new Mock<ILogger<CategoryService>>();

        _unitOfWorkMock.Setup(u => u.Categories).Returns(_categoryRepoMock.Object);

        _sut = new CategoryService(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingCategory_ReturnsSuccess()
    {
        var categoryId = Guid.NewGuid();
        var category = new Category { Id = categoryId, Name = "Electronics", IsActive = true, CreatedBy = "test" };
        _categoryRepoMock.Setup(r => r.GetByIdAsync(categoryId)).ReturnsAsync(category);

        var result = await _sut.GetByIdAsync(categoryId);

        result.Success.Should().BeTrue();
        result.Data!.Name.Should().Be("Electronics");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistent_ReturnsFailure()
    {
        var categoryId = Guid.NewGuid();
        _categoryRepoMock.Setup(r => r.GetByIdAsync(categoryId)).ReturnsAsync((Category?)null);

        var result = await _sut.GetByIdAsync(categoryId);

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsSuccess()
    {
        var request = new CreateCategoryRequest { Name = "Clothing", Description = "Apparel", SortOrder = 1 };
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.CreateAsync(request, "user1");

        result.Success.Should().BeTrue();
        result.Data!.Name.Should().Be("Clothing");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllCategories()
    {
        var categories = new List<Category>
        {
            new() { Id = Guid.NewGuid(), Name = "Cat1", IsActive = true, CreatedBy = "test" },
            new() { Id = Guid.NewGuid(), Name = "Cat2", IsActive = true, CreatedBy = "test" }
        };
        _categoryRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);

        var result = await _sut.GetAllAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_EmptyList_ReturnsEmpty()
    {
        _categoryRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Category>());

        var result = await _sut.GetAllAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCategoryTreeAsync_ReturnsTree()
    {
        var tree = new List<Category>
        {
            new() { Id = Guid.NewGuid(), Name = "Root", CreatedBy = "test", SubCategories = new List<Category>
            {
                new() { Id = Guid.NewGuid(), Name = "Child", CreatedBy = "test" }
            }}
        };
        _categoryRepoMock.Setup(r => r.GetCategoryTreeAsync()).ReturnsAsync(tree);

        var result = await _sut.GetCategoryTreeAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateAsync_ExistingCategory_ReturnsSuccess()
    {
        var categoryId = Guid.NewGuid();
        var category = new Category { Id = categoryId, Name = "Old", CreatedBy = "test" };
        _categoryRepoMock.Setup(r => r.GetByIdAsync(categoryId)).ReturnsAsync(category);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var request = new UpdateCategoryRequest { Name = "Updated", SortOrder = 2 };
        var result = await _sut.UpdateAsync(categoryId, request, "user1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_NonExistingCategory_ReturnsFail()
    {
        _categoryRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Category?)null);

        var request = new UpdateCategoryRequest { Name = "Updated" };
        var result = await _sut.UpdateAsync(Guid.NewGuid(), request, "user1");

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ExistingCategory_ReturnsSuccess()
    {
        var categoryId = Guid.NewGuid();
        var category = new Category { Id = categoryId, Name = "ToDelete", CreatedBy = "test" };
        _categoryRepoMock.Setup(r => r.GetByIdAsync(categoryId)).ReturnsAsync(category);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.DeleteAsync(categoryId, "user1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_NonExistingCategory_ReturnsFail()
    {
        _categoryRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Category?)null);

        var result = await _sut.DeleteAsync(Guid.NewGuid(), "user1");

        result.Success.Should().BeFalse();
    }
}
