using System;
using System.Collections.Generic;
using AcctOpeningImageValidationAPI.Models;

namespace AcctOpeningImageValidationAPI.Repository.Abstraction
{
    public interface IOCRRepository
    {
        /// <summary>
        /// Abstract Method for OCR Validation
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
       OCRUsage ValidateUsage (string email);
    }
}
