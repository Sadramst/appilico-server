using AppilicoShopServer.Business.DTOs.Common;
using AppilicoShopServer.Business.DTOs.Contact;

namespace AppilicoShopServer.Business.Interfaces;

/// <summary>Contact service interface.</summary>
public interface IContactService
{
    Task<ApiResponse<bool>> SubmitAsync(ContactRequest request);
}
