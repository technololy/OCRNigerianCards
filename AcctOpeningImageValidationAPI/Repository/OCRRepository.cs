using System;
using System.Linq;
using AcctOpeningImageValidationAPI.Helpers;
using AcctOpeningImageValidationAPI.Models;
using AcctOpeningImageValidationAPI.Repository.Abstraction;
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



            return null;
        }
    }
}
