using AppifySheets.Immutable.BankIntegrationTypes;
using AppifySheets.TBC.IntegrationService.Client.ApiConfiguration;
using AppifySheets.TBC.IntegrationService.Client.SoapInfrastructure.GetAccountMovements;
using AppifySheets.TBC.IntegrationService.Client.SoapInfrastructure.GetPaymentOrderStatus;
using AppifySheets.TBC.IntegrationService.Client.SoapInfrastructure.ImportSinglePaymentOrders;
using AppifySheets.TBC.IntegrationService.Client.SoapInfrastructure.PostboxMessages;
using AppifySheets.TBC.IntegrationService.Client.TBC_Services;
using Shouldly;
using Xunit;

namespace AppifySheets.TBC.IntegrationService.Tests;

// https://developers.tbcbank.ge/docs/dbi-overview
public class TBCSoapCallerTests
{
    readonly TBCSoapCaller _tbcSoapCaller;

    public TBCSoapCallerTests()
    {
        var credentials = new TBCApiCredentials("API_USERNAME", "API_PASSWORD");
        var tbcApiCredentialsWithCertificate = new TBCApiCredentialsWithCertificate(credentials, "CERTIFICATE_FILE_NAME.pfx", "CERTIFICATE_PASSWORD");

        _tbcSoapCaller = new TBCSoapCaller(tbcApiCredentialsWithCertificate);
    }

    [Fact]
    public async Task AccountMovements_returns_values()
    {
        var accountMovements =
            await GetAccountMovementsHelper.GetAccountMovement(new Period(new DateTime(2023, 9, 1), new DateTime(2023, 9, 26)), _tbcSoapCaller);

        accountMovements.IsSuccess.ShouldBeTrue();
        accountMovements.Value.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task RequestSoapGetPaymentOrderStatus_should_return_values()
    {
        var checkStatus2 = await _tbcSoapCaller.GetDeserialized(new GetPaymentOrderStatusRequestIo(1632027071));
        checkStatus2.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task PostboxMessagesRequestSoapGetPaymentOrderStatus_should_return_values()
    {
        var checkStatus2 = await _tbcSoapCaller.GetDeserialized(new GetPostboxMessagesRequestIo(MessageType.MOVEMENT_MESSAGE));
        checkStatus2.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ImportSinglePaymentOrders_returns_values()
    {
        var ownAccountGEL = BankAccountWithCurrencyV.Create(new BankAccountV("GE31TB7467936080100003"), CurrencyV.GEL).Value;
        var ownAccountUSD = BankAccountWithCurrencyV.Create(new BankAccountV("GE47TB7467936170100001"), CurrencyV.USD).Value;

        var transferTypeRecordSpecific = new TransferTypeRecordSpecific
        {
            DocumentNumber = 63865984018636,
            Amount = 0.01m,
            BeneficiaryName = "TEST",
            SenderAccountWithCurrency = ownAccountGEL,
            Description = "TEST"
        };

        var withinBankGel2 = await _tbcSoapCaller.GetDeserialized(new ImportSinglePaymentOrdersRequestIo(
            new TransferWithinBankPaymentOrderIo
            {
                RecipientAccountWithCurrency = BankAccountWithCurrencyV.Create(new BankAccountV("GE24TB7755145063300001"), CurrencyV.GEL).Value,
                TransferTypeRecordSpecific = transferTypeRecordSpecific
            }));

        var withinBankCurrency = await _tbcSoapCaller.GetDeserialized(new ImportSinglePaymentOrdersRequestIo(
            new TransferWithinBankPaymentOrderIo
            {
                TransferTypeRecordSpecific = transferTypeRecordSpecific with
                {
                    SenderAccountWithCurrency = ownAccountUSD
                },
                RecipientAccountWithCurrency = BankAccountWithCurrencyV.Create(new BankAccountV("GE86TB1144836120100002"), CurrencyV.USD).Value,
            }));

        var toAnotherBankGel = await _tbcSoapCaller.GetDeserialized(
            new ImportSinglePaymentOrdersRequestIo(
                new TransferToOtherBankNationalCurrencyPaymentOrderIo(
                    BankAccountWithCurrencyV.Create(new BankAccountV("GE33BG0000000263255500"), CurrencyV.GEL).Value, "123123123")
                {
                    TransferTypeRecordSpecific = transferTypeRecordSpecific
                }));

        var toAnotherBankCurrencyGood = await _tbcSoapCaller.GetDeserialized(
            new ImportSinglePaymentOrdersRequestIo(
                new TransferToOtherBankForeignCurrencyPaymentOrderIo(
                    "test", "test", "SHA", "TEST",
                    BankAccountWithCurrencyV.Create(new BankAccountV("GE33BG0000000263255500"), CurrencyV.USD).Value)
                {
                    TransferTypeRecordSpecific = transferTypeRecordSpecific with { SenderAccountWithCurrency = ownAccountUSD }
                }));

        var toAnotherBankCurrencyBad = await _tbcSoapCaller.GetDeserialized(
            new ImportSinglePaymentOrdersRequestIo(
                new TransferToOtherBankForeignCurrencyPaymentOrderIo("test", "test", "SHA", "TEST",
                    BankAccountWithCurrencyV.Create(new BankAccountV("GE33BG0000000263255500"), CurrencyV.USD).Value)
                {
                    TransferTypeRecordSpecific = transferTypeRecordSpecific with { SenderAccountWithCurrency = ownAccountUSD }
                }));

        var toChina = await _tbcSoapCaller.GetDeserialized(
            new ImportSinglePaymentOrdersRequestIo(
                new TransferToOtherBankForeignCurrencyPaymentOrderIo("China",
                    // "ICBKCNBJSZN", "INDUSTRIAL AND COMMERCIAL BANK OF CHINA SHENZHEN BRANCH", "SHA", "Invoice(LZSK202311028)",
                    "ICBKCNBJSZN", "INDUSTRIAL AND COMMERCIAL BANK OF CHINA SHENZHEN BRANCH", "SHA",
                    BankAccountWithCurrencyV.Create(new BankAccountV("4000109819100186641"), CurrencyV.USD).Value)
                {
                    TransferTypeRecordSpecific = transferTypeRecordSpecific with
                    {
                        SenderAccountWithCurrency = ownAccountUSD,
                        BeneficiaryName = "Shenzhen Shinekoo Supply Chain Co.,Ltd"
                    }
                }));

        var toTreasury = await _tbcSoapCaller.GetDeserialized(
            new ImportSinglePaymentOrdersRequestIo(
                new TreasuryTransferPaymentOrderIo(101001000)
                    { TransferTypeRecordSpecific = transferTypeRecordSpecific }));
    }
}