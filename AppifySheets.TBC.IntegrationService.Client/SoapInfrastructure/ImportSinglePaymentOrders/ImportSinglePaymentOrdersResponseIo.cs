using System.Xml.Serialization;

namespace AppifySheets.TBC.IntegrationService.Client.SoapInfrastructure.ImportSinglePaymentOrders;

[XmlRoot(ElementName="PaymentOrdersResults")]
public class PaymentOrdersResults { 

    [XmlElement(ElementName="position")] 
    public int Position { get; init; } 

    [XmlElement(ElementName="paymentId")] 
    public int PaymentId { get; init; } 
}

[XmlRoot(ElementName="ImportSinglePaymentOrdersResponseIo")]
public class ImportSinglePaymentOrdersResponseIo : ISoapResponse
{ 

    [XmlElement(ElementName="PaymentOrdersResults")] 
    public PaymentOrdersResults? PaymentOrdersResults { get; init; } 
}