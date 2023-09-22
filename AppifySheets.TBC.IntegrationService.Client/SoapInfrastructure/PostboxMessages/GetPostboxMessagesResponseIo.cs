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
    public string Name { get; set; } 

    [XmlElement(ElementName="value")] 
    public DateTime Value { get; set; } 
}

[XmlRoot(ElementName="messages")]
public class Messages { 

    [XmlElement(ElementName="messageId")] 
    public int MessageId { get; set; } 

    [XmlElement(ElementName="messageText")] 
    public string MessageText { get; set; } 

    [XmlElement(ElementName="messageType")] 
    public string MessageType { get; set; } 

    [XmlElement(ElementName="messageStatus")] 
    public string MessageStatus { get; set; } 

    [XmlElement(ElementName="additionalAttributes")] 
    public List<AdditionalAttributes> AdditionalAttributes { get; set; } 
}

[XmlRoot(ElementName="GetPostboxMessagesResponseIo")]
public class GetPostboxMessagesResponseIo : ISoapResponse { 

    [XmlElement(ElementName="messages")] 
    public List<Messages> Messages { get; set; } 

    [XmlAttribute(AttributeName="xsi")] 
    public string Xsi { get; set; } 

    [XmlAttribute(AttributeName="xsd")] 
    public string Xsd { get; set; } 

    [XmlText] 
    public string Text { get; set; } 
}

[XmlRoot(ElementName="Root")]
public class Root { 

    [XmlElement(ElementName="GetPostboxMessagesResponseIo")] 
    public GetPostboxMessagesResponseIo? GetPostboxMessagesResponseIo { get; set; } 

    [XmlAttribute(AttributeName="ns2")] 
    public string Ns2 { get; set; } 

    [XmlText] 
    public string Text { get; set; } 
}