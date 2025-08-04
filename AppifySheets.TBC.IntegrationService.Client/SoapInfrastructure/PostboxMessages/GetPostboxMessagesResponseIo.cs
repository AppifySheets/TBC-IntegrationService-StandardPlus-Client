// using System.Xml.Serialization;
// XmlSerializer serializer = new XmlSerializer(typeof(Root));
// using (StringReader reader = new StringReader(xml))
// {
//    var test = (Root)serializer.Deserialize(reader);
// }

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using AppifySheets.TBC.IntegrationService.Client.SoapInfrastructure;

[XmlRoot(ElementName="additionalAttributes")]
public class AdditionalAttributes { 

    [XmlElement(ElementName="name")] 
    public required string Name { get; init; } 

    [XmlElement(ElementName="value")] 
    public DateTime Value { get; init; } 
}

[XmlRoot(ElementName="messages")]
public class Messages { 

    [XmlElement(ElementName="messageId")] 
    public int MessageId { get; init; } 

    [XmlElement(ElementName="messageText")] 
    public required string MessageText { get; init; } 

    [XmlElement(ElementName="messageType")] 
    public required string MessageType { get; init; } 

    [XmlElement(ElementName="messageStatus")] 
    public required string MessageStatus { get; init; } 

    [XmlElement(ElementName="additionalAttributes")] 
    public List<AdditionalAttributes>? AdditionalAttributes { get; init; } 
}

[XmlRoot(ElementName="GetPostboxMessagesResponseIo")]
public class GetPostboxMessagesResponseIo : ISoapResponse { 

    [XmlElement(ElementName="messages")] 
    public List<Messages>? Messages { get; init; } 

    [XmlAttribute(AttributeName="xsi")] 
    public string? Xsi { get; init; } 

    [XmlAttribute(AttributeName="xsd")] 
    public string? Xsd { get; init; } 

    [XmlText] 
    public string? Text { get; init; } 
}

[XmlRoot(ElementName="Root")]
public class Root { 

    [XmlElement(ElementName="GetPostboxMessagesResponseIo")] 
    public GetPostboxMessagesResponseIo? GetPostboxMessagesResponseIo { get; set; } 

    [XmlAttribute(AttributeName="ns2")] 
    public string? Ns2 { get; init; } 

    [XmlText] 
    public string? Text { get; init; } 
}