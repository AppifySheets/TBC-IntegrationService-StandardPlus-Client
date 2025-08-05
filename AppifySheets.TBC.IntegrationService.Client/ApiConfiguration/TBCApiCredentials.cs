using System;
using CSharpFunctionalExtensions;

namespace AppifySheets.TBC.IntegrationService.Client.ApiConfiguration;

/// <summary>
/// TBC Bank API credentials
/// </summary>
public record TBCApiCredentials(string Username, string Password)
{
    /// <summary>
    /// Creates TBC API credentials with validation
    /// </summary>
    public static Result<TBCApiCredentials> Create(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
            return Result.Failure<TBCApiCredentials>("Username cannot be empty");
            
        if (string.IsNullOrWhiteSpace(password))
            return Result.Failure<TBCApiCredentials>("Password cannot be empty");
            
        return Result.Success(new TBCApiCredentials(username.Trim(), password));
    }
}

/// <summary>
/// TBC Bank API credentials with certificate for authentication
/// </summary>
public record TBCApiCredentialsWithCertificate
{
    // Public constructor for backward compatibility
    public TBCApiCredentialsWithCertificate(TBCApiCredentials credentials, string certificateFileName, string certificatePassword)
    {
        if (!certificateFileName.EndsWith(".pfx", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Certificate must have a '.pfx' extension");
            
        Credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
        CertificateFileName = certificateFileName;
        CertificatePassword = certificatePassword;
    }
    
    public TBCApiCredentials Credentials { get; }
    public string CertificateFileName { get; }
    public string CertificatePassword { get; }
    
    // Backward compatibility
    public TBCApiCredentials TBCApiCredentials => Credentials;
    
    /// <summary>
    /// Creates TBC API credentials with certificate validation
    /// </summary>
    public static Result<TBCApiCredentialsWithCertificate> Create(
        TBCApiCredentials credentials, 
        string certificateFileName, 
        string certificatePassword)
    {
        if (credentials == null)
            return Result.Failure<TBCApiCredentialsWithCertificate>("Credentials cannot be null");
            
        if (string.IsNullOrWhiteSpace(certificateFileName))
            return Result.Failure<TBCApiCredentialsWithCertificate>("Certificate file name cannot be empty");
            
        if (!certificateFileName.EndsWith(".pfx", StringComparison.OrdinalIgnoreCase))
            return Result.Failure<TBCApiCredentialsWithCertificate>("Certificate must have a '.pfx' extension");
            
        if (string.IsNullOrWhiteSpace(certificatePassword))
            return Result.Failure<TBCApiCredentialsWithCertificate>("Certificate password cannot be empty");
            
        return Result.Success(new TBCApiCredentialsWithCertificate(
            credentials, 
            certificateFileName.Trim(), 
            certificatePassword));
    }
    
    /// <summary>
    /// Creates credentials with all parameters
    /// </summary>
    public static Result<TBCApiCredentialsWithCertificate> Create(
        string username, 
        string password,
        string certificateFileName, 
        string certificatePassword)
    {
        var credentialsResult = TBCApiCredentials.Create(username, password);
        if (credentialsResult.IsFailure)
            return Result.Failure<TBCApiCredentialsWithCertificate>(credentialsResult.Error);
            
        return Create(credentialsResult.Value, certificateFileName, certificatePassword);
    }
}
