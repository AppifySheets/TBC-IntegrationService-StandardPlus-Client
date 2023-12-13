using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AppifySheets.TBC.IntegrationService.Client.ApiConfiguration;
using AppifySheets.TBC.IntegrationService.Client.SoapInfrastructure;
using CSharpFunctionalExtensions;

namespace AppifySheets.TBC.IntegrationService.Client.TBC_Services;

public class TBCSoapCaller
{
    readonly TBCApiCredentialsWithCertificate _tbcApiCredentialsWithCertificate;

    public TBCSoapCaller()
    {
        var credentials = new TBCApiCredentials("2684679_1", "3a$5ti2PLBd8Ut^yfi");
        _tbcApiCredentialsWithCertificate = new TBCApiCredentialsWithCertificate(credentials, "TBCIntegrationService.pfx", "XR_w;,64");
    }

    static PerformedActionSoapEnvelope GetPerformedActionFor(TBCApiCredentials credentials, PerformedActionSoapEnvelope.TBCServiceAction serviceAction, 
        [StringSyntax(StringSyntaxAttribute.Xml)] string xmlBody)
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml($"""
                        <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                        xmlns:myg="http://www.mygemini.com/schemas/mygemini"
                        xmlns:wsse="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"
                        xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
                           <soapenv:Header>
                           <wsse:Security>
                            <wsse:UsernameToken>
                              <wsse:Username>{credentials.Username}</wsse:Username>
                              <wsse:Password>{credentials.Password}</wsse:Password>
                              <wsse:Nonce>1111</wsse:Nonce>
                            </wsse:UsernameToken>
                           </wsse:Security>
                           </soapenv:Header>
                           <soapenv:Body>
                             {xmlBody}
                           </soapenv:Body>
                        </soapenv:Envelope>
                        """);

        return new PerformedActionSoapEnvelope(xmlDoc, serviceAction);
    }
    public Task<Result<string>> CallTBCServiceAsync(SoapBase soapBase)
    {
        var template = GetPerformedActionFor(_tbcApiCredentialsWithCertificate.TBCApiCredentials, soapBase.TBCServiceAction, soapBase.SoapXml);
        
        return CallTBCServiceAsync(template);
    }
    
    async Task<Result<string>> CallTBCServiceAsync(PerformedActionSoapEnvelope performedActionSoapEnvelope)
    {
        const string url = "https://secdbi.tbconline.ge/dbi/dbiService";
        var action = $"http://www.mygemini.com/schemas/mygemini/{performedActionSoapEnvelope.Action}";

        var soapEnvelopeXml = performedActionSoapEnvelope.Document;
        using var handler = new HttpClientHandler();
        handler.ClientCertificateOptions = ClientCertificateOption.Manual;
        handler.SslProtocols = SslProtocols.Tls12;
        handler.ClientCertificates.AddRange(GetCertificates());

        using var client = new HttpClient(handler);

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("SOAPAction", action);

        using var content = new StringContent(soapEnvelopeXml.OuterXml, Encoding.UTF8, "text/xml");
        request.Content = content;

        using var response = await client.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();
        try
        {
            response.EnsureSuccessStatusCode();
            return responseContent;
        }
        catch (Exception)
        {
            return Result.Failure<string>(responseContent);
        }

        X509Certificate2Collection GetCertificates()
        {
            var collection = new X509Certificate2Collection();
            collection.Import(_tbcApiCredentialsWithCertificate.CertificateFileName, _tbcApiCredentialsWithCertificate.CertificatePassword, X509KeyStorageFlags.PersistKeySet);
            return collection;
        }
    }
}