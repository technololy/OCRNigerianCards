using System;
using System.Net.Http;
using System.Threading.Tasks;
using AcctOpeningImageValidationAPI.Helpers;
using AcctOpeningImageValidationAPI.Repository.Services.Request;
using AcctOpeningImageValidationAPI.Repository.Services.Response;
using Microsoft.Extensions.Options;
using Refit;

namespace AcctOpeningImageValidationAPI.Repository.Services.Implementation
{
    public class RestClientService
    {
        private readonly IRestClientService _restClientService;
        private AppSettings _setting;

        public RestClientService(IOptions<AppSettings> options)
        {
            _setting = options.Value;

            var httpClient = new HttpClient
            {
               BaseAddress = new Uri(_setting.ApiDocumentBaseUrl)
              
            };

            _restClientService = RestService.For<IRestClientService>(httpClient);
        }

        public async Task<DocumentUploadResponse> UploadDocument(DocumentUploadRequest document)
        {
            try
            {
                var result = await _restClientService.UplodDocument(document).ConfigureAwait(true);

                return result;

            }
            catch (ApiException e)
            {
                return new DocumentUploadResponse();

            }
            catch (Exception e)
            {
                return new DocumentUploadResponse();
            }
        }
    }
}
