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

/// <summary>
/// TBC Bank SOAP API client for executing banking operations
/// </summary>
public sealed class TBCSoapCaller(TBCApiCredentialsWithCertificate tbcApiCredentialsWithCertificate)
{
    readonly TBCApiCredentialsWithCertificate _credentials = tbcApiCredentialsWithCertificate ?? throw new ArgumentNullException(nameof(tbcApiCredentialsWithCertificate));
    
    /// <summary>
    /// Creates a TBC SOAP caller instance with validation
    /// </summary>
    public static TBCSoapCaller Create(TBCApiCredentialsWithCertificate credentials) => new(credentials);

    /// <summary>
    /// Creates a TBC SOAP caller with all parameters
    /// </summary>
    public static Result<TBCSoapCaller> Create(
        string username, 
        string password,
        string certificateFileName, 
        string certificatePassword)
    {
        var credentialsResult = TBCApiCredentialsWithCertificate.Create(
            username, password, certificateFileName, certificatePassword);
            
        if (credentialsResult.IsFailure)
            return Result.Failure<TBCSoapCaller>(credentialsResult.Error);
            
        return Create(credentialsResult.Value);
    }
    public async Task<Result<TDeserializeInto>> GetDeserialized<TDeserializeInto>(RequestSoap<TDeserializeInto> requestSoap) where TDeserializeInto : ISoapResponse
    {
        var response = await CallTBCServiceAsync(requestSoap);
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (response.IsFailure) return response.ConvertFailure<TDeserializeInto>();

        return response.Value.DeserializeInto<TDeserializeInto>();
    }
    
    static PerformedActionSoapEnvelope GetPerformedActionFor(TBCApiCredentials credentials, TBCServiceAction serviceAction,
        [StringSyntax(StringSyntaxAttribute.Xml)]
        string xmlBody, string nonce)
    {
        try
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
                                  <wsse:Nonce>{nonce}</wsse:Nonce>
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
                var xmlText = xmlDoc.InnerXml.FormatXml();
            }

            return new PerformedActionSoapEnvelope(xmlDoc, serviceAction);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public Task<Result<string>> CallTBCServiceAsync<TDeserializeInto>(RequestSoap<TDeserializeInto> requestSoap) where TDeserializeInto : ISoapResponse
    {
        var template = GetPerformedActionFor(_credentials.Credentials, requestSoap.TBCServiceAction, requestSoap.SoapXml(), requestSoap.Nonce);

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
            // Try to parse SOAP fault first, fallback to formatted XML
            var faultParseResult = TryParseSoapFault(responseContent);
            return Result.Failure<string>(faultParseResult.IsSuccess
                ? faultParseResult.Value.FormattedError
                : responseContent.FormatXml());
        }

        X509Certificate2Collection GetCertificates()
        {
            var collection = new X509Certificate2Collection();
            collection.Import(_credentials.CertificateFileName, _credentials.CertificatePassword, X509KeyStorageFlags.PersistKeySet);
            return collection;
        }
    }

    /// <summary>
    /// Attempts to parse SOAP fault from response content
    /// </summary>
    static Result<SoapFaultResponse> TryParseSoapFault(string responseContent)
    {
        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(responseContent);

            var nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("s", "http://schemas.xmlsoap.org/soap/envelope/");

            var faultNode = doc.SelectSingleNode("//s:Fault", nsManager);
            if (faultNode == null)
                return Result.Failure<SoapFaultResponse>("No SOAP fault found in response");

            return faultNode.OuterXml.XmlDeserializeFromString<SoapFaultResponse>();
        }
        catch (Exception ex)
        {
            return Result.Failure<SoapFaultResponse>($"Failed to parse SOAP fault: {ex.Message}");
        }
    }
}