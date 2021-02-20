﻿// Generated by https://quicktype.io

using System;
using System.Collections.Generic;

namespace QuickType.Passport
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class ReadResult
    {
        public int page { get; set; }
        public int angle { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public string unit { get; set; }
    }

    public class PageResult
    {
        public int page { get; set; }
        public List<object> tables { get; set; }
    }

    public class DateOfIssue
    {
        public string type { get; set; }
        public string valueString { get; set; }
        public string text { get; set; }
        public int page { get; set; }
        public List<double> boundingBox { get; set; }
        public double confidence { get; set; }
    }

    public class FormerPassportNo
    {
        public string type { get; set; }
        public string valueString { get; set; }
        public string text { get; set; }
        public int page { get; set; }
        public List<double> boundingBox { get; set; }
        public double confidence { get; set; }
    }

    public class Sex
    {
        public string type { get; set; }
        public string valueString { get; set; }
        public string text { get; set; }
        public int page { get; set; }
        public List<double> boundingBox { get; set; }
        public double confidence { get; set; }
    }

    public class PlaceOfBirth
    {
        public string type { get; set; }
        public string valueString { get; set; }
        public string text { get; set; }
        public int page { get; set; }
        public List<double> boundingBox { get; set; }
        public double confidence { get; set; }
    }

    public class Surname
    {
        public string type { get; set; }
        public string valueString { get; set; }
        public string text { get; set; }
        public int page { get; set; }
        public List<double> boundingBox { get; set; }
        public double confidence { get; set; }
    }

    public class DateOfBirth
    {
        public string type { get; set; }
        public string valueString { get; set; }
        public string text { get; set; }
        public int page { get; set; }
        public List<double> boundingBox { get; set; }
        public double confidence { get; set; }
    }

    public class Nationality
    {
        public string type { get; set; }
        public string valueString { get; set; }
        public string text { get; set; }
        public int page { get; set; }
        public List<double> boundingBox { get; set; }
        public double confidence { get; set; }
    }

    public class CardType
    {
        public string type { get; set; }
        public string valueString { get; set; }
        public string text { get; set; }
        public int page { get; set; }
        public List<double> boundingBox { get; set; }
        public double confidence { get; set; }
    }

    public class GivenNames
    {
        public string type { get; set; }
        public string valueString { get; set; }
        public string text { get; set; }
        public int page { get; set; }
        public List<double> boundingBox { get; set; }
        public double confidence { get; set; }
    }

    public class PassPortNumber
    {
        public string type { get; set; }
        public string valueString { get; set; }
        public string text { get; set; }
        public int page { get; set; }
        public List<double> boundingBox { get; set; }
        public double confidence { get; set; }
    }

    public class Type
    {
        public string type { get; set; }
        public string valueString { get; set; }
        public string text { get; set; }
        public int page { get; set; }
        public List<double> boundingBox { get; set; }
        public double confidence { get; set; }
    }

    public class DateOfExpiry
    {
        public string type { get; set; }
        public string valueString { get; set; }
        public string text { get; set; }
        public int page { get; set; }
        public List<double> boundingBox { get; set; }
        public double confidence { get; set; }
    }

    public class CountryCode
    {
        public string type { get; set; }
        public string valueString { get; set; }
        public string text { get; set; }
        public int page { get; set; }
        public List<double> boundingBox { get; set; }
        public double confidence { get; set; }
    }

    public class Authority
    {
        public string type { get; set; }
        public string valueString { get; set; }
        public string text { get; set; }
        public int page { get; set; }
        public List<double> boundingBox { get; set; }
        public double confidence { get; set; }
    }

    public class Fields
    {
        public DateOfIssue DateOfIssue { get; set; }
        public FormerPassportNo FormerPassportNo { get; set; }
        public Sex Sex { get; set; }
        public PlaceOfBirth PlaceOfBirth { get; set; }
        public Surname Surname { get; set; }
        public DateOfBirth DateOfBirth { get; set; }
        public Nationality Nationality { get; set; }
        public CardType CardType { get; set; }
        public GivenNames GivenNames { get; set; }
        public PassPortNumber PassPortNumber { get; set; }
        public Type Type { get; set; }
        public DateOfExpiry DateOfExpiry { get; set; }
        public CountryCode CountryCode { get; set; }
        public Authority Authority { get; set; }
        public object Signature { get; set; }
    }

    public class DocumentResult
    {
        public string docType { get; set; }
        public string modelId { get; set; }
        public List<int> pageRange { get; set; }
        public Fields fields { get; set; }
        public double docTypeConfidence { get; set; }
    }

    public class AnalyzeResult
    {
        public string version { get; set; }
        public List<ReadResult> readResults { get; set; }
        public List<PageResult> pageResults { get; set; }
        public List<DocumentResult> documentResults { get; set; }
        public List<object> errors { get; set; }
    }

    public class Root
    {
        public string status { get; set; }
        public DateTime createdDateTime { get; set; }
        public DateTime lastUpdatedDateTime { get; set; }
        public AnalyzeResult analyzeResult { get; set; }
    }


}
