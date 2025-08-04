// using System.Xml.Serialization;
// XmlSerializer serializer = new XmlSerializer(typeof(Envelope));
// using (StringReader reader = new StringReader(xml))
// {
//    var test = (Envelope)serializer.Deserialize(reader);
// }

using System.Xml.Serialization;

namespace AppifySheets.TBC.IntegrationService.Client.SoapInfrastructure.PasswordChangeRelated;

[XmlRoot(ElementName="ChangePasswordResponseIo")]
public class ChangePasswordResponseIo : ISoapResponse { 

    [XmlElement(ElementName="message")] 
    public string? Message { get; init; } 

    [XmlAttribute(AttributeName="i")] 
    public string? I { get; init; } 

    [XmlText] 
    public string? Text { get; init; } 
}

[XmlRoot(ElementName="Body")]
public class Body { 

    [XmlElement(ElementName="ChangePasswordResponseIo")] 
    public ChangePasswordResponseIo? ChangePasswordResponseIo { get; init; } 
}

[XmlRoot(ElementName="Envelope")]
public class Envelope { 

    [XmlElement(ElementName="Header")] 
    public object? Header { get; init; } 

    [XmlElement(ElementName="Body")] 
    public required Body Body { get; init; } 

    [XmlAttribute(AttributeName="SOAP-ENV")] 
    public string? SOAPENV { get; init; } 

    [XmlAttribute(AttributeName="ns2")] 
    public string? Ns2 { get; init; } 

    [XmlText] 
    public string? Text { get; init; } 
}