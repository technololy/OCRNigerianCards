using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using static ModelLib.Passport;

namespace IdentificationValidationLib
{
    public interface IComputerVision
    {
        Task<(bool isSuccess, string message)> ReadText(string imageFilePath, Models.Camudatafield camudatafield = null);

        Task<(bool isSuccess, string message)> PerformOcrWithAzureAI(string imageFilePath, Models.Camudatafield camudatafield = null);
    }


    public class ComputerVision : IComputerVision
    {
        static HttpClient client;
        private IConfiguration Configuration { get; set; }

        public ComputerVision(IConfiguration _configuration)
        {
            client = new HttpClient();

            //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Configuration = _configuration;
        }


        /// Gets the text from the specified image file by using
        /// the Computer Vision REST API.
        /// </summary>
        /// <param name="imageFilePath">The image file with text.</param>
        public async Task<(bool isSuccess, string message)> ReadText(string imageFilePath, Models.Camudatafield camudatafield = null)
        {
            try
            {
                string subscriptionKey = Configuration.GetSection("AppSettings").GetSection("subscriptionKey").Value;

                //HttpClient client = new HttpClient();
                bool isSuccess = false;
                string responseMessage = "";
                // Request headers.
                client.DefaultRequestHeaders.Add(
                    "Ocp-Apim-Subscription-Key", subscriptionKey);
                string endpoint = Configuration.GetSection("AppSettings").GetSection("AzureFacialBaseUrl").Value;
                string uriBase = endpoint + Configuration.GetSection("AppSettings").GetSection("AzureFacialFaceEndPoint").Value;
                string url = uriBase;

                HttpResponseMessage response;

                // Two REST API methods are required to extract text.
                // One method to submit the image for processing, the other method
                // to retrieve the text found in the image.

                // operationLocation stores the URI of the second REST API method,
                // returned by the first REST API method.
                string operationLocation;

                // Reads the contents of the specified local image
                // into a byte array.
                byte[] byteData = GetImageURLAsByteArray(imageFilePath);

                if (byteData?.Length <= 0)
                {
                    responseMessage = "Invalid Image. Byte length is zero";
                    return (isSuccess, responseMessage);
                }

                // Adds the byte array as an octet stream to the request body.
                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    // This example uses the "application/octet-stream" content type.
                    // The other content types you can use are "application/json"
                    // and "multipart/form-data".
                    content.Headers.ContentType =
                        new MediaTypeHeaderValue("application/octet-stream");

                    // The first REST API method, Batch Read, starts
                    // the async process to analyze the written text in the image.
                    response = await client.PostAsync(url, content);
                }

                // The response header for the Batch Read method contains the URI
                // of the second method, Read Operation Result, which
                // returns the results of the process in the response body.
                // The Batch Read operation does not return anything in the response body.
                if (response.IsSuccessStatusCode)
                    operationLocation =
                        response.Headers.GetValues("Operation-Location").FirstOrDefault();
                else
                {
                    // Display the JSON error data.
                    string errorString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("\n\nResponse:\n{0}\n",
                       Newtonsoft.Json.Linq.JToken.Parse(errorString).ToString());
                    return (isSuccess = false, errorString);
                }

                // If the first REST API method completes successfully, the second 
                // REST API method retrieves the text written in the image.
                //
                // Note: The response may not be immediately available. Text
                // recognition is an asynchronous operation that can take a variable
                // amount of time depending on the length of the text.
                // You may need to wait or retry this operation.
                //
                // This example checks once per second for ten seconds.
                string contentString;
                int i = 0;
                do
                {
                    System.Threading.Thread.Sleep(1000);
                    response = await client.GetAsync(operationLocation);
                    contentString = await response.Content.ReadAsStringAsync();
                    ++i;
                }
                while (i < 60 && contentString.IndexOf("\"status\":\"succeeded\"") == -1);

                if (i == 60 && contentString.IndexOf("\"status\":\"succeeded\"") == -1)
                {
                    Console.WriteLine("\nTimeout error.\n");
                    return (false, responseMessage = "Timeout error");
                }

                // Display the JSON response.
                Console.WriteLine("\nResponse:\n\n{0}\n",
                    JToken.Parse(contentString).ToString());
                //extract the whole text into a list of strings
                //from this list, know if its passport, drivers licsnse or voters card
                return (true, contentString);


            }
            catch (Exception exc)
            {
                Console.WriteLine("\n" + exc.InnerException);
                return (false, exc.InnerException.ToString());

            }


        }
        public static byte[] GetImageURLAsByteArray(string imageFilePath)
        {


            //static string imageFilePath = @"damilola_oyebanji_international_passport.jpg";
            //            string imageFilePath2 = "voters_card_sterling_customer.png";
            //            imageFilePath2 = @"lolades_drivers_license.jpg";
            //            //static string imageFilePath = @"Identification.jpg";
            //#if DEBUG
            //            var img2 = System.IO.File.ReadAllBytesAsync(imageFilePath2);
            //            var img = System.IO.File.ReadAllBytes(imageFilePath2);
            //            return img;
            //#endif

            var response = client.GetAsync(imageFilePath).Result;
            var bytes = response.Content.ReadAsByteArrayAsync().Result;
            return bytes;
        }

        public async Task<(bool isSuccess, string message)> PerformOcrWithAzureAI(string imageFilePath, Models.Camudatafield camudatafield = null)
        {

            bool isSuccess = false;
            string responseMessage = "";

            var blobTokenResponse = await HelperLib.Function.GetBlobStorageBearerToken();
            if (!blobTokenResponse.isSuccess)
            {
                isSuccess = false;
                responseMessage = "";
                return (isSuccess, responseMessage);
            }

            var imageStream = await HelperLib.Function.GetImageStream(blobTokenResponse.message, imageFilePath);

            if (!imageStream.isSuccess)
            {
                isSuccess = false;
                responseMessage = "can not get image after using beaere token";
                return (isSuccess, responseMessage);
            }


            string subscriptionKey = Configuration.GetSection("AppSettings").GetSection("formRecognizerSubscriptionKey").Value;

            string modelID = Configuration.GetSection("AppSettings").GetSection("modelID").Value;

            //HttpClient client = new HttpClient();
            // Request headers.
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
            string endpoint = Configuration.GetSection("AppSettings").GetSection("formRecognizerBaseUrl").Value;
            string v21 = Configuration.GetSection("AppSettings").GetSection("previewVersion").Value;


            string url = $"{endpoint}formrecognizer/{v21}/custom/models/{modelID}/analyze"; // + queryString;
            Debug.WriteLine($"form recognizer is {url}");

            var content = new ByteArrayContent(imageStream.message);
            if (imageFilePath.ToLower().EndsWith(".jpg"))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("image/jpg");
            }
            else if (imageFilePath.ToLower().EndsWith(".png"))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            }
            else if (imageFilePath.ToLower().EndsWith(".pdf"))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            }
            else
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            var response = await client.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                return (isSuccess, responseMessage);

            }

            System.Threading.Thread.Sleep(5000);
            var jsonString = response.Headers.GetValues("Operation-Location").ToArray();
            var client2 = new HttpClient();
            client2.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            var uri_2 = jsonString[0].ToString();
            var response_2 = await client2.GetAsync(uri_2);
            if (!response_2.IsSuccessStatusCode)
            {
                return (isSuccess, responseMessage);

            }


            responseMessage = await response_2.Content.ReadAsStringAsync();
            isSuccess = true;
            // rTxtBoxForm.Text = JsonConvert.SerializeObject(replyText);  use this to create a json representation of string response



            return (isSuccess, responseMessage);




        }
    }
}
