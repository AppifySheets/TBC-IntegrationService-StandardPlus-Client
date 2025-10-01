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

var ownAccountGEL = BankAccount.Create("GE31TB7467936080100003", "GEL").Value;
var ownAccountUSD = BankAccount.Create("GE47TB7467936170100001", "USD").Value;

var bankTransferCommonDetails = new BankTransferCommonDetails
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
        RecipientAccountWithCurrency = BankAccount.Create("GE86TB1144836120100002", "GEL").Value,
        BankTransferCommonDetails = bankTransferCommonDetails,
        PersonalNumber = null
    }));

var withinBankCurrency = await tbcSoapCaller.GetDeserialized(new ImportSinglePaymentOrdersRequestIo(
    new TransferWithinBankPaymentOrderIo
    {
        BankTransferCommonDetails = bankTransferCommonDetails with
        {
            SenderAccountWithCurrency = ownAccountUSD
        },
        RecipientAccountWithCurrency = BankAccount.Create("GE86TB1144836120100002", "USD").Value,
        PersonalNumber = null
    }));

var toAnotherBankGel = await tbcSoapCaller.GetDeserialized(
    new ImportSinglePaymentOrdersRequestIo(
        new TransferToOtherBankNationalCurrencyPaymentOrderIo(
            BankAccount.Create("GE33BG0000000263255500", "GEL").Value, "123123123")
        {
            BankTransferCommonDetails = bankTransferCommonDetails
        }));

var toAnotherBankCurrencyGood = await tbcSoapCaller.GetDeserialized(
    new ImportSinglePaymentOrdersRequestIo(
        new TransferToOtherBankForeignCurrencyPaymentOrderIo("test", "test", "SHA", "TEST",
            BankAccount.Create("GE33BG0000000263255500", "USD").Value)
        {
            BankTransferCommonDetails = bankTransferCommonDetails with { SenderAccountWithCurrency = ownAccountUSD }
        }));

var toAnotherBankCurrencyBad = await tbcSoapCaller.GetDeserialized(
    new ImportSinglePaymentOrdersRequestIo(
        new TransferToOtherBankForeignCurrencyPaymentOrderIo("test", "test", "SHA", "TEST",
            BankAccount.Create("GE33BG0000000263255500", "USD").Value)
        {
            BankTransferCommonDetails = bankTransferCommonDetails with { SenderAccountWithCurrency = ownAccountUSD }
        }));

var toChina = await tbcSoapCaller.GetDeserialized(
    new ImportSinglePaymentOrdersRequestIo(
        new TransferToOtherBankForeignCurrencyPaymentOrderIo( "China",
            // "ICBKCNBJSZN", "INDUSTRIAL AND COMMERCIAL BANK OF CHINA SHENZHEN BRANCH", "SHA", "Invoice(LZSK202311028)",
            "ICBKCNBJSZN", "INDUSTRIAL AND COMMERCIAL BANK OF CHINA SHENZHEN BRANCH", "SHA",
            BankAccount.Create("4000109819100186641", "USD").Value)
        {
            BankTransferCommonDetails = bankTransferCommonDetails with
            {
                SenderAccountWithCurrency = ownAccountUSD,
                BeneficiaryName = "Shenzhen Shinekoo Supply Chain Co.,Ltd"
            }
        }));

var toTreasury = await tbcSoapCaller.GetDeserialized(
    new ImportSinglePaymentOrdersRequestIo(
        new TreasuryTransferPaymentOrderIo(101001000)
            { BankTransferCommonDetails = bankTransferCommonDetails }));

Debugger.Break();