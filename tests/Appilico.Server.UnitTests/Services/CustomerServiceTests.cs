using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Appilico.Server.Business.DTOs.Customer;
using Appilico.Server.Business.Services;
using Appilico.Server.Domain.Entities;
using Appilico.Server.Domain.Enums;
using Appilico.Server.Domain.Interfaces;
using Appilico.Server.UnitTests.Helpers;
using System.Linq.Expressions;

namespace Appilico.Server.UnitTests.Services;

public class CustomerServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICustomerRepository> _customerRepoMock;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<CustomerService>> _loggerMock;
    private readonly CustomerService _sut;

    public CustomerServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _customerRepoMock = new Mock<ICustomerRepository>();
        _mapper = TestMapperConfig.CreateMapper();
        _loggerMock = new Mock<ILogger<CustomerService>>();

        _unitOfWorkMock.Setup(u => u.Customers).Returns(_customerRepoMock.Object);

        _sut = new CustomerService(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingCustomer_ReturnsSuccess()
    {
        var customer = CreateTestCustomer();
        _customerRepoMock.Setup(r => r.GetWithAddressesAsync(customer.Id)).ReturnsAsync(customer);

        var result = await _sut.GetByIdAsync(customer.Id);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingCustomer_ReturnsFail()
    {
        _customerRepoMock.Setup(r => r.GetWithAddressesAsync(It.IsAny<Guid>())).ReturnsAsync((Customer?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task GetByUserIdAsync_ExistingUser_ReturnsSuccess()
    {
        var userId = "user-123";
        var customer = CreateTestCustomer(userId: userId);
        _customerRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(customer);

        var result = await _sut.GetByUserIdAsync(userId);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetByUserIdAsync_NonExistingUser_ReturnsFail()
    {
        _customerRepoMock.Setup(r => r.GetByUserIdAsync("nonexistent")).ReturnsAsync((Customer?)null);

        var result = await _sut.GetByUserIdAsync("nonexistent");

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPagedCustomers()
    {
        var customers = new List<Customer> { CreateTestCustomer(), CreateTestCustomer() };
        _customerRepoMock.Setup(r => r.GetPagedAsync(
                It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<Expression<Func<Customer, bool>>>(),
                It.IsAny<Func<IQueryable<Customer>, IOrderedQueryable<Customer>>>(),
                It.IsAny<Expression<Func<Customer, object>>[]>()))
            .ReturnsAsync((customers.AsReadOnly() as IReadOnlyList<Customer>, 2));

        var result = await _sut.GetAllAsync(1, 10);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ExistingCustomer_ReturnsSuccess()
    {
        var customer = CreateTestCustomer();
        customer.User = new AppUser { FirstName = "John", LastName = "Doe" };
        _customerRepoMock.Setup(r => r.GetWithAddressesAsync(customer.Id)).ReturnsAsync(customer);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var request = new UpdateCustomerRequest { FirstName = "Updated", LastName = "Customer", PhoneNumber = "555-1234" };
        var result = await _sut.UpdateAsync(customer.Id, request, "admin1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_NonExistingCustomer_ReturnsFail()
    {
        _customerRepoMock.Setup(r => r.GetWithAddressesAsync(It.IsAny<Guid>())).ReturnsAsync((Customer?)null);

        var request = new UpdateCustomerRequest { FirstName = "Updated", LastName = "Customer" };
        var result = await _sut.UpdateAsync(Guid.NewGuid(), request, "admin1");

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task GetLoyaltyAsync_ExistingCustomer_ReturnsLoyalty()
    {
        var customer = CreateTestCustomer();
        customer.LoyaltyPoints = 500;
        customer.TotalPurchases = 1500m;
        _customerRepoMock.Setup(r => r.GetByIdAsync(customer.Id)).ReturnsAsync(customer);

        var result = await _sut.GetLoyaltyAsync(customer.Id);

        result.Success.Should().BeTrue();
        result.Data!.LoyaltyPoints.Should().Be(500);
    }

    [Fact]
    public async Task GetLoyaltyAsync_NonExistingCustomer_ReturnsFail()
    {
        _customerRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Customer?)null);

        var result = await _sut.GetLoyaltyAsync(Guid.NewGuid());

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task AddLoyaltyPointsAsync_ExistingCustomer_ReturnsSuccess()
    {
        var customer = CreateTestCustomer();
        customer.LoyaltyPoints = 100;
        _customerRepoMock.Setup(r => r.GetByIdAsync(customer.Id)).ReturnsAsync(customer);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.AddLoyaltyPointsAsync(customer.Id, 50);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task AddLoyaltyPointsAsync_NonExistingCustomer_ReturnsFail()
    {
        _customerRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Customer?)null);

        var result = await _sut.AddLoyaltyPointsAsync(Guid.NewGuid(), 50);

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task GetAddressesAsync_ExistingCustomer_ReturnsAddresses()
    {
        var userId = "user-123";
        var customer = CreateTestCustomer(userId: userId);
        customer.Addresses = new List<CustomerAddress>
        {
            new() { Id = Guid.NewGuid(), Title = "Home", AddressLine1 = "123 Main St", City = "Springfield", Country = "US", PostalCode = "12345", AddressType = AddressType.Shipping, CreatedBy = "test" }
        };
        _customerRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(customer);

        var result = await _sut.GetAddressesAsync(userId);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetAddressesAsync_NonExistingCustomer_ReturnsFail()
    {
        _customerRepoMock.Setup(r => r.GetByUserIdAsync("nope")).ReturnsAsync((Customer?)null);

        var result = await _sut.GetAddressesAsync("nope");

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAddressAsync_ValidAddress_ReturnsSuccess()
    {
        var userId = "user-123";
        var customer = CreateTestCustomer(userId: userId);
        customer.Addresses = new List<CustomerAddress>();
        _customerRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(customer);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var request = new CreateAddressRequest
        {
            Title = "Home",
            AddressLine1 = "456 Oak Ave",
            City = "Portland",
            State = "OR",
            PostalCode = "97201",
            Country = "US",
            AddressType = AddressType.Shipping,
            IsDefault = true
        };

        var result = await _sut.CreateAddressAsync(userId, request);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAddressAsync_NonExistingCustomer_ReturnsFail()
    {
        _customerRepoMock.Setup(r => r.GetByUserIdAsync("nope")).ReturnsAsync((Customer?)null);

        var request = new CreateAddressRequest { Title = "Home", AddressLine1 = "123 St", City = "City", PostalCode = "12345", Country = "US", AddressType = AddressType.Shipping };
        var result = await _sut.CreateAddressAsync("nope", request);

        result.Success.Should().BeFalse();
    }

    // ──────── UpdateAddressAsync ────────

    [Fact]
    public async Task UpdateAddressAsync_ExistingAddress_ReturnsSuccess()
    {
        var userId = "user-123";
        var addressId = Guid.NewGuid();
        var customer = CreateTestCustomer(userId: userId);
        customer.Addresses = new List<CustomerAddress>
        {
            new() { Id = addressId, Title = "Home", AddressLine1 = "123 St", City = "Springfield", Country = "US", PostalCode = "12345", AddressType = AddressType.Shipping, CreatedBy = "test" }
        };
        _customerRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(customer);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var request = new UpdateAddressRequest
        {
            Title = "Work", AddressLine1 = "456 Oak Ave", City = "Portland",
            PostalCode = "97201", Country = "US", AddressType = AddressType.Billing, IsDefault = false
        };
        var result = await _sut.UpdateAddressAsync(userId, addressId, request);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAddressAsync_NonExistingCustomer_ReturnsFail()
    {
        _customerRepoMock.Setup(r => r.GetByUserIdAsync("nope")).ReturnsAsync((Customer?)null);

        var request = new UpdateAddressRequest { Title = "Home", AddressLine1 = "123 St", City = "City", PostalCode = "12345", Country = "US", AddressType = AddressType.Shipping };
        var result = await _sut.UpdateAddressAsync("nope", Guid.NewGuid(), request);

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAddressAsync_AddressNotFound_ReturnsFail()
    {
        var userId = "user-123";
        var customer = CreateTestCustomer(userId: userId);
        customer.Addresses = new List<CustomerAddress>();
        _customerRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(customer);

        var request = new UpdateAddressRequest { Title = "Home", AddressLine1 = "123 St", City = "City", PostalCode = "12345", Country = "US", AddressType = AddressType.Shipping };
        var result = await _sut.UpdateAddressAsync(userId, Guid.NewGuid(), request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Address not found");
    }

    // ──────── DeleteAddressAsync ────────

    [Fact]
    public async Task DeleteAddressAsync_ExistingAddress_ReturnsSuccess()
    {
        var userId = "user-123";
        var addressId = Guid.NewGuid();
        var customer = CreateTestCustomer(userId: userId);
        customer.Addresses = new List<CustomerAddress>
        {
            new() { Id = addressId, Title = "Home", AddressLine1 = "123 St", City = "Springfield", Country = "US", PostalCode = "12345", AddressType = AddressType.Shipping, CreatedBy = "test" }
        };
        _customerRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(customer);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.DeleteAddressAsync(userId, addressId);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAddressAsync_NonExistingCustomer_ReturnsFail()
    {
        _customerRepoMock.Setup(r => r.GetByUserIdAsync("nope")).ReturnsAsync((Customer?)null);

        var result = await _sut.DeleteAddressAsync("nope", Guid.NewGuid());

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAddressAsync_AddressNotFound_ReturnsFail()
    {
        var userId = "user-123";
        var customer = CreateTestCustomer(userId: userId);
        customer.Addresses = new List<CustomerAddress>();
        _customerRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(customer);

        var result = await _sut.DeleteAddressAsync(userId, Guid.NewGuid());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Address not found");
    }

    private static Customer CreateTestCustomer(string? userId = null)
    {
        return new Customer
        {
            Id = Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid().ToString(),
            CustomerCode = "CUST-001",
            MembershipTier = MembershipTier.Bronze,
            LoyaltyPoints = 0,
            JoinDate = DateTime.UtcNow,
            CreatedBy = "test",
            Addresses = new List<CustomerAddress>()
        };
    }
}
