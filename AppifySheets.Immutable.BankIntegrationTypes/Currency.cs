using System;
using System.Linq;
using CSharpFunctionalExtensions;

namespace AppifySheets.Immutable.BankIntegrationTypes;

/// <summary>
/// Represents a currency with ISO 4217 3-letter code
/// </summary>
public record Currency
{
    // Common currencies as static instances
    // ReSharper disable once InconsistentNaming
    public static readonly Currency GEL = new("GEL");
    // ReSharper disable once InconsistentNaming
    public static readonly Currency USD = new("USD");
    // ReSharper disable once InconsistentNaming
    public static readonly Currency EUR = new("EUR");
    // ReSharper disable once InconsistentNaming
    public static readonly Currency GBP = new("GBP");
    // ReSharper disable once InconsistentNaming
    public static readonly Currency CHF = new("CHF");
    // ReSharper disable once InconsistentNaming
    public static readonly Currency JPY = new("JPY");
    // ReSharper disable once InconsistentNaming
    public static readonly Currency CNY = new("CNY");

    Currency(string code)
    {
        Code = code;
    }
    
    public string Code { get; }
    
    /// <summary>
    /// Creates a currency instance with validation
    /// </summary>
    public static Result<Currency> Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure<Currency>("Currency code cannot be empty");
            
        var normalized = code.Trim().ToUpperInvariant();
        
        if (normalized.Length != 3)
            return Result.Failure<Currency>("Currency code must be exactly 3 characters (ISO 4217)");
            
        if (!normalized.All(char.IsLetter))
            return Result.Failure<Currency>("Currency code must contain only letters");
            
        return Result.Success(new Currency(normalized));
    }
    
    /// <summary>
    /// Parses a currency code, throwing exception if invalid
    /// </summary>
    public static Currency Parse(string code)
    {
        var result = Create(code);
        if (result.IsFailure)
            throw new ArgumentException(result.Error, nameof(code));
        return result.Value;
    }
    
    public override string ToString() => Code;
    
    // Implicit conversion to string for convenience
    public static implicit operator string(Currency currency) => currency.Code;
}