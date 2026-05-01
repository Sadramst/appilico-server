using Microsoft.Extensions.Logging;
using Appilico.Server.Business.DTOs.Common;
using Appilico.Server.Business.DTOs.Contact;
using Appilico.Server.Business.Interfaces;
using Appilico.Server.DataAccess.Data;
using Appilico.Server.Domain.Entities;

namespace Appilico.Server.Business.Services;

/// <summary>Contact service implementation.</summary>
public class ContactService : IContactService
{
    private readonly AppDbContext _db;
    private readonly ILogger<ContactService> _logger;

    public ContactService(AppDbContext db, ILogger<ContactService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<bool>> SubmitAsync(ContactRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Message))
            return ApiResponse<bool>.FailResponse("Name, email, and message are required");

        var message = new ContactMessage
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            Company = request.Company?.Trim(),
            Subject = request.Subject?.Trim(),
            Message = request.Message.Trim()
        };

        _db.ContactMessages.Add(message);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Contact message received from {Email}", request.Email);
        return ApiResponse<bool>.SuccessResponse(true, "Message received. We'll be in touch shortly.");
    }
}
