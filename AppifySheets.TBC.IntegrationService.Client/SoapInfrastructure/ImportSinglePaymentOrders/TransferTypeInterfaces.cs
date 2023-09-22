using AppifySheets.Immutable.BankIntegrationTypes;

namespace AppifySheets.TBC.IntegrationService.Client.SoapInfrastructure.ImportSinglePaymentOrders;

public sealed record TransferTypeRecordSpecific
{
    public required BankAccountWithCurrencyV SenderAccountWithCurrency { get; init; }
    public required long DocumentNumber { get; init; }
    public required decimal Amount { get; init; }
    public required string BeneficiaryName { get; init; }
    public required string Description { get; init; }
    public string? AdditionalDescription { get; init; }
}
public abstract record TransferTypeRecord
{
    public required TransferTypeRecordSpecific TransferTypeRecordSpecific { get; init; }

    public BankAccountWithCurrencyV SenderAccountWithCurrency => TransferTypeRecordSpecific.SenderAccountWithCurrency;
    public long DocumentNumber => TransferTypeRecordSpecific.DocumentNumber;
    public decimal Amount => TransferTypeRecordSpecific.Amount;
    public string BeneficiaryName => TransferTypeRecordSpecific.BeneficiaryName;
    public string Description => TransferTypeRecordSpecific.Description;
    public string? AdditionalDescription => TransferTypeRecordSpecific.AdditionalDescription;
}

// public interface IDescription
// {
//     public string? Description { get; }
// }

// public interface IAdditionalDescription
// {
//     public string? AdditionalDescription { get; }
// }

// public interface IBeneficiaryName
// {
//     public string BeneficiaryName { get; }
// }

public interface ITreasury
{
    public long TreasuryCode { get; }
}

public interface IRecipient
{
    public BankAccountWithCurrencyV RecipientAccountWithCurrency { get; }
}

public interface IBeneficiaryTaxCode //: IBeneficiaryName
{
    public string BeneficiaryTaxCode { get; }
}

public interface IBeneficiaryForCurrencyTransfer // : IBeneficiaryName
{
    public string BeneficiaryAddress { get; }
    public string BeneficiaryBankCode { get; }
    public string BeneficiaryBankName { get; }
    // public string IntermediaryBankCode { get; }
    // public string IntermediaryBankName { get; }
    public string ChargeDetails { get; }
}