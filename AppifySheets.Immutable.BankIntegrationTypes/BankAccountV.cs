using System;
using System.Text.RegularExpressions;

namespace AppifySheets.Immutable.BankIntegrationTypes;

// ReSharper disable once InconsistentNaming
public record BankAccountV
{
    public BankAccountV(string accountNumber)
    {
        const string pattern2Match = @"(^[a-zA-Z]{2}\d{2}[a-zA-Z]{2}\d{16})(\w{3})?$";

        var iban = Regex.Match(accountNumber, pattern2Match).Groups[1].Value;
	
        // if(string.IsNullOrEmpty(iban)) throw new InvalidOperationException($"Account#: [{accountNumber}] doesn't seem to be in an IBAN format!");
        
        AccountNumber = accountNumber;
    }

    public string AccountNumber { get; }

    public override string ToString() => AccountNumber;
}