using System;

namespace AppifySheets.TBC.IntegrationService.Client.ApiConfiguration;

public record TBCApiCredentials(string Username, string Password);

// ReSharper disable once InconsistentNaming
// ReSharper disable once InconsistentNaming
public record TBCApiCredentialsWithCertificate
{
    public TBCApiCredentialsWithCertificate(TBCApiCredentials TBCApiCredentials, string CertificateFileName, string CertificatePassword)
    {
        if (!CertificateFileName.EndsWith(".pfx"))
            throw new InvalidOperationException("Certificate must have a '.pfx' extension");
        
        this.TBCApiCredentials = TBCApiCredentials;
        this.CertificateFileName = CertificateFileName;
        this.CertificatePassword = CertificatePassword;
    }

    public TBCApiCredentials TBCApiCredentials { get; }
    public string CertificateFileName { get; }
    public string CertificatePassword { get; }
}
