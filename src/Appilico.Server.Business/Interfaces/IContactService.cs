using Appilico.Server.Business.DTOs.Common;
using Appilico.Server.Business.DTOs.Contact;

namespace Appilico.Server.Business.Interfaces;

/// <summary>Contact service interface.</summary>
public interface IContactService
{
    Task<ApiResponse<bool>> SubmitAsync(ContactRequest request);
}
