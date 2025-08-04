using AppifySheets.Immutable.BankIntegrationTypes;

namespace AppifySheets.TBC.IntegrationService.Client.SoapInfrastructure.ImportSinglePaymentOrders;

public sealed record TransferTypeRecordSpecific
{
    public required BankAccountWithCurrencyV SenderAccountWithCurrency { get; init; }
    public required long DocumentNumber { get; init; }
    public required decimal Amount { get; init; }
    public required string BeneficiaryName { get; init; }
    public required string? PersonalNumber { get; init; }
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
    public string? PersonalNumber => TransferTypeRecordSpecific.PersonalNumber;
    public string Description => TransferTypeRecordSpecific.Description;
    public string? AdditionalDescription => TransferTypeRecordSpecific.AdditionalDescription;
}

public interface IBeneficiaryName
{
    public string BeneficiaryName { get; }
    public string? PersonalNumber { get; }
}

public interface IDescription
{
    public string Description { get; }
}

public interface ITreasury
{
    public long TreasuryCode { get; }
}

public interface IRecipient
{
    public BankAccountWithCurrencyV RecipientAccountWithCurrency { get; }
}

public interface IBeneficiaryTaxCode
{
    public string BeneficiaryTaxCode { get; }
}

public interface IBeneficiaryForCurrencyTransfer
{
    public string BeneficiaryAddress { get; }
    public string BeneficiaryBankCode { get; }
    public string BeneficiaryBankName { get; }
    public string ChargeDetails { get; }
}