using AutoMapper;
using Microsoft.Extensions.Logging;
using Appilico.Server.Business.DTOs.Common;
using Appilico.Server.Business.DTOs.Customer;
using Appilico.Server.Business.Interfaces;
using Appilico.Server.Domain.Interfaces;

namespace Appilico.Server.Business.Services;

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
    public async Task<ApiResponse<CustomerDto>> UpdateAsync(Guid id, UpdateCustomerRequest request, string userId)
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

        if (request.MembershipTier.HasValue)
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
}
