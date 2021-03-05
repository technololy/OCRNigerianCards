using System;
namespace AcctOpeningImageValidationAPI.Models
{
    public class ValidateInputModel
    {
        public string Email { get; set; }

        public string Base64Encoded { get; set; }

        public string UserName { get; set; }
    }
}
