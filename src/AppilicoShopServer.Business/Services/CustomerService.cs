using AutoMapper;
using Microsoft.Extensions.Logging;
using AppilicoShopServer.Business.DTOs.Common;
using AppilicoShopServer.Business.DTOs.Customer;
using AppilicoShopServer.Business.Interfaces;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Interfaces;

namespace AppilicoShopServer.Business.Services;

/// <summary>Customer service implementation.</summary>
public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CustomerService> _logger;

    /// <summary>Initializes a new instance of CustomerService.</summary>
    public CustomerService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CustomerService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<List<CustomerDto>>> GetAllAsync(int page = 1, int pageSize = 10, string? search = null)
    {
        var (items, totalCount) = await _unitOfWork.Customers.GetPagedAsync(page, pageSize,
            string.IsNullOrEmpty(search) ? null : c => c.CustomerCode.Contains(search) || c.User.FirstName.Contains(search) || c.User.LastName.Contains(search),
            null,
            c => c.User);

        var dtos = _mapper.Map<List<CustomerDto>>(items);
        var pagination = PaginationMeta.Create(page, pageSize, totalCount);

        return ApiResponse<List<CustomerDto>>.SuccessResponse(dtos, pagination);
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<CustomerDto>> GetByIdAsync(Guid id)
    {
        var customer = await _unitOfWork.Customers.GetWithAddressesAsync(id);
        if (customer == null)
            return ApiResponse<CustomerDto>.FailResponse("Customer not found");

        return ApiResponse<CustomerDto>.SuccessResponse(_mapper.Map<CustomerDto>(customer));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<CustomerDto>> GetByUserIdAsync(string userId)
    {
        var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);
        if (customer == null)
            return ApiResponse<CustomerDto>.FailResponse("Customer not found");

        return ApiResponse<CustomerDto>.SuccessResponse(_mapper.Map<CustomerDto>(customer));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<CustomerDto>> UpdateAsync(Guid id, UpdateCustomerRequest request, string userId, bool canManageMembershipTier = false)
    {
        var customer = await _unitOfWork.Customers.GetWithAddressesAsync(id);
        if (customer == null)
            return ApiResponse<CustomerDto>.FailResponse("Customer not found");

        if (customer.User != null)
        {
            customer.User.FirstName = request.FirstName;
            customer.User.LastName = request.LastName;
            if (request.PhoneNumber != null) customer.User.PhoneNumber = request.PhoneNumber;
        }

        if (canManageMembershipTier && request.MembershipTier.HasValue)
            customer.MembershipTier = request.MembershipTier.Value;

        customer.UpdatedBy = userId;
        _unitOfWork.Customers.Update(customer);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Customer {CustomerId} updated by {UserId}", id, userId);
        return ApiResponse<CustomerDto>.SuccessResponse(_mapper.Map<CustomerDto>(customer), "Customer updated successfully");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<CustomerLoyaltyDto>> GetLoyaltyAsync(Guid customerId)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
        if (customer == null)
            return ApiResponse<CustomerLoyaltyDto>.FailResponse("Customer not found");

        var loyalty = new CustomerLoyaltyDto
        {
            CustomerId = customer.Id,
            LoyaltyPoints = customer.LoyaltyPoints,
            MembershipTier = customer.MembershipTier,
            TotalPurchases = customer.TotalPurchases
        };

        return ApiResponse<CustomerLoyaltyDto>.SuccessResponse(loyalty);
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<CustomerLoyaltyDto>> AddLoyaltyPointsAsync(Guid customerId, int points)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
        if (customer == null)
            return ApiResponse<CustomerLoyaltyDto>.FailResponse("Customer not found");

        customer.LoyaltyPoints += points;
        _unitOfWork.Customers.Update(customer);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("{Points} loyalty points added to customer {CustomerId}", points, customerId);

        return ApiResponse<CustomerLoyaltyDto>.SuccessResponse(new CustomerLoyaltyDto
        {
            CustomerId = customer.Id,
            LoyaltyPoints = customer.LoyaltyPoints,
            MembershipTier = customer.MembershipTier,
            TotalPurchases = customer.TotalPurchases
        });
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<List<CustomerAddressDto>>> GetAddressesAsync(string userId)
    {
        var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);
        if (customer == null)
            return ApiResponse<List<CustomerAddressDto>>.FailResponse("Customer not found");

        var dtos = _mapper.Map<List<CustomerAddressDto>>(customer.Addresses);
        return ApiResponse<List<CustomerAddressDto>>.SuccessResponse(dtos);
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<CustomerAddressDto>> CreateAddressAsync(string userId, CreateAddressRequest request)
    {
        var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);
        if (customer == null)
            return ApiResponse<CustomerAddressDto>.FailResponse("Customer not found");

        var address = new CustomerAddress
        {
            CustomerId = customer.Id,
            Title = request.Title,
            AddressLine1 = request.AddressLine1,
            AddressLine2 = request.AddressLine2,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            Country = request.Country,
            IsDefault = request.IsDefault,
            AddressType = request.AddressType,
            CreatedBy = userId
        };

        // If this is set as default, clear other defaults of same type
        if (request.IsDefault)
        {
            foreach (var existing in customer.Addresses.Where(a => a.AddressType == request.AddressType && a.IsDefault))
                existing.IsDefault = false;
        }

        customer.Addresses.Add(address);
        _unitOfWork.Customers.Update(customer);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Address created for customer {CustomerId}", customer.Id);
        return ApiResponse<CustomerAddressDto>.SuccessResponse(_mapper.Map<CustomerAddressDto>(address), "Address created successfully");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<CustomerAddressDto>> UpdateAddressAsync(string userId, Guid addressId, UpdateAddressRequest request)
    {
        var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);
        if (customer == null)
            return ApiResponse<CustomerAddressDto>.FailResponse("Customer not found");

        var address = customer.Addresses.FirstOrDefault(a => a.Id == addressId);
        if (address == null)
            return ApiResponse<CustomerAddressDto>.FailResponse("Address not found");

        address.Title = request.Title;
        address.AddressLine1 = request.AddressLine1;
        address.AddressLine2 = request.AddressLine2;
        address.City = request.City;
        address.State = request.State;
        address.PostalCode = request.PostalCode;
        address.Country = request.Country;
        address.IsDefault = request.IsDefault;
        address.AddressType = request.AddressType;
        address.UpdatedBy = userId;

        if (request.IsDefault)
        {
            foreach (var existing in customer.Addresses.Where(a => a.Id != addressId && a.AddressType == request.AddressType && a.IsDefault))
                existing.IsDefault = false;
        }

        _unitOfWork.Customers.Update(customer);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Address {AddressId} updated for customer {CustomerId}", addressId, customer.Id);
        return ApiResponse<CustomerAddressDto>.SuccessResponse(_mapper.Map<CustomerAddressDto>(address), "Address updated successfully");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<bool>> DeleteAddressAsync(string userId, Guid addressId)
    {
        var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);
        if (customer == null)
            return ApiResponse<bool>.FailResponse("Customer not found");

        var address = customer.Addresses.FirstOrDefault(a => a.Id == addressId);
        if (address == null)
            return ApiResponse<bool>.FailResponse("Address not found");

        customer.Addresses.Remove(address);
        _unitOfWork.Customers.Update(customer);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Address {AddressId} deleted for customer {CustomerId}", addressId, customer.Id);
        return ApiResponse<bool>.SuccessResponse(true, "Address deleted successfully");
    }
}
