using System;
using System.Linq;
using AcctOpeningImageValidationAPI.Helpers;
using AcctOpeningImageValidationAPI.Models;
using AcctOpeningImageValidationAPI.Repository.Abstraction;
using HelperLib.Exceptions;
using Microsoft.Extensions.Options;

namespace AcctOpeningImageValidationAPI.Repository
{
    public class OCRRepository : BaseRepository<OCRUsage>, IOCRRepository
    {
        protected readonly AppSettings _settings;

        public OCRRepository(SterlingOnebankIDCardsContext context, IOptions<AppSettings> options) : base(context)
        {
            _settings = options.Value;
        }

        public OCRUsage ValidateUsage(string email)
        {
            var usage = GetAll().Where(x => x.EmailAddress == email);

            if(usage.Count() >= _settings.MaximumUsageForOCR)
            {
                throw new MaximumOCRUsageException("You've reached maximum usage for this service, please try again!");
            }

            if(usage.Count() < _settings.MaximumUsageForOCR)
            {

            }

            CreateAndReturn(new OCRUsage
            {

            });

            return null;
        }
    }
}
