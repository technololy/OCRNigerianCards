using System;
using Newtonsoft.Json;

namespace AcctOpeningImageValidationAPI.Repository.Services.Request
{
    public class DocumentUploadRequest
    {
        [JsonProperty(PropertyName = "folderName")]
        public string FolderName { get; set; }

        [JsonProperty(PropertyName = "fileName")]
        public string FileName { get; set; }

        [JsonProperty(PropertyName = "base64String")]
        public string Base64String { get; set; }
    }
}
