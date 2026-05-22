using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Appilico.Server.Business.DTOs.Blog;
using Appilico.Server.Business.Services;
using Appilico.Server.DataAccess.Data;
using Appilico.Server.DataAccess.Repositories;
using Appilico.Server.Domain.Entities;

namespace Appilico.Server.UnitTests.Services;

public class BlogServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly BlogService _sut;

    public BlogServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);
        var logger = new Mock<ILogger<BlogService>>().Object;
        _sut = new BlogService(new UnitOfWork(_db), logger);
    }

    public void Dispose() => _db.Dispose();

    private static BlogPost MakePost(string title, string category = "Analytics", bool published = true) => new()
    {
        Title = title,
        Slug = title.ToLowerInvariant().Replace(" ", "-"),
        Excerpt = "Test excerpt",
        Content = "Word count content that is long enough to be readable and informative for unit testing purposes",
        Category = category,
        Author = "Appilico Engineering",
        PublishedAt = published ? DateTime.UtcNow.AddDays(-1) : null,
        IsPublished = published,
        IsDeleted = false
    };

    // ─── GetPostsAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetPostsAsync_ReturnsOnlyPublishedNonDeleted()
    {
        var deletedPost = new BlogPost { Title = "Deleted Post", Slug = "deleted", Excerpt = "x", Content = "x", Category = "Analytics", Author = "a", IsPublished = true };
        _db.BlogPosts.AddRange(
            MakePost("Published Post"),
            MakePost("Draft Post", published: false),
            deletedPost);
        await _db.SaveChangesAsync();

        // Mark the post as deleted via a second save (AppDbContext.SaveChangesAsync resets IsDeleted=false on Add)
        deletedPost.IsDeleted = true;
        await _db.SaveChangesAsync();

        var result = await _sut.GetPostsAsync(null, 1, 10);

        result.Success.Should().BeTrue();
        result.Data!.TotalCount.Should().Be(1);
        result.Data.Items.First().Title.Should().Be("Published Post");
    }

    [Fact]
    public async Task GetPostsAsync_FiltersByCategory()
    {
        _db.BlogPosts.AddRange(MakePost("Mining", "Mining"), MakePost("Tech", "Technology"));
        await _db.SaveChangesAsync();

        var result = await _sut.GetPostsAsync("Mining", 1, 10);

        result.Data!.Items.Should().HaveCount(1);
        result.Data.Items[0].Title.Should().Be("Mining");
    }

    [Fact]
    public async Task GetPostsAsync_PaginatesCorrectly()
    {
        for (var i = 1; i <= 5; i++)
            _db.BlogPosts.Add(MakePost($"Post {i}"));
        await _db.SaveChangesAsync();

        var result = await _sut.GetPostsAsync(null, 2, 2);

        result.Data!.Page.Should().Be(2);
        result.Data.Items.Should().HaveCount(2);
        result.Data.TotalCount.Should().Be(5);
        result.Data.TotalPages.Should().Be(3);
    }

    // ─── GetPostBySlugAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task GetPostBySlugAsync_ExistingSlug_ReturnsPost()
    {
        _db.BlogPosts.Add(MakePost("Hello World"));
        await _db.SaveChangesAsync();

        var result = await _sut.GetPostBySlugAsync("hello-world");

        result.Success.Should().BeTrue();
        result.Data!.Slug.Should().Be("hello-world");
    }

    [Fact]
    public async Task GetPostBySlugAsync_UnknownSlug_ReturnsFailure()
    {
        var result = await _sut.GetPostBySlugAsync("does-not-exist");

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task GetPostBySlugAsync_PopulatesRelatedPosts_SameCategory()
    {
        _db.BlogPosts.AddRange(
            MakePost("Main Post", "Analytics"),
            MakePost("Related One", "Analytics"),
            MakePost("Related Two", "Analytics"),
            MakePost("Different Category", "Technology"));
        await _db.SaveChangesAsync();

        var result = await _sut.GetPostBySlugAsync("main-post");

        result.Data!.RelatedPosts.Should().HaveCount(2);
        result.Data.RelatedPosts.All(p => p.Category == "Analytics").Should().BeTrue();
    }

    // ─── CreatePostAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task CreatePostAsync_GeneratesSlugFromTitle()
    {
        var request = new CreateBlogPostRequest
        {
            Title = "Hello World Test",
            Excerpt = "excerpt",
            Content = "some content words here to read",
            Category = "Analytics",
            Author = "Appilico Engineering",
            IsPublished = false
        };

        var result = await _sut.CreatePostAsync(request);

        result.Success.Should().BeTrue();
        result.Data!.Slug.Should().Be("hello-world-test");
    }

    [Fact]
    public async Task CreatePostAsync_SetsPublishedAtWhenIsPublishedTrue()
    {
        var request = new CreateBlogPostRequest
        {
            Title = "Published Now",
            Excerpt = "excerpt",
            Content = "content here",
            Category = "Analytics",
            Author = "Appilico Engineering",
            IsPublished = true
        };

        var result = await _sut.CreatePostAsync(request);

        result.Data!.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CreatePostAsync_CalculatesReadTimeMinutes()
    {
        // 200 words → 1 min, 400 words → 2 min
        var words = string.Join(" ", Enumerable.Repeat("word", 420));
        var request = new CreateBlogPostRequest
        {
            Title = "Long Post",
            Excerpt = "x",
            Content = words,
            Category = "Analytics",
            Author = "Appilico Engineering",
            IsPublished = false
        };

        var result = await _sut.CreatePostAsync(request);

        result.Data!.ReadTimeMinutes.Should().Be(3); // ceil(420/200) = 3
    }

    // ─── UpdatePostAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task UpdatePostAsync_PartialUpdate_OnlyChangesProvidedFields()
    {
        var post = MakePost("Original Title");
        _db.BlogPosts.Add(post);
        await _db.SaveChangesAsync();

        var request = new UpdateBlogPostRequest { Excerpt = "Updated excerpt" };
        var result = await _sut.UpdatePostAsync(post.Id, request);

        result.Success.Should().BeTrue();
        result.Data!.Title.Should().Be("Original Title"); // unchanged
        result.Data.Excerpt.Should().Be("Updated excerpt");
    }

    [Fact]
    public async Task UpdatePostAsync_NonExistentId_ReturnsFailure()
    {
        var result = await _sut.UpdatePostAsync(Guid.NewGuid(), new UpdateBlogPostRequest());
        result.Success.Should().BeFalse();
    }

    // ─── DeletePostAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task DeletePostAsync_SoftDeletes_PostNotReturnedAfterwards()
    {
        var post = MakePost("To Delete");
        _db.BlogPosts.Add(post);
        await _db.SaveChangesAsync();

        var deleteResult = await _sut.DeletePostAsync(post.Id);
        deleteResult.Success.Should().BeTrue();

        var fetchResult = await _sut.GetPostBySlugAsync(post.Slug);
        fetchResult.Success.Should().BeFalse(); // soft deleted, not visible
    }

    // ─── PublishPostAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task PublishPostAsync_SetIsPublishedAndPublishedAt()
    {
        var post = MakePost("Draft", published: false);
        _db.BlogPosts.Add(post);
        await _db.SaveChangesAsync();

        var result = await _sut.PublishPostAsync(post.Id);

        result.Success.Should().BeTrue();
        result.Data!.IsPublished.Should().BeTrue();
        result.Data.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task PublishPostAsync_AlreadyPublished_DoesNotResetPublishedAt()
    {
        var originalDate = DateTime.UtcNow.AddDays(-5);
        var post = MakePost("Already Published");
        post.PublishedAt = originalDate;
        _db.BlogPosts.Add(post);
        await _db.SaveChangesAsync();

        var result = await _sut.PublishPostAsync(post.Id);

        result.Data!.PublishedAt.Should().BeCloseTo(originalDate, TimeSpan.FromSeconds(1));
    }
}
