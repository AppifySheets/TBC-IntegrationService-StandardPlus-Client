using CSharpFunctionalExtensions;

namespace AppifySheets.Immutable.BankIntegrationTypes;

/// <summary>
/// Represents a bank account with IBAN and currency
/// </summary>
public record BankAccount
{
    BankAccount(Iban iban, Currency currency)
    {
        Iban = iban;
        Currency = currency;
    }
    
    public Iban Iban { get; }
    public Currency Currency { get; }
    
    /// <summary>
    /// Creates a bank account with validation
    /// </summary>
    /// <param name="iban">The IBAN or account number</param>
    /// <param name="currencyCode">The currency code</param>
    /// <param name="validateIban">Whether to validate the IBAN format (default: true)</param>
    public static Result<BankAccount> Create(string iban, string currencyCode, bool validateIban = true)
    {
        // Create IBAN with or without validation based on the flag
        var ibanResult = validateIban ? Iban.Create(iban) : Iban.CreateWithoutValidation(iban);
            
        if (ibanResult.IsFailure)
            return Result.Failure<BankAccount>($"Invalid {(validateIban ? "IBAN" : "account number")}: {ibanResult.Error}");
            
        var currencyResult = Currency.Create(currencyCode);
        if (currencyResult.IsFailure)
            return Result.Failure<BankAccount>($"Invalid currency: {currencyResult.Error}");
            
        return Result.Success(new BankAccount(ibanResult.Value, currencyResult.Value));
    }
    
    /// <summary>
    /// Creates a bank account with pre-validated components
    /// </summary>
    public static Result<BankAccount> Create(Iban iban, Currency currency)
    {
        if (iban == null)
            return Result.Failure<BankAccount>("IBAN cannot be null");
        if (currency == null)
            return Result.Failure<BankAccount>("Currency cannot be null");
            
        return Result.Success(new BankAccount(iban, currency));
    }
    
    
    public override string ToString() => $"{Iban}{Currency}";
    
    // For backward compatibility - maps to old property names
    public Iban BankAccountNumber => Iban;
    public Currency CurrencyV => Currency;
}