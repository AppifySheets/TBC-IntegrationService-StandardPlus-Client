using System;
using CSharpFunctionalExtensions;

namespace AppifySheets.Immutable.BankIntegrationTypes;

public class BankAccountWithCurrencyV
{
    BankAccountWithCurrencyV(BankAccountV bankAccountNumber, CurrencyV currencyV)
    {
        BankAccountNumber = bankAccountNumber ?? throw new ArgumentNullException(nameof(bankAccountNumber));

        CurrencyV = currencyV ?? throw new ArgumentNullException(nameof(currencyV));
    }

    public static Result<BankAccountWithCurrencyV> Create(BankAccountV bankAccountNumber, CurrencyV currencyV) => Result.Try(() => new BankAccountWithCurrencyV(bankAccountNumber, currencyV));

    public BankAccountV BankAccountNumber { get; }
    public CurrencyV CurrencyV { get; }
}

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