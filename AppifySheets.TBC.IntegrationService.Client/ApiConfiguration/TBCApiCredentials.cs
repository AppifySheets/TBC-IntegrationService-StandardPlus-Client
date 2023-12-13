namespace AppifySheets.TBC.IntegrationService.Client.ApiConfiguration;

public record TBCApiCredentials(string Username, string Password);

public record TBCApiCredentialsWithCertificate(TBCApiCredentials TBCApiCredentials, string CertificateFileName, string CertificatePassword);
