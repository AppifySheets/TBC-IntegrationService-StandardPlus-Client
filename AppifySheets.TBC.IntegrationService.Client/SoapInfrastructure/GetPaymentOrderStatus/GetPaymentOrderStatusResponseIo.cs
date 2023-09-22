using System.Xml.Serialization;
using static AppifySheets.TBC.IntegrationService.Client.SoapInfrastructure.GetPaymentOrderStatus.PaymentStatusEnum;

namespace AppifySheets.TBC.IntegrationService.Client.SoapInfrastructure.GetPaymentOrderStatus;

[XmlRoot(ElementName="GetPaymentOrderStatusResponseIo")]
public class GetPaymentOrderStatusResponseIo : ISoapResponse
{ 

    [XmlElement(ElementName="status")] 
    public string Status { get; init; }

    public PaymentStatusEnum PaymentStatus => Status switch
    {
        "I" => InitialState,
        "DR" => Draft,
        "D" => Deleted,
        "WC" => WaitingForCertification,
        "WS" or "CERT" or "VERIF" => InProgress,
        "CPE" => Error,
        "F" => Finished,
        "FL" => Failed,
        "C" => Cancelled,
        _ => Other
    };
    //
    // [XmlAttribute(AttributeName="ns2")] 
    // public string Ns2 { get; init; } 
    //
    // [XmlText] 
    // public string Text { get; init; } 
}


/*
 *I	Initial state
DR	Draft
G	Registered
D	Deleted
WC	Waiting for certification
CERT	In progress
VERIF	In progress
CPE	Error
WS	In progress
F	Finished
FL	Failed
C	Cancelled
 *
 */

public enum PaymentStatusEnum
{
    InitialState,
    Draft,
    Registered,
    Deleted,
    WaitingForCertification,
    InProgress,
    Error,
    Finished,
    Failed,
    Cancelled,
    Other
}


[XmlRoot(ElementName="Body")]
public class Body { 

    [XmlElement(ElementName="GetPaymentOrderStatusResponseIo")] 
    public GetPaymentOrderStatusResponseIo? GetPaymentOrderStatusResponseIo { get; init; } 
}