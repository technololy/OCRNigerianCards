﻿using System;
namespace AcctOpeningImageValidationAPI.Helpers
{
    public class AppSettings
    {
        public int MaximumUsageForOCR { get; set; }

        public string ApiDocumentBaseUrl { get; set; }

        public string AzureContentFolderName { get; set; }

        public string subscriptionKey { get; set; }

        public string AzureFacialBaseUrl { get; set; }
    }
}
