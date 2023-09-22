// using System.Xml.Serialization;
// XmlSerializer serializer = new XmlSerializer(typeof(Envelope));
// using (StringReader reader = new StringReader(xml))
// {
//    var test = (Envelope)serializer.Deserialize(reader);
// }

using System.Xml.Serialization;
using AppifySheets.TBC.IntegrationService.Client.SoapInfrastructure;

[XmlRoot(ElementName="ChangePasswordResponseIo")]
public class ChangePasswordResponseIo : ISoapResponse { 

    [XmlElement(ElementName="message")] 
    public string Message { get; set; } 

    [XmlAttribute(AttributeName="i")] 
    public string I { get; set; } 

    [XmlText] 
    public string Text { get; set; } 
}

[XmlRoot(ElementName="Body")]
public class Body { 

    [XmlElement(ElementName="ChangePasswordResponseIo")] 
    public ChangePasswordResponseIo ChangePasswordResponseIo { get; set; } 
}

[XmlRoot(ElementName="Envelope")]
public class Envelope { 

    [XmlElement(ElementName="Header")] 
    public object Header { get; set; } 

    [XmlElement(ElementName="Body")] 
    public Body Body { get; set; } 

    [XmlAttribute(AttributeName="SOAP-ENV")] 
    public string SOAPENV { get; set; } 

    [XmlAttribute(AttributeName="ns2")] 
    public string Ns2 { get; set; } 

    [XmlText] 
    public string Text { get; set; } 
}