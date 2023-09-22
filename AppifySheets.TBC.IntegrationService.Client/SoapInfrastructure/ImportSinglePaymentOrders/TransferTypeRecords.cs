using AppifySheets.Immutable.BankIntegrationTypes;
using JetBrains.Annotations;

namespace AppifySheets.TBC.IntegrationService.Client.SoapInfrastructure.ImportSinglePaymentOrders;

public record TransferWithinBankPaymentOrderIo : TransferTypeRecord, IRecipient
{
    public required BankAccountWithCurrencyV RecipientAccountWithCurrency { get; init; }
}

public record TransferToOtherBankForeignCurrencyPaymentOrderIo(
    string BeneficiaryAddress,
    string BeneficiaryBankCode,
    string BeneficiaryBankName,
    string ChargeDetails,
    BankAccountWithCurrencyV RecipientAccountWithCurrency)
    : TransferTypeRecord, IRecipient, IBeneficiaryForCurrencyTransfer;

public record TransferToOtherBankNationalCurrencyPaymentOrderIo(BankAccountWithCurrencyV RecipientAccountWithCurrency, string BeneficiaryTaxCode)
    : TransferTypeRecord, IRecipient, IBeneficiaryTaxCode;

public record TreasuryTransferPaymentOrderIo(long TreasuryCode)
    : TransferTypeRecord, ITreasury;

[UsedImplicitly]
public record TransferToOwnAccountPaymentOrderIo(BankAccountWithCurrencyV RecipientAccountWithCurrency, string Description) : TransferTypeRecord, IRecipient;