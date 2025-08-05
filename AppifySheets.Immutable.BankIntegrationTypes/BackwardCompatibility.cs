using System;
using CSharpFunctionalExtensions;

namespace AppifySheets.Immutable.BankIntegrationTypes;

/// <summary>
/// Type aliases for backward compatibility
/// These are kept for backward compatibility only
/// New code should use Iban, Currency, and BankAccount instead
/// </summary>

// Keep original classes but mark as obsolete
[Obsolete("Use Iban instead")]
public record BankAccountV
{
    public BankAccountV(string accountNumber)
    {
        const string pattern2Match = @"(^[a-zA-Z]{2}\d{2}[a-zA-Z]{2}\d{16})(\w{3})?$";
        var iban = System.Text.RegularExpressions.Regex.Match(accountNumber, pattern2Match).Groups[1].Value;
        AccountNumber = accountNumber;
    }

    public string AccountNumber { get; }
    public override string ToString() => AccountNumber;
}

[Obsolete("Use Currency instead")]
public record CurrencyV
{
    public static readonly CurrencyV GEL = new CurrencyV("GEL"); 
    public static readonly CurrencyV USD = new CurrencyV("USD"); 
    public static readonly CurrencyV EUR = new CurrencyV("EUR"); 
    public static readonly CurrencyV GBP = new CurrencyV("GBP"); 
    public string Code { get; }
    public CurrencyV(string code)
    {
        if (code.Length != 3)
            throw new InvalidOperationException("Code must be 3 characters long");
        Code = code;
    }

    public override string ToString() => Code;
}

[Obsolete("Use BankAccount instead")]
public class BankAccountWithCurrencyV(BankAccountV bankAccountNumber, CurrencyV currencyV)
{
    public BankAccountV BankAccountNumber { get; } = bankAccountNumber ?? throw new ArgumentNullException(nameof(bankAccountNumber));
    public CurrencyV CurrencyV { get; } = currencyV ?? throw new ArgumentNullException(nameof(currencyV));

    public static Result<BankAccountWithCurrencyV> Create(BankAccountV bankAccountNumber, CurrencyV currencyV) 
        => Result.Try(() => new BankAccountWithCurrencyV(bankAccountNumber, currencyV));

    public override string ToString() => $"{BankAccountNumber}{CurrencyV}";
}