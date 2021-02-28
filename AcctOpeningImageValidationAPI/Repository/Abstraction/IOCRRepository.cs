using System;
using System.Collections.Generic;
using AcctOpeningImageValidationAPI.Models;

namespace AcctOpeningImageValidationAPI.Repository.Abstraction
{
    public interface IOCRRepository
    {
       OCRUsage ValidateUsage (string email);
    }
}
