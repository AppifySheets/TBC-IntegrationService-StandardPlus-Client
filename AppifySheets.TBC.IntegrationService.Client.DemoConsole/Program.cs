using System.Diagnostics;
using AppifySheets.Immutable.BankIntegrationTypes;
using AppifySheets.TBC.IntegrationService.Client.ApiConfiguration;
using AppifySheets.TBC.IntegrationService.Client.SoapInfrastructure.GetAccountMovements;
using AppifySheets.TBC.IntegrationService.Client.SoapInfrastructure.GetPaymentOrderStatus;
using AppifySheets.TBC.IntegrationService.Client.SoapInfrastructure.ImportSinglePaymentOrders;
using AppifySheets.TBC.IntegrationService.Client.TBC_Services;

var credentials = new TBCApiCredentials("Username", "Password");
var tbcApiCredentialsWithCertificate = new TBCApiCredentialsWithCertificate(credentials, "TBCIntegrationService.pfx", "CertificatePassword");

var tbcSoapCaller = new TBCSoapCaller(tbcApiCredentialsWithCertificate);

var accountMovements =
    await GetAccountMovementsHelper.GetAccountMovementAsync(new Period(new DateTime(2023, 9, 1), new DateTime(2023, 9, 26)), tbcSoapCaller);

// return;

// var checkStatus = await Worker
//     .GetDeserialized(new SoapBaseWithDeserializer<GetPaymentOrderStatusResponseIo>(tbcSoapCaller)
//     {
//         RequestSoap = new RequestSoapGetPaymentOrderStatus(1632027071)
//     });

var checkStatus2 = await tbcSoapCaller.GetDeserialized(new GetPaymentOrderStatusRequestIo(1632027071));

var ownAccountGEL = new BankAccountWithCurrencyV(new BankAccountV("GE31TB7467936080100003"), CurrencyV.GEL);
var ownAccountUSD = new BankAccountWithCurrencyV(new BankAccountV("GE47TB7467936170100001"), CurrencyV.USD);

var transferTypeRecordSpecific = new TransferTypeRecordSpecific
{
    DocumentNumber = 123,
    Amount = 0.01m,
    BeneficiaryName = "TEST",
    SenderAccountWithCurrency = ownAccountGEL,
    Description = "TEST",
    PersonalNumber = null // Adding required PersonalNumber field
};

var withinBankGel2 = await tbcSoapCaller.GetDeserialized(new ImportSinglePaymentOrdersRequestIo(
    new TransferWithinBankPaymentOrderIo
    {
        RecipientAccountWithCurrency = new BankAccountWithCurrencyV(new BankAccountV("GE86TB1144836120100002"), CurrencyV.GEL),
        TransferTypeRecordSpecific = transferTypeRecordSpecific
    }));

var withinBankCurrency = await tbcSoapCaller.GetDeserialized(new ImportSinglePaymentOrdersRequestIo(
    new TransferWithinBankPaymentOrderIo
    {
        TransferTypeRecordSpecific = transferTypeRecordSpecific with
        {
            SenderAccountWithCurrency = ownAccountUSD
        },
        RecipientAccountWithCurrency = new BankAccountWithCurrencyV(new BankAccountV("GE86TB1144836120100002"), CurrencyV.USD),
    }));

var toAnotherBankGel = await tbcSoapCaller.GetDeserialized(
    new ImportSinglePaymentOrdersRequestIo(
        new TransferToOtherBankNationalCurrencyPaymentOrderIo(
            new BankAccountWithCurrencyV(new BankAccountV("GE33BG0000000263255500"), CurrencyV.GEL), "123123123")
        {
            TransferTypeRecordSpecific = transferTypeRecordSpecific
        }));

var toAnotherBankCurrencyGood = await tbcSoapCaller.GetDeserialized(
    new ImportSinglePaymentOrdersRequestIo(
        new TransferToOtherBankForeignCurrencyPaymentOrderIo("test", "test", "SHA", "TEST",
            new BankAccountWithCurrencyV(new BankAccountV("GE33BG0000000263255500"), CurrencyV.USD))
        {
            TransferTypeRecordSpecific = transferTypeRecordSpecific with { SenderAccountWithCurrency = ownAccountUSD }
        }));

var toAnotherBankCurrencyBad = await tbcSoapCaller.GetDeserialized(
    new ImportSinglePaymentOrdersRequestIo(
        new TransferToOtherBankForeignCurrencyPaymentOrderIo("test", "test", "SHA", "TEST",
            new BankAccountWithCurrencyV(new BankAccountV("GE33BG0000000263255500"), CurrencyV.USD))
        {
            TransferTypeRecordSpecific = transferTypeRecordSpecific with { SenderAccountWithCurrency = ownAccountUSD }
        }));

var toChina = await tbcSoapCaller.GetDeserialized(
    new ImportSinglePaymentOrdersRequestIo(
        new TransferToOtherBankForeignCurrencyPaymentOrderIo( "China",
            // "ICBKCNBJSZN", "INDUSTRIAL AND COMMERCIAL BANK OF CHINA SHENZHEN BRANCH", "SHA", "Invoice(LZSK202311028)",
            "ICBKCNBJSZN", "INDUSTRIAL AND COMMERCIAL BANK OF CHINA SHENZHEN BRANCH", "SHA",
            new BankAccountWithCurrencyV(new BankAccountV("4000109819100186641"), CurrencyV.USD))
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

Debugger.Break();