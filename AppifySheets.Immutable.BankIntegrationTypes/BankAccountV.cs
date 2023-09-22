using System;
using System.Text.RegularExpressions;

namespace AppifySheets.Immutable.BankIntegrationTypes;

// ReSharper disable once InconsistentNaming
public record BankAccountV
{
    public BankAccountV(string accountNumber)
    {
        const string pattern2Match = @"[a-zA-Z]{2}\d{2}[a-zA-Z]{2}\d{16}";

        // if (!Regex.IsMatch(accountNumber, pattern2Match))
        //     throw new InvalidOperationException($"Account#: [{accountNumber}] doesn't seem to be in an IBAN format!");

        // var bankAccountNumberWithoutCurrency = Regex.Match(accountNumber, pattern2Match);

        AccountNumber = accountNumber;//bankAccountNumberWithoutCurrency.Groups[0].Value;
    }

    public string AccountNumber { get; }

    public override string ToString() => AccountNumber;
}