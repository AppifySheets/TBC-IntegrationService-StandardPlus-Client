using System;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace AppifySheets.Immutable.BankIntegrationTypes;

/// <summary>
/// Represents an International Bank Account Number (IBAN)
/// </summary>
public record Iban
{
    const string IbanPattern = @"^[A-Z]{2}\d{2}[A-Z0-9]+$";
    const int MinIbanLength = 15;
    const int MaxIbanLength = 34;

    Iban(string value)
    {
        Value = value;
    }
    
    public string Value { get; }
    
    /// <summary>
    /// Creates an IBAN instance with validation
    /// </summary>
    public static Result<Iban> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Iban>("IBAN cannot be empty");
            
        // Normalize: remove spaces and convert to uppercase
        var normalized = value.Replace(" ", "").ToUpperInvariant();
        
        if (normalized.Length is < MinIbanLength or > MaxIbanLength)
            return Result.Failure<Iban>($"IBAN length must be between {MinIbanLength} and {MaxIbanLength} characters");
            
        if (!Regex.IsMatch(normalized, IbanPattern))
            return Result.Failure<Iban>("Invalid IBAN format");
            
        return Result.Success(new Iban(normalized));
    }
    
    /// <summary>
    /// Creates an IBAN instance without validation (for non-IBAN account numbers)
    /// </summary>
    internal static Result<Iban> CreateWithoutValidation(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Iban>("Account number cannot be empty");
            
        // Just normalize by removing spaces and converting to uppercase
        var normalized = value.Replace(" ", "").ToUpperInvariant();
        return Result.Success(new Iban(normalized));
    }
    
    
    public override string ToString() => Value;
    
    // Implicit conversion to string for convenience
    public static implicit operator string(Iban iban) => iban.Value;
}