using System;
using System.Xml.Serialization;

namespace AppifySheets.TBC.IntegrationService.Client.SoapInfrastructure;

/// <summary>
/// Represents a SOAP fault response from TBC Bank API
/// Contains error code and message when API calls fail
/// </summary>
[XmlRoot("Fault", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
public sealed record SoapFaultResponse : ISoapResponse
{
    /// <summary>
    /// The fault code indicating the type of error (e.g., "a:USER_IS_BLOCKED")
    /// </summary>
    [XmlElement("faultcode", Namespace = "")]
    public string FaultCode { get; init; } = string.Empty;

    /// <summary>
    /// Human-readable description of the error
    /// </summary>
    [XmlElement("faultstring", Namespace = "")]
    public string FaultString { get; init; } = string.Empty;

    /// <summary>
    /// Creates a SoapFaultResponse with the specified fault code and message
    /// </summary>
    public static SoapFaultResponse Create(string faultCode, string faultString) =>
        new() { FaultCode = faultCode, FaultString = faultString };

    /// <summary>
    /// Gets a formatted error message combining fault code and string
    /// </summary>
    public string FormattedError => $"SOAP Fault [{FaultCode}]: {FaultString}";

    /// <summary>
    /// Checks if this represents a specific fault code
    /// </summary>
    public bool IsFaultCode(string faultCode) =>
        string.Equals(FaultCode, faultCode, StringComparison.OrdinalIgnoreCase);
}