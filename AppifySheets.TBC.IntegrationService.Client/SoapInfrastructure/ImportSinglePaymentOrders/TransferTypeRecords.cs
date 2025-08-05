using AppifySheets.Immutable.BankIntegrationTypes;
using JetBrains.Annotations;

namespace AppifySheets.TBC.IntegrationService.Client.SoapInfrastructure.ImportSinglePaymentOrders;

public record TransferWithinBankPaymentOrderIo : TransferTypeRecord, IRecipient, IBeneficiaryName, IDescription
{
    public required BankAccount RecipientAccountWithCurrency { get; init; }
}

public record TransferToOtherBankForeignCurrencyPaymentOrderIo(
    string BeneficiaryAddress,
    string BeneficiaryBankCode,
    string BeneficiaryBankName,
    string ChargeDetails,
    BankAccount RecipientAccountWithCurrency)
    : TransferTypeRecord, IBeneficiaryForCurrencyTransfer, IRecipient, IBeneficiaryName, IDescription;

public record TransferToOtherBankNationalCurrencyPaymentOrderIo(BankAccount RecipientAccountWithCurrency, string BeneficiaryTaxCode)
    : TransferTypeRecord, IBeneficiaryTaxCode, IRecipient, IBeneficiaryName, IDescription;

public record TreasuryTransferPaymentOrderIo(long TreasuryCode) : TransferTypeRecord, ITreasury;

[UsedImplicitly]
public record TransferToOwnAccountPaymentOrderIo(BankAccount RecipientAccountWithCurrency) : TransferTypeRecord, IRecipient, IBeneficiaryName, IDescription;