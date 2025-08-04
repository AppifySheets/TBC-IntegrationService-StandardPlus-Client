# TBC Bank IntegrationService Standard+ C#/net8 Client
## Why - official implementation is error-prone, easy-to-mess-up and requires a lot of manual fixing
### Current library attempts to wrap soap calls in immutable, type-safe way

#### You will require 4 things from the TBC Bank - 1) `.pfx` certificate, 2) `Username`, 3) `Password` and 4) `certificate_password`

Service Documentation by the TBC Bank is here - https://developers.tbcbank.ge/docs/dbi-overview

## Following services are implemented:
* [Import Single Payment Orders](https://developers.tbcbank.ge/docs/import-single-payments) - Execute various types of payment transfers
* [Get Account Movements](https://developers.tbcbank.ge/docs/account-movement) - Retrieve account transaction history
* [Get Payment Order Status](https://developers.tbcbank.ge/docs/get-payment-order-status) - Check status of submitted payment orders
* [Change Password](https://developers.tbcbank.ge/docs/change-password) - Change API user password

### Usage
See the [Demo](AppifySheets.TBC.IntegrationService.Client.DemoConsole/Program.cs)

```csharp
var credentials = new TBCApiCredentials("Username", "Password"); // Obtain API Credentials & Certificate with password from the Bank/Banker
var tbcApiCredentialsWithCertificate = new TBCApiCredentialsWithCertificate(credentials, "TBCIntegrationService.pfx", "certificate_password");

var tbcSoapCaller = new TBCSoapCaller(tbcApiCredentialsWithCertificate);

var accountMovements =
    await GetAccountMovementsHelper.GetAccountMovement(new Period(new DateTime(2023, 9, 1), new DateTime(2023, 9, 26)), tbcSoapCaller);

var checkStatus = await tbcSoapCaller.GetDeserialized(new GetPaymentOrderStatusRequestIo(1632027071));

// Example IBAN format: GE00TB0000000000000000
var ownAccountGEL = BankAccountWithCurrencyV.Create(new BankAccountV("GE00TB0000000000000001"), CurrencyV.GEL).Value;
var ownAccountUSD = BankAccountWithCurrencyV.Create(new BankAccountV("GE00TB0000000000000002"), CurrencyV.USD).Value;

var transferTypeRecordSpecific = new TransferTypeRecordSpecific
{
    DocumentNumber = 123,
    Amount = 0.01m,
    BeneficiaryName = "TEST",
    SenderAccountWithCurrency = ownAccountGEL,
    Description = "TEST"
};

var withinBankGel2 = await tbcSoapCaller.GetDeserialized(new ImportSinglePaymentOrdersRequestIo(
    new TransferWithinBankPaymentOrderIo
    {
        RecipientAccountWithCurrency = BankAccountWithCurrencyV.Create(new BankAccountV("GE00TB0000000000000003"), CurrencyV.GEL).Value,
        TransferTypeRecordSpecific = transferTypeRecordSpecific
    }));

var withinBankCurrency = await tbcSoapCaller.GetDeserialized(new ImportSinglePaymentOrdersRequestIo(
    new TransferWithinBankPaymentOrderIo
    {
        TransferTypeRecordSpecific = transferTypeRecordSpecific with
        {
            SenderAccountWithCurrency = ownAccountUSD
        },
        RecipientAccountWithCurrency = BankAccountWithCurrencyV.Create(new BankAccountV("GE00TB0000000000000004"), CurrencyV.USD).Value,
    }));

var toAnotherBankGel = await tbcSoapCaller.GetDeserialized(
    new ImportSinglePaymentOrdersRequestIo(
        new TransferToOtherBankNationalCurrencyPaymentOrderIo(
            BankAccountWithCurrencyV.Create(new BankAccountV("GE00BG0000000000000001"), CurrencyV.GEL).Value, "123456789")
        {
            TransferTypeRecordSpecific = transferTypeRecordSpecific
        }));

var toAnotherBankCurrencyGood = await tbcSoapCaller.GetDeserialized(
    new ImportSinglePaymentOrdersRequestIo(
        new TransferToOtherBankForeignCurrencyPaymentOrderIo("test", "test", "SHA", "TEST",
            BankAccountWithCurrencyV.Create(new BankAccountV("GE00BG0000000000000002"), CurrencyV.USD).Value)
        {
            TransferTypeRecordSpecific = transferTypeRecordSpecific with { SenderAccountWithCurrency = ownAccountUSD }
        }));

var toAnotherBankCurrencyBad = await tbcSoapCaller.GetDeserialized(
    new ImportSinglePaymentOrdersRequestIo(
        new TransferToOtherBankForeignCurrencyPaymentOrderIo("test", "test", "SHA", "TEST",
            BankAccountWithCurrencyV.Create(new BankAccountV("GE00BG0000000000000002"), CurrencyV.USD).Value)
        {
            TransferTypeRecordSpecific = transferTypeRecordSpecific with { SenderAccountWithCurrency = ownAccountUSD }
        }));

var toChina = await tbcSoapCaller.GetDeserialized(
    new ImportSinglePaymentOrdersRequestIo(
        new TransferToOtherBankForeignCurrencyPaymentOrderIo( "China",
            // "ICBKCNBJSZN", "INDUSTRIAL AND COMMERCIAL BANK OF CHINA SHENZHEN BRANCH", "SHA", "Invoice(LZSK202311028)",
            "ICBKCNBJSZN", "INDUSTRIAL AND COMMERCIAL BANK OF CHINA SHENZHEN BRANCH", "SHA",
            BankAccountWithCurrencyV.Create(new BankAccountV("CN0000000000000000001"), CurrencyV.USD).Value)
        {
            TransferTypeRecordSpecific = transferTypeRecordSpecific with
            {
                SenderAccountWithCurrency = ownAccountUSD,
                BeneficiaryName = "Shenzhen Shinekoo Supply Chain Co.,Ltd"
            }
        }));

var toTreasury = await tbcSoapCaller.GetDeserialized(
    new ImportSinglePaymentOrdersRequestIo(
        new TreasuryTransferPaymentOrderIo(101001000)
            { TransferTypeRecordSpecific = transferTypeRecordSpecific }));
```
