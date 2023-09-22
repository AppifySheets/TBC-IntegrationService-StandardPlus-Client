using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AppifySheets.TBC.IntegrationService.Client.ApiConfiguration;
using AppifySheets.TBC.IntegrationService.Client.SoapInfrastructure;
using AppifySheets.TBC.IntegrationService.Client.SoapInfrastructure.ImportSinglePaymentOrders;
using CSharpFunctionalExtensions;

namespace AppifySheets.TBC.IntegrationService.Client.TBC_Services;

// ReSharper disable once InconsistentNaming
public sealed class TBCSoapCaller(TBCApiCredentialsWithCertificate tbcApiCredentialsWithCertificate)
{
    public async Task<Result<TDeserializeInto>> GetDeserialized<TDeserializeInto>(RequestSoap<TDeserializeInto> RequestSoap) where TDeserializeInto : ISoapResponse
    {
        var response = await CallTBCServiceAsync(RequestSoap);
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (response.IsFailure) return response.ConvertFailure<TDeserializeInto>();

        return response.Value.DeserializeInto<TDeserializeInto>();
    }

    static PerformedActionSoapEnvelope GetPerformedActionFor(TBCApiCredentials credentials, TBCServiceAction serviceAction,
        [StringSyntax(StringSyntaxAttribute.Xml)]
        string xmlBody)
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

        if (Debugger.IsAttached)
        {
            var xmlText =  xmlDoc.InnerXml.FormatXml();
        }

        return new PerformedActionSoapEnvelope(xmlDoc, serviceAction);
    }

    public Task<Result<string>> CallTBCServiceAsync<TDeserializeInto>(RequestSoap<TDeserializeInto> requestSoap) where TDeserializeInto : ISoapResponse
    {
        var template = GetPerformedActionFor(tbcApiCredentialsWithCertificate.TBCApiCredentials, requestSoap.TBCServiceAction, requestSoap.SoapXml());

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

        var responseResult = await Result.Try(() => client.SendAsync(request), exception => exception.ToString());
        if (responseResult.IsFailure)
            return responseResult
                .ConvertFailure<string>()
                .OnFailureCompensate(r => r.FormatXml());

        

        using var response = responseResult.Value;

        var responseContent = await response.Content.ReadAsStringAsync();
        try
        {
            response.EnsureSuccessStatusCode();
            return responseContent;
        }
        catch (Exception)
        {
            return Result.Failure<string>(responseContent.FormatXml());
        }

        X509Certificate2Collection GetCertificates()
        {
            var collection = new X509Certificate2Collection();
            collection.Import(tbcApiCredentialsWithCertificate.CertificateFileName, tbcApiCredentialsWithCertificate.CertificatePassword, X509KeyStorageFlags.PersistKeySet);
            return collection;
        }
    }

    
}