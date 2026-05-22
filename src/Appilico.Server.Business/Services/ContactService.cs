using Microsoft.Extensions.Logging;
using Appilico.Server.Business.DTOs.Common;
using Appilico.Server.Business.DTOs.Contact;
using Appilico.Server.Business.Interfaces;
using Appilico.Server.Domain.Entities;
using Appilico.Server.Domain.Interfaces;

namespace Appilico.Server.Business.Services;

/// <summary>Contact service implementation.</summary>
public class ContactService : IContactService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailWorkQueue _emailWorkQueue;
    private readonly ILogger<ContactService> _logger;

    public ContactService(IUnitOfWork unitOfWork, IEmailWorkQueue emailWorkQueue, ILogger<ContactService> logger)
    {
        _unitOfWork = unitOfWork;
        _emailWorkQueue = emailWorkQueue;
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

        await _unitOfWork.ContactMessages.AddAsync(message);
        await _unitOfWork.SaveChangesAsync();

        await _emailWorkQueue.QueueAsync((emailService, _) => emailService.SendContactNotificationAsync(message));
        await _emailWorkQueue.QueueAsync((emailService, _) => emailService.SendContactAutoReplyAsync(message.Email, message.Name));

        _logger.LogInformation("Contact message received from {Email}", request.Email);
        return ApiResponse<bool>.SuccessResponse(true, "Message received. We'll be in touch shortly.");
    }
}
