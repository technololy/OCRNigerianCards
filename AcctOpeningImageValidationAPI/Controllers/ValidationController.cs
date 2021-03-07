using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using AcctOpeningImageValidationAPI.Helpers;
using AcctOpeningImageValidationAPI.Models;
using AcctOpeningImageValidationAPI.Repository.Abstraction;
using AcctOpeningImageValidationAPI.Repository.Services.Implementation;
using AcctOpeningImageValidationAPI.Repository.Services.Request;
using HelperLib.Exceptions;
using IdentificationValidationLib;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ReadAttributesFromFacialImage;
using static IdentificationValidationLib.Validation;
using static ModelLib.Passport;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AcctOpeningImageValidationAPI.Controllers
{
    public class ValidationController : ControllerBase
    {

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IComputerVision computerVision;
        private readonly IExternalImageValidationService externalImageValidationService;
        private readonly IFaceValidation faceValidation;
        private readonly Models.SterlingOnebankIDCardsContext context;
        private IConfiguration Configuration { get; set; }
        private readonly IOCRRepository _ocrRepository;
        private readonly RestClientService _restClientService;
        private readonly AppSettings _appSettings;

        public ValidationController(IConfiguration configuration,
            IComputerVision _computerVision,
            IExternalImageValidationService externalImageValidationService,
            ReadAttributesFromFacialImage.IFaceValidation faceValidation,
            Models.SterlingOnebankIDCardsContext _context,
            RestClientService restClientService,
            IOptions<AppSettings> option,
            IOCRRepository ocrRepository)
        {
            Configuration = configuration;
            this.computerVision = _computerVision;
            this.externalImageValidationService = externalImageValidationService;
            this.faceValidation = faceValidation;
            this.context = _context;
            _ocrRepository = ocrRepository;
            _appSettings = option.Value;
            _restClientService = restClientService;
        }

        /// <summary>
        /// This method is obsolete
        /// </summary>
        /// <param name="ImageURL"></param>
        /// <param name="camudatafield"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ValidateIdentificationImage")]
        [Obsolete]
        public async Task<IActionResult> ValidateImage([Required] string ImageURL, [FromBody] IdentificationValidationLib.Models.Camudatafield camudatafield)
        {
            // var test = Configuration.GetSection("AppSettings").GetSection("subscriptionKey").Value;
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(camudatafield);

            var bypass = Configuration.GetSection("AppSettings").GetSection("ByPass").Value;
            if (bypass.ToLower() == "true")
            {
                log.Info($"bypass set to true for {json}");
                return new OkObjectResult("success");

            }
            var response2 = await computerVision.PerformOcrWithAzureAI(ImageURL, null);
            var response = await computerVision.ReadText(ImageURL);
            if (!response.isSuccess)
            {
                return new UnprocessableEntityObjectResult(response.message);
            }

            //HelperServices helper = new HelperServices();

            var returnedExtractedTextFromImages = HelperServices.ExtractWordIntoLists(response.message);
            if (returnedExtractedTextFromImages == null || !returnedExtractedTextFromImages.Any())
            {
                //can not extract text from image. exit

                return new UnprocessableEntityObjectResult("can not see/extract any text from image submitted");

            }


            //go to extract the biodata from the images 
            Validation v = new Validation();
            v.ExtractDocKeyDetails
                (returnedExtractedTextFromImages, new IdentificationValidationLib.Models.Camudatafield());
            if (!v.IsExtractionOk)
            {
                return new UnprocessableEntityObjectResult(v.exception);
            }
            var compare = HelperServices.Compare(v, camudatafield);
            if (!compare)
            {
                return new UnprocessableEntityObjectResult($"ID Number extracted from image submitted does not match submitted ID details. The extracted ID number is {v.idNumber} and the submitted one is {camudatafield.Idno}");

            }

            var appruv = await this.externalImageValidationService.ValidateDoc(v, camudatafield);
            if (appruv.isSuccess)
            {
                // return new OkObjectResult(appruv.msg);
                return new OkObjectResult("success");

            }
            else
            {
                return new UnprocessableEntityObjectResult(appruv.msg);

            }

        }

        [HttpPost]
        [Route("ValidateIDCard")]
        public async Task<IActionResult> ValidateIDCard([FromBody] ValidateInputModel validate)
        {
            var result = await _restClientService.UploadDocument(new DocumentUploadRequest
            {
                FolderName = _appSettings.AzureContentFolderName,
                Base64String = validate.Base64Encoded,
                FileName = validate.UserName
            });

            try
            {
                _ocrRepository.ValidateUsage(result.Url);
            }
            catch (MaximumOCRUsageException e)
            {

                //TODO: Return a base response from here
                return BadRequest();
            }

            var bypass = Configuration.GetSection("AppSettings").GetSection("ByPassIdCards").Value;
            if (bypass.ToLower() == "true")
            {
                log.Info($"bypass set to true for {result.Url}");
                return new OkObjectResult("success");

            }

            var response = await computerVision.PerformOcrWithAzureAI(result.Url, null);
            //var response = await computerVision.ReadText(ImageURL);
            if (!response.isSuccess)
            {
                return new UnprocessableEntityObjectResult(response.message);
            }

            Root documentRoot = JsonSerializer.Deserialize<Root>(response.message);
            //QuickType.VotersCard.Root votersCard = new QuickType.VotersCard.Root();
            //QuickType.Passport.InternationalPassportRoot internationalPassport = new QuickType.Passport.InternationalPassportRoot();
            //QuickType.DriversLicenseRoot driversLicense = new QuickType.DriversLicenseRoot();
            //QuickType.NationalID.NationalIdRoot nationalId = new QuickType.NationalID.NationalIdRoot();
            DocumentType docType = new DocumentType();
            string firstName = ""; string middleName = ""; string lastName = ""; string idNumber = ""; DateTime dateOfBirth = new DateTime();
            (bool isSuccess, string msg) appruv;

            Models.ScannedIDCardDetails scannedIDCardDetails = new Models.ScannedIDCardDetails();

            if (documentRoot.analyzeResult.documentResults.FirstOrDefault().docType.ToLower().Contains("driverslicense"))
            {
                var driversLicense = JsonSerializer.Deserialize<QuickType.DriversLicense.DriversLicenseRoot>(response.message);
                docType = DocumentType.DriversLicense;
                string[] splitNames = HelperLib.Function.SplitDriversLicenseFullName(driversLicense.analyzeResult.documentResults.FirstOrDefault().fields.FullName.text);
                firstName = splitNames[0];
                middleName = splitNames[1];
                lastName = splitNames[2];
                var details = driversLicense.analyzeResult.documentResults.FirstOrDefault().fields;


                idNumber = driversLicense.analyzeResult.documentResults.FirstOrDefault().fields.DriversLicenseNo.text;
                dateOfBirth = Convert.ToDateTime(driversLicense.analyzeResult.documentResults.FirstOrDefault().fields.DateOfBirth.text);

                //TODO: Another Endpoint
                scannedIDCardDetails = new Models.ScannedIDCardDetails()
                {
                    Address = details.Address.text,
                    IDType = details.CardType.text,
                    DateOfBirth = dateOfBirth.ToLongDateString(),
                    BloodGroup = details.BloofGroup.text,
                    FullName = details.FullName.text,
                    Gender = details.Sex.ToString(),
                    IDNumber = idNumber,
                    IssueDate = details.DateOfIssue.text,
                    FirstName = firstName,
                    MiddleName = middleName,
                    LastName = lastName,
                    FirstIssueState = details.FirstIssueState.text,
                    ExpiryDate = details.DateOfExpiry.text,
                    FormerIDNumber = details.FullName.text,
                    NextOfKin = details.NextOfKin.text,
                    Height = details.Height.text,
                    IDClass = details.ClassOfLicense.text

                };
            }
            else if (documentRoot.analyzeResult.documentResults.FirstOrDefault().docType.ToLower().Contains("voterscard"))
            {
                var votersCard = JsonSerializer.Deserialize<QuickType.VotersCard.Root>(response.message);
                docType = DocumentType.VotersCard;
                string[] splitNames = HelperLib.Function.SplitDriversLicenseFullName(votersCard.analyzeResult.documentResults.FirstOrDefault().fields.FullName.text);
                firstName = splitNames[0];
                middleName = splitNames[1];
                lastName = splitNames[2];
                dateOfBirth = Convert.ToDateTime(votersCard.analyzeResult.documentResults.FirstOrDefault().fields.DateOfBirth.text);

                var details = votersCard.analyzeResult.documentResults.FirstOrDefault().fields;

                scannedIDCardDetails = new Models.ScannedIDCardDetails()
                {
                    Address = details.Address.text,
                    IDType = details.CardType.text,
                    DateOfBirth = dateOfBirth.ToLongDateString(),
                    Delim = details.delim.text,
                    FullName = details.FullName.text,
                    Gender = details.Sex.ToString(),
                    IDNumber = details.VoterCardNo.text,
                    Occupation = details.Occupation.text,
                    FirstName = firstName,
                    MiddleName = middleName,
                    LastName = lastName

                };
            }
            else if (documentRoot.analyzeResult.documentResults.FirstOrDefault().docType.ToLower().Contains("NationalIdNumber"))
            {
                var nationalId = JsonSerializer.Deserialize<QuickType.NationalID.Root>(response.message);
                docType = DocumentType.nationalId;
                firstName = nationalId.analyzeResult.documentResults.FirstOrDefault().fields.Firstname.text;
                lastName = nationalId.analyzeResult.documentResults.FirstOrDefault().fields.Surname.text;
                middleName = nationalId.analyzeResult.documentResults.FirstOrDefault().fields.MiddleName.text;
                dateOfBirth = Convert.ToDateTime(nationalId.analyzeResult.documentResults.FirstOrDefault().fields.DateOfBirth.text);

                var details = nationalId.analyzeResult.documentResults.FirstOrDefault().fields;

                scannedIDCardDetails = new Models.ScannedIDCardDetails()
                {

                    IDType = details.CardType.text,
                    DateOfBirth = dateOfBirth.ToLongDateString(),

                    IDNumber = idNumber,
                    IssueDate = details.IssueDate.text,
                    FirstName = firstName,
                    MiddleName = middleName,
                    LastName = lastName,

                    ExpiryDate = details.DateOfExpiry.text,


                };

            }
            else if (documentRoot.analyzeResult.documentResults.FirstOrDefault().docType.ToLower().Contains("internationalpassport"))
            {
                var internationalPassport = JsonSerializer.Deserialize<QuickType.Passport.Root>(response.message);
                docType = DocumentType.InternationalPassport;
                string[] splitNames = HelperLib.Function.SplitInternationalPassportGivenName(internationalPassport.analyzeResult.documentResults.FirstOrDefault().fields.GivenNames.text);
                firstName = splitNames[0];
                middleName = splitNames[1];
                lastName = internationalPassport.analyzeResult.documentResults.FirstOrDefault().fields.Surname.text;
                dateOfBirth = Convert.ToDateTime(internationalPassport.analyzeResult.documentResults.FirstOrDefault().fields.DateOfBirth.text);



                var details = internationalPassport.analyzeResult.documentResults.FirstOrDefault().fields;
                scannedIDCardDetails = new Models.ScannedIDCardDetails()
                {

                    IDType = details.CardType.text,
                    DateOfBirth = dateOfBirth.ToLongDateString(),

                    IDNumber = idNumber,
                    IssueDate = details.DateOfIssue.text,
                    FirstName = firstName,
                    MiddleName = middleName,
                    LastName = lastName,

                    ExpiryDate = details.DateOfExpiry.text,
                    FormerIDNumber = details.FormerPassportNo.text,
                    FullName = details.Surname.text + " " + details.GivenNames.text,
                    Address = details.Authority.text,
                    IssuingAuthority = details.Authority.text,
                    Gender = details.Sex.text,
                    Email = validate.Email



                };

            }


            //Models.ScannedIDCardDetails scannedIDCardDetails = new Models.ScannedIDCardDetails()
            //{
            //    FullName = Fullname,
            //    FirstName = firstName,
            //    LastName = lastName,
            //    MiddleName = middleName,
            //    ExpiryDate = ExpiryDate,
            //    IssueDate = IssueDate,
            //    DateOfBirth = dateOfBirth.ToLongDateString(),
            //    Address = AddressOnCard,
            //    BloodGroup = bloodGroup,
            //    Height = Height,
            //    Delim = Delim,
            //    IDNumber = idNumber,
            //    NextOfKin = nextOfKin,
            //    Occupation = occupation,
            //    IssuingAuthority = IssuingAuth,
            //    IDType = IDType,
            //    FormerIDNumber = formerID,
            //    FirstIssueState = firstIssueState,
            //    IDClass = IdClass,
            //    Gender = gender





            //};
            await context.ScannedIDCardDetail.AddAsync(scannedIDCardDetails);
            await context.SaveChangesAsync();

            appruv = await this.externalImageValidationService.ValidateDoc(firstName, middleName, lastName, idNumber, dateOfBirth, docType);
            if (appruv.isSuccess)
            {
                await context.AppruvResponses.AddAsync(new Models.AppruvResponse { StatusOfRequest = "success", Email = validate.Email });
                await context.SaveChangesAsync();
                return new OkObjectResult("success");

            }
            else
            {
                return new UnprocessableEntityObjectResult(appruv.msg);

            }
        }

        [HttpPost]
        [Route("ValidateNigerianIDCards")]
        public async Task<IActionResult> ValidateNigerianIDCards([Required] string ImageURL, [Required] string UserEmail)
        {
            // var test = Configuration.GetSection("AppSettings").GetSection("subscriptionKey").Value;

            //TODO: 1 Call Core Banking Content Server, Content Server return absolute url path
            //TODO: 2 Returned Scanned Document Object
            //TODO: 3

            try
            {
                _ocrRepository.ValidateUsage(UserEmail);

            } catch (MaximumOCRUsageException e) {

                //TODO: Return a base response from here
            }

            var bypass = Configuration.GetSection("AppSettings").GetSection("ByPassIdCards").Value;
            if (bypass.ToLower() == "true")
            {
                log.Info($"bypass set to true for {ImageURL}");
                return new OkObjectResult("success");

            }

            var response = await computerVision.PerformOcrWithAzureAI(ImageURL, null);
            //var response = await computerVision.ReadText(ImageURL);
            if (!response.isSuccess)
            {
                return new UnprocessableEntityObjectResult(response.message);
            }

            Root documentRoot = JsonSerializer.Deserialize<Root>(response.message);
            //QuickType.VotersCard.Root votersCard = new QuickType.VotersCard.Root();
            //QuickType.Passport.InternationalPassportRoot internationalPassport = new QuickType.Passport.InternationalPassportRoot();
            //QuickType.DriversLicenseRoot driversLicense = new QuickType.DriversLicenseRoot();
            //QuickType.NationalID.NationalIdRoot nationalId = new QuickType.NationalID.NationalIdRoot();
            DocumentType docType = new DocumentType();
            string firstName = ""; string middleName = ""; string lastName = ""; string idNumber = ""; DateTime dateOfBirth = new DateTime();
            (bool isSuccess, string msg) appruv;

            Models.ScannedIDCardDetails scannedIDCardDetails = new Models.ScannedIDCardDetails();

            if (documentRoot.analyzeResult.documentResults.FirstOrDefault().docType.ToLower().Contains("driverslicense"))
            {
                var driversLicense = JsonSerializer.Deserialize<QuickType.DriversLicense.DriversLicenseRoot>(response.message);
                docType = DocumentType.DriversLicense;
                string[] splitNames = HelperLib.Function.SplitDriversLicenseFullName(driversLicense.analyzeResult.documentResults.FirstOrDefault().fields.FullName.text);
                firstName = splitNames[0];
                middleName = splitNames[1];
                lastName = splitNames[2];
                var details = driversLicense.analyzeResult.documentResults.FirstOrDefault().fields;


                idNumber = driversLicense.analyzeResult.documentResults.FirstOrDefault().fields.DriversLicenseNo.text;
                dateOfBirth = Convert.ToDateTime(driversLicense.analyzeResult.documentResults.FirstOrDefault().fields.DateOfBirth.text);

                //TODO: Another Endpoint
                scannedIDCardDetails = new Models.ScannedIDCardDetails()
                {
                    Address = details.Address.text,
                    IDType = details.CardType.text,
                    DateOfBirth = dateOfBirth.ToLongDateString(),
                    BloodGroup = details.BloofGroup.text,
                    FullName = details.FullName.text,
                    Gender = details.Sex.ToString(),
                    IDNumber = idNumber,
                    IssueDate = details.DateOfIssue.text,
                    FirstName = firstName,
                    MiddleName = middleName,
                    LastName = lastName,
                    FirstIssueState = details.FirstIssueState.text,
                    ExpiryDate = details.DateOfExpiry.text,
                    FormerIDNumber = details.FullName.text,
                    NextOfKin = details.NextOfKin.text,
                    Height = details.Height.text,
                    IDClass = details.ClassOfLicense.text

                };
            }
            else if (documentRoot.analyzeResult.documentResults.FirstOrDefault().docType.ToLower().Contains("voterscard"))
            {
                var votersCard = JsonSerializer.Deserialize<QuickType.VotersCard.Root>(response.message);
                docType = DocumentType.VotersCard;
                string[] splitNames = HelperLib.Function.SplitDriversLicenseFullName(votersCard.analyzeResult.documentResults.FirstOrDefault().fields.FullName.text);
                firstName = splitNames[0];
                middleName = splitNames[1];
                lastName = splitNames[2];
                dateOfBirth = Convert.ToDateTime(votersCard.analyzeResult.documentResults.FirstOrDefault().fields.DateOfBirth.text);

                var details = votersCard.analyzeResult.documentResults.FirstOrDefault().fields;

                scannedIDCardDetails = new Models.ScannedIDCardDetails()
                {
                    Address = details.Address.text,
                    IDType = details.CardType.text,
                    DateOfBirth = dateOfBirth.ToLongDateString(),
                    Delim = details.delim.text,
                    FullName = details.FullName.text,
                    Gender = details.Sex.ToString(),
                    IDNumber = details.VoterCardNo.text,
                    Occupation = details.Occupation.text,
                    FirstName = firstName,
                    MiddleName = middleName,
                    LastName = lastName

                };
            }
            else if (documentRoot.analyzeResult.documentResults.FirstOrDefault().docType.ToLower().Contains("NationalIdNumber"))
            {
                var nationalId = JsonSerializer.Deserialize<QuickType.NationalID.Root>(response.message);
                docType = DocumentType.nationalId;
                firstName = nationalId.analyzeResult.documentResults.FirstOrDefault().fields.Firstname.text;
                lastName = nationalId.analyzeResult.documentResults.FirstOrDefault().fields.Surname.text;
                middleName = nationalId.analyzeResult.documentResults.FirstOrDefault().fields.MiddleName.text;
                dateOfBirth = Convert.ToDateTime(nationalId.analyzeResult.documentResults.FirstOrDefault().fields.DateOfBirth.text);

                var details = nationalId.analyzeResult.documentResults.FirstOrDefault().fields;

                scannedIDCardDetails = new Models.ScannedIDCardDetails()
                {

                    IDType = details.CardType.text,
                    DateOfBirth = dateOfBirth.ToLongDateString(),

                    IDNumber = idNumber,
                    IssueDate = details.IssueDate.text,
                    FirstName = firstName,
                    MiddleName = middleName,
                    LastName = lastName,

                    ExpiryDate = details.DateOfExpiry.text,


                };

            }
            else if (documentRoot.analyzeResult.documentResults.FirstOrDefault().docType.ToLower().Contains("internationalpassport"))
            {
                var internationalPassport = JsonSerializer.Deserialize<QuickType.Passport.Root>(response.message);
                docType = DocumentType.InternationalPassport;
                string[] splitNames = HelperLib.Function.SplitInternationalPassportGivenName(internationalPassport.analyzeResult.documentResults.FirstOrDefault().fields.GivenNames.text);
                firstName = splitNames[0];
                middleName = splitNames[1];
                lastName = internationalPassport.analyzeResult.documentResults.FirstOrDefault().fields.Surname.text;
                dateOfBirth = Convert.ToDateTime(internationalPassport.analyzeResult.documentResults.FirstOrDefault().fields.DateOfBirth.text);



                var details = internationalPassport.analyzeResult.documentResults.FirstOrDefault().fields;
                scannedIDCardDetails = new Models.ScannedIDCardDetails()
                {

                    IDType = details.CardType.text,
                    DateOfBirth = dateOfBirth.ToLongDateString(),

                    IDNumber = idNumber,
                    IssueDate = details.DateOfIssue.text,
                    FirstName = firstName,
                    MiddleName = middleName,
                    LastName = lastName,

                    ExpiryDate = details.DateOfExpiry.text,
                    FormerIDNumber = details.FormerPassportNo.text,
                    FullName = details.Surname.text + " " + details.GivenNames.text,
                    Address = details.Authority.text,
                    IssuingAuthority = details.Authority.text,
                    Gender = details.Sex.text,
                    Email = UserEmail



                };

            }


            //Models.ScannedIDCardDetails scannedIDCardDetails = new Models.ScannedIDCardDetails()
            //{
            //    FullName = Fullname,
            //    FirstName = firstName,
            //    LastName = lastName,
            //    MiddleName = middleName,
            //    ExpiryDate = ExpiryDate,
            //    IssueDate = IssueDate,
            //    DateOfBirth = dateOfBirth.ToLongDateString(),
            //    Address = AddressOnCard,
            //    BloodGroup = bloodGroup,
            //    Height = Height,
            //    Delim = Delim,
            //    IDNumber = idNumber,
            //    NextOfKin = nextOfKin,
            //    Occupation = occupation,
            //    IssuingAuthority = IssuingAuth,
            //    IDType = IDType,
            //    FormerIDNumber = formerID,
            //    FirstIssueState = firstIssueState,
            //    IDClass = IdClass,
            //    Gender = gender





            //};
            await context.ScannedIDCardDetail.AddAsync(scannedIDCardDetails);
            await context.SaveChangesAsync();

            appruv = await this.externalImageValidationService.ValidateDoc(firstName, middleName, lastName, idNumber, dateOfBirth, docType);
            if (appruv.isSuccess)
            {
                await context.AppruvResponses.AddAsync(new Models.AppruvResponse { StatusOfRequest = "success", Email = UserEmail });
                await context.SaveChangesAsync();
                return new OkObjectResult("success");

            }
            else
            {
                return new UnprocessableEntityObjectResult(appruv.msg);

            }
        }

        [HttpGet]
        [Route("ValidateFaceImage")]

        public async Task<IActionResult> ValidateFaceImage([Required] string ImageURL)
        {


            var bypass = Configuration.GetSection("AppSettings").GetSection("ByPassFacial").Value;
            if (bypass.ToLower() == "true")
            {
                log.Info($"bypass set to true for {ImageURL}");
                return new OkObjectResult("success");

            }

            try
            {
                var result = await faceValidation.PerformFaceValidationAsync(ImageURL);
                log.Info($"source:{ImageURL}, result:{result.faces}");
                if (result.IsSuccess)
                {
                    var faceAttributes = result.faces.FirstOrDefault().FaceAttributes;
                    Models.FacialValidation facialValidation = new Models.FacialValidation()
                    {
                        FacialHair = faceAttributes.FacialHair.ToString(),
                        Hair = faceAttributes.Hair.ToString(),
                        Accessories = faceAttributes.Accessories.FirstOrDefault().ToString(),
                        Age = faceAttributes.Age.ToString(),
                        Gender = faceAttributes.Gender.Value.ToString(),
                        Emotion = faceAttributes.Emotion.ToString(),
                        Smile = faceAttributes.Smile.Value.ToString(),
                        Occlusion = faceAttributes.Occlusion.ToString(),
                    };
                    await context.FacialValidations.AddAsync(facialValidation);
                    await context.SaveChangesAsync();
                    //return new OkObjectResult(result.faces);
                    return new OkObjectResult("success");

                }
                else
                {
                    return new UnprocessableEntityObjectResult(result.faces);

                }
            }
            catch (Exception ex)
            {
                log.Error($"source:{ImageURL}, error {ex}");
                return new BadRequestObjectResult(ex);
            }
        }





        [HttpGet]
        [Route("QaWork")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> QaWork([Required] string identifier)
        {


            var connString = Configuration.GetConnectionString("OneBankConn");
            connString = "";
            connString = "Server=localhost;Initial Catalog=KMNDB; Integrated Security=false;user id=sa;password=reallyStrongPwd123";

            try
            {
                SqlDataClientLib.Class1 c = new SqlDataClientLib.Class1();
                var code = c.ReturnSingle(connString, "SELECT [Code] FROM [KMNDB].[dbo].[OTP_table] where id=1", "");
                if (!string.IsNullOrEmpty(code))
                {
                    var query = $"update [KMNDB].[dbo].[OTP_table] set userid=2 where code={code}";
                    var num = c.ExecuteDbAction(connString, query, "");
                    //return new OkObjectResult(result.faces);
                    if (num > 0)
                    {
                        return new OkObjectResult("success");

                    }
                    else
                    {
                        return new UnprocessableEntityObjectResult("");

                    }

                }
                else
                {
                    return new UnprocessableEntityObjectResult("");

                }
            }
            catch (Exception ex)
            {
                log.Error($"source:{identifier}, error {ex}");
                return new BadRequestObjectResult(ex);
            }
        }

        [HttpGet]
        [Route("NewToWorkWork")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> NewToWorkWork(string identifier, string name)
        {


            var connString = Configuration.GetConnectionString("OneBankConn");
            //connString = "";
            connString = "Server=10.0.41.101;Initial Catalog=SterlingMobile;Integrated Security=False;User ID=sa;Password=tylent;MultipleActiveResultSets=True;pooling=true;Max Pool Size=200;";

            try
            {
                SqlDataClientLib.Class1 c = new SqlDataClientLib.Class1();
                var getuserid = $"select userid FROM [SterlingMobile].[dbo].[User] where UserName = @username";
                var id = c.ReturnSingle(connString, getuserid, name);
                if (string.IsNullOrEmpty(id))
                {

                    return new UnprocessableEntityObjectResult($"cant get id because {c.errorMessage}");
                }


                var query = $"delete  from [SterlingMobile].[dbo].userdeviceinfo where UserId =@param;";
                var num = c.ExecuteDbAction(connString, query, id);

                var query2 = $"delete  from [SterlingMobile].[dbo].Beneficiary where UserId =@param;";
                var num2 = c.ExecuteDbAction(connString, query2, id);

                var query3 = $"delete  from [SterlingMobile].[dbo].[User] where username =@param;";
                var num3 = c.ExecuteDbAction(connString, query3, name);
                //return new OkObjectResult(result.faces);
                if (num > 0 && num3 > 0)
                {
                    return new OkObjectResult("success");

                }
                else
                {
                    return new UnprocessableEntityObjectResult("cant delete");

                }




            }
            catch (Exception ex)
            {
                log.Error($"source:{identifier}, error {ex}");
                return new BadRequestObjectResult(ex);
            }
        }




    }
}
