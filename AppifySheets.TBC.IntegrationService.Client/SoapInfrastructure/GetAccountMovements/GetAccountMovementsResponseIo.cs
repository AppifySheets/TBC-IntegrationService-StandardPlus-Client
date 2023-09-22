using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace AppifySheets.TBC.IntegrationService.Client.SoapInfrastructure.GetAccountMovements;

[XmlRoot(ElementName = "pager")]
public class Pager
{
    [XmlElement(ElementName = "pageIndex")]
    public int PageIndex { get; init; }

    [XmlElement(ElementName = "pageSize")] public int PageSize { get; init; }
}

[XmlRoot(ElementName = "result")]
public class ResultXml
{
    [XmlElement(ElementName = "pager")] public Pager Pager { get; init; }

    [XmlElement(ElementName = "totalCount")]
    public int TotalCount { get; init; }
}

[XmlRoot(ElementName = "amount")]
public class AmountT
{
    [XmlElement(ElementName = "amount")] public decimal Amount { get; init; }
    [XmlElement(ElementName = "currency")] public string Currency { get; init; }
}

[XmlRoot(ElementName = "accountMovement")]
public class AccountMovement
{
    [XmlElement(ElementName = "movementId")]
    public string MovementId { get; init; }

    [XmlElement(ElementName = "externalPaymentId")]
    public string ExternalPaymentId { get; init; }

    [XmlElement(ElementName = "debitCredit")]
    public string DebitCredit { get; init; }

    [XmlElement(ElementName = "valueDate")]
    public DateTime ValueDate { get; init; }

    [XmlElement(ElementName = "description")]
    public string Description { get; init; }

    [XmlElement(ElementName = "amount")] public AmountT Amount { get; init; }

    [XmlElement(ElementName = "accountNumber")]
    public string AccountNumber { get; init; }

    [XmlElement(ElementName = "accountName")]
    public string AccountName { get; init; }

    [XmlElement(ElementName = "additionalInformation")]
    public string AdditionalInformation { get; init; }

    [XmlElement(ElementName = "documentDate")]
    public string DocumentDate { get; init; }

    [XmlElement(ElementName = "documentNumber")]
    public string DocumentNumber { get; init; }

    [XmlElement(ElementName = "partnerAccountNumber")]
    public string PartnerAccountNumber { get; init; }

    [XmlElement(ElementName = "partnerName")]
    public string PartnerName { get; init; }

    [XmlElement(ElementName = "partnerTaxCode")]
    public string PartnerTaxCode { get; init; }

    [XmlElement(ElementName = "partnerBankCode")]
    public string PartnerBankCode { get; init; }

    [XmlElement(ElementName = "partnerBank")]
    public string PartnerBank { get; init; }

    [XmlElement(ElementName = "taxpayerCode")]
    public string TaxpayerCode { get; init; }

    [XmlElement(ElementName = "taxpayerName")]
    public string TaxpayerName { get; init; }

    [XmlElement(ElementName = "operationCode")]
    public string OperationCode { get; init; }

    [XmlElement(ElementName = "partnerDocumentType")]
    public string PartnerDocumentType { get; init; }

    [XmlElement(ElementName = "statusCode")]
    public string StatusCode { get; init; }

    [XmlElement(ElementName = "transactionType")]
    public string TransactionType { get; init; }

    [XmlElement(ElementName = "additionalDescription")]
    public string AdditionalDescription { get; init; }

    [XmlElement(ElementName = "partnerPersonalNumber")]
    public string PartnerPersonalNumber { get; init; }

    [XmlElement(ElementName = "partnerDocumentNumber")]
    public string PartnerDocumentNumber { get; init; }

    [XmlElement(ElementName = "paymentId")]
    public string PaymentId { get; init; }

    [XmlElement(ElementName = "parentExternalPaymentId")]
    public string ParentExternalPaymentId { get; init; }
}

[XmlRoot(ElementName = "GetAccountMovementsResponseIo")]
public class GetAccountMovementsResponseIo : ISoapResponse
{
    [XmlElement(ElementName = "result")] public ResultXml? ResultXml { get; init; }

    [XmlElement(ElementName = "accountMovement")]
    public List<AccountMovement>? AccountMovement { get; init; }

    [XmlAttribute(AttributeName = "ns2", Namespace = "http://www.w3.org/2000/xmlns/")]
    public string Ns2 { get; init; }
}