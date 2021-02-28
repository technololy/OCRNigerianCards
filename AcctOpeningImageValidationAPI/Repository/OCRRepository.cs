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
        /// <summary>
        /// Injecting AppSettings to Plain Object CRL
        /// Which can help preload the AppSetting from appsetting.json
        /// </summary>
        protected readonly AppSettings _settings;

        /// <summary>
        /// Constructor injecting two properties using DI
        /// AppDbContext
        /// AppSettings
        /// </summary>
        /// <param name="context"></param>
        /// <param name="options"></param>
        public OCRRepository(SterlingOnebankIDCardsContext context, IOptions<AppSettings> options) : base(context)
        {
            _settings = options.Value;
        }

        /// <summary>
        /// OCR Validation Method
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public OCRUsage ValidateUsage(string email)
        {
            var usage = GetAll().Where(x => x.EmailAddress == email);

            if(usage == null)
            {
                var entity = new OCRUsage { Count = 1, EmailAddress = email };

                CreateAndReturn(entity);

                return entity;

            } else {

                if (usage.Count() >= _settings.MaximumUsageForOCR)
                {
                    throw new MaximumOCRUsageException("You've reached maximum usage for this service, please try again!");
                }

                if (usage.Count() <= _settings.MaximumUsageForOCR)
                {
                    var entity = GetAll().FirstOrDefault(x => x.EmailAddress == email);

                    entity.Count += 1;

                    Update(entity);

                    return entity;
                }

                throw new MaximumOCRUsageException("You've reached maximum usage for this service, please try again!");
            }
        }
    }
}
