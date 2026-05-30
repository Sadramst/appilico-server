using AutoMapper;
using Microsoft.Extensions.Logging;
using AppilicoShopServer.Business.DTOs.Common;
using AppilicoShopServer.Business.DTOs.Inventory;
using AppilicoShopServer.Business.Interfaces;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Interfaces;

namespace AppilicoShopServer.Business.Services;

/// <summary>Inventory service implementation.</summary>
public class InventoryService : IInventoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<InventoryService> _logger;

    /// <summary>Initializes a new instance of InventoryService.</summary>
    public InventoryService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<InventoryService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<List<InventoryTransactionDto>>> GetTransactionsAsync(Guid productId, int page = 1, int pageSize = 20)
    {
        var (items, totalCount) = await _unitOfWork.Inventory.GetPagedAsync(page, pageSize,
            predicate: t => t.ProductId == productId,
            orderBy: q => q.OrderByDescending(t => t.CreatedAt));

        var dtos = _mapper.Map<List<InventoryTransactionDto>>(items);
        var pagination = PaginationMeta.Create(page, pageSize, totalCount);

        return ApiResponse<List<InventoryTransactionDto>>.SuccessResponse(dtos, pagination);
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<InventoryTransactionDto>> AdjustAsync(AdjustInventoryRequest request, string userId)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId);
        if (product == null)
            return ApiResponse<InventoryTransactionDto>.FailResponse("Product not found");

        var transaction = new InventoryTransaction
        {
            ProductId = request.ProductId,
            VariantId = request.VariantId,
            TransactionType = request.TransactionType,
            Quantity = request.Quantity,
            Reference = request.Reference,
            Notes = request.Notes,
            CreatedBy = userId
        };

        // Adjust stock based on transaction type
        if (request.TransactionType == Domain.Enums.InventoryTransactionType.StockIn ||
            request.TransactionType == Domain.Enums.InventoryTransactionType.Return)
        {
            product.StockQuantity += request.Quantity;
        }
        else
        {
            if (product.StockQuantity < request.Quantity)
                return ApiResponse<InventoryTransactionDto>.FailResponse("Insufficient stock");

            product.StockQuantity -= request.Quantity;
        }

        await _unitOfWork.Inventory.AddAsync(transaction);
        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Inventory adjusted for product {ProductId}: {Type} {Quantity}", request.ProductId, request.TransactionType, request.Quantity);
        return ApiResponse<InventoryTransactionDto>.SuccessResponse(_mapper.Map<InventoryTransactionDto>(transaction), "Inventory adjusted successfully");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<List<LowStockProductDto>>> GetLowStockAsync(int threshold = 10)
    {
        var products = await _unitOfWork.Inventory.GetLowStockProductsAsync();
        return ApiResponse<List<LowStockProductDto>>.SuccessResponse(_mapper.Map<List<LowStockProductDto>>(products));
    }
}
