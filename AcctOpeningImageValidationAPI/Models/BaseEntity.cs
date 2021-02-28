using System;
namespace AcctOpeningImageValidationAPI.Models
{
    public class BaseEntity
    {
        public int Id { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime DateModified { get; set; }

        public bool IsEnabled { get; set; }
    }
}
