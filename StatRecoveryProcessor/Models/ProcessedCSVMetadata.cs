using System;
using System.Collections.Generic;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;
using CsvHelper;
using CsvHelper.Configuration;

namespace StatRecoveryProcessor.Models;

public class ProcessedCSVMetadataMap : ClassMap<ProcessedCSVMetadata>
{
    public ProcessedCSVMetadataMap()
    {
        Map(m => m.Id).Name("Id");
        Map(m => m.ClaimNumber).Name("Claim Number");
        Map(m => m.ClaimDate).Name("Claim Date").TypeConverterOption.NullValues("");
        Map(m => m.OpenAmount).Name("Open Amount");
        Map(m => m.OriginalAmount).Name("Original Amount");
        Map(m => m.Status).Name("Status");
        Map(m => m.CustomerName).Name("Customer Name");
        Map(m => m.ARReasonCode).Name("AR Reason Code");
        Map(m => m.CustomerReasonCode).Name("Customer Reason Code");
        Map(m => m.AttachmentList).Name("Attachment List").TypeConverter<AttachmentListConverter>();
        Map(m => m.CheckNumber).Name("Check Number");
        Map(m => m.CheckDate).Name("Check Date").TypeConverterOption.NullValues("");
        Map(m => m.Comments).Name("Comments");
        Map(m => m.DaysOutstanding).Name("Days Outstanding");
        Map(m => m.Division).Name("Division");
        Map(m => m.PONumber).Name("PO Number");
        Map(m => m.Brand).Name("Brand");
        Map(m => m.MergeStatus).Name("Merge Status");
        Map(m => m.UnresolvedAmount).Name("Unresolved Amount");
        Map(m => m.DocumentType).Name("Document Type");
        Map(m => m.DocumentDate).Name("Document Date").TypeConverterOption.NullValues("");
        Map(m => m.OriginalCustomer).Name("Original Customer");
        Map(m => m.Location).Name("Location");
        Map(m => m.CustomerLocation).Name("Customer Location");
        Map(m => m.CreateDate).Name("Create Date").TypeConverterOption.NullValues("");
        Map(m => m.LoadId).Name("Load Id");
        Map(m => m.CarrierName).Name("Carrier Name");
        Map(m => m.InvoiceStoreNumber).Name("Invoice Store Number");
    }
}

public class AttachmentListConverter : DefaultTypeConverter
{
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrEmpty(text))
            return new List<string>();

        return text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }

    public override string? ConvertToString(object? value, IWriterRow row, MemberMapData memberMapData)
    {
        if (value is List<string> list)
            return string.Join(",", list);
        return string.Empty;
    }
}

public class ProcessedCSVMetadata
{
    public string Id { get; set; } = string.Empty;
    public string ClaimNumber { get; set; } = string.Empty;
    public DateTime? ClaimDate { get; set; }
    public decimal OpenAmount { get; set; }
    public decimal OriginalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string ARReasonCode { get; set; } = string.Empty;
    public string CustomerReasonCode { get; set; } = string.Empty;
    public List<string> AttachmentList { get; set; } = new();
    public string CheckNumber { get; set; } = string.Empty;
    public DateTime? CheckDate { get; set; }
    public string Comments { get; set; } = string.Empty;
    public int DaysOutstanding { get; set; }
    public string Division { get; set; } = string.Empty;
    public string PONumber { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string MergeStatus { get; set; } = string.Empty;
    public decimal UnresolvedAmount { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public DateTime? DocumentDate { get; set; }
    public string OriginalCustomer { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string CustomerLocation { get; set; } = string.Empty;
    public DateTime? CreateDate { get; set; }
    public string LoadId { get; set; } = string.Empty;
    public string CarrierName { get; set; } = string.Empty;
    public string InvoiceStoreNumber { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"Claim: {ClaimNumber}, ClaimDate: {ClaimDate} PO: {PONumber}, Attachments: {string.Join(", ", AttachmentList)}";
    }
}