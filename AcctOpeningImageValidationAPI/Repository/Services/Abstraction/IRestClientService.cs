using System;
using System.Threading.Tasks;
using AcctOpeningImageValidationAPI.Repository.Services.Request;
using AcctOpeningImageValidationAPI.Repository.Services.Response;
using Refit;

namespace AcctOpeningImageValidationAPI.Repository.Services
{
    public interface IRestClientService
    {
        [Post("/User/UploadImageToAzureWithResponseCode")]
        [Headers("ApiKey : Test2020QuafgLo@!!WB",
                 "Content-Type : application/json",
                 "Accept : application/json")]
        Task<DocumentUploadResponse> UplodDocument([Body] DocumentUploadRequest document);
    }
}
