using System;
namespace AcctOpeningImageValidationAPI.Models
{
    public class OCRUsage : BaseEntity
    {
        public int Count { get; set; }

        public string EmailAddress { get; set; }
    }
}
