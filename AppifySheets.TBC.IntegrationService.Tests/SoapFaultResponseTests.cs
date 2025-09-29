using AppifySheets.TBC.IntegrationService.Client.SoapInfrastructure;
using AppifySheets.TBC.IntegrationService.Client.TBC_Services;
using CSharpFunctionalExtensions;
using Shouldly;
using Xunit;

namespace AppifySheets.TBC.IntegrationService.Tests;

public class SoapFaultResponseTests
{
    [Fact]
    public void Should_Deserialize_SOAP_Fault_Response()
    {
        // Arrange
        const string soapFaultXml = """
            <?xml version="1.0" encoding="UTF-8" standalone="no"?>
            <s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
                <s:Header/>
                <s:Body>
                    <s:Fault>
                        <faultcode xmlns:a="http://www.mygemini.com/schemas/mygemini">a:USER_IS_BLOCKED</faultcode>
                        <faultstring xml:lang="en">User is blocked.</faultstring>
                    </s:Fault>
                </s:Body>
            </s:Envelope>
            """;

        // Act
        var result = soapFaultXml.DeserializeInto<SoapFaultResponse>();

        // Assert - deserialization should fail with SOAP fault error message
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldContain("USER_IS_BLOCKED");
        result.Error.ShouldContain("User is blocked");
    }


    [Fact]
    public void Should_Create_SoapFaultResponse_With_Static_Factory()
    {
        // Act
        var fault = SoapFaultResponse.Create("a:USER_IS_BLOCKED", "User is blocked.");

        // Assert
        fault.FaultCode.ShouldBe("a:USER_IS_BLOCKED");
        fault.FaultString.ShouldBe("User is blocked.");
        fault.FormattedError.ShouldBe("SOAP Fault [a:USER_IS_BLOCKED]: User is blocked.");
    }

    [Fact]
    public void Should_Check_Fault_Code_Case_Insensitive()
    {
        // Arrange
        var fault = SoapFaultResponse.Create("a:USER_IS_BLOCKED", "User is blocked.");

        // Act & Assert
        fault.IsFaultCode("a:user_is_blocked").ShouldBeTrue();
        fault.IsFaultCode("A:USER_IS_BLOCKED").ShouldBeTrue();
        fault.IsFaultCode("a:USER_IS_BLOCKED").ShouldBeTrue();
        fault.IsFaultCode("different_code").ShouldBeFalse();
    }

    [Fact]
    public void TryParseSoapFault_Should_Parse_Valid_Fault()
    {
        // Arrange
        const string soapFaultXml = """
            <?xml version="1.0" encoding="UTF-8" standalone="no"?>
            <s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
                <s:Header/>
                <s:Body>
                    <s:Fault>
                        <faultcode xmlns:a="http://www.mygemini.com/schemas/mygemini">a:USER_IS_BLOCKED</faultcode>
                        <faultstring xml:lang="en">User is blocked.</faultstring>
                    </s:Fault>
                </s:Body>
            </s:Envelope>
            """;

        // Act - use reflection to call the private static method
        var method = typeof(TBCSoapCaller).GetMethod("TryParseSoapFault",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var result = (Result<SoapFaultResponse>)method!.Invoke(null, [soapFaultXml])!;

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.FaultCode.ShouldBe("a:USER_IS_BLOCKED");
        result.Value.FaultString.ShouldBe("User is blocked.");
        result.Value.FormattedError.ShouldBe("SOAP Fault [a:USER_IS_BLOCKED]: User is blocked.");
    }
}