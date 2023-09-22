using System.Xml;
using AppifySheets.TBC.IntegrationService.Client.TBC_Services;
using CSharpFunctionalExtensions;

namespace AppifySheets.TBC.IntegrationService.Client.SoapInfrastructure;

public interface ISoapResponse;

// ReSharper disable once UnusedTypeParameter
public abstract record RequestSoap<TResponseDeserializeInto> where TResponseDeserializeInto : ISoapResponse
{
    public abstract string SoapXml();

    public abstract TBCServiceAction TBCServiceAction { get; }
}

public static class DeserializationExtensions
{
    public static Result<T> DeserializeInto<T>(this string str)
    {
        var doc = new XmlDocument();
        doc.LoadXml(str);
        var bodyNode = doc.SelectSingleNode("//SOAP-ENV:Body", GetNamespaceManager(doc));
        if (bodyNode == null) return Result.Failure<T>("bodyNode was null");
        
        return bodyNode.InnerXml.Replace("ns2:","")
            .XmlDeserializeFromString<T>();

        static XmlNamespaceManager GetNamespaceManager(XmlDocument doc)
        {
            var nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("SOAP-ENV", "http://schemas.xmlsoap.org/soap/envelope/");
            nsManager.AddNamespace("ns2", "http://www.mygemini.com/schemas/mygemini");
            return nsManager;
        }
    }
}