using System;
using Newtonsoft.Json;

namespace AcctOpeningImageValidationAPI.Repository.Services.Response
{
    public class DocumentUploadResponse
    {
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName =  "responseCode")]
        public string ResponseCode { get; set; }

        [JsonProperty(PropertyName = "responseDescription")]
        public string ResponseDescription { get; set; }
    }
}
