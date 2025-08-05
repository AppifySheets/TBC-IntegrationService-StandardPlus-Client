using AppifySheets.Immutable.BankIntegrationTypes;

namespace AppifySheets.TBC.IntegrationService.Client.SoapInfrastructure.ImportSinglePaymentOrders;

public sealed record BankTransferCommonDetails
{
    public required BankAccount SenderAccountWithCurrency { get; init; }
    public required long DocumentNumber { get; init; }
    public required decimal Amount { get; init; }
    public required string BeneficiaryName { get; init; }
    public required string? PersonalNumber { get; init; }
    public required string Description { get; init; }
    public string? AdditionalDescription { get; init; }
}

public abstract record TransferTypeRecord
{
    public required BankTransferCommonDetails BankTransferCommonDetails { get; init; }

    public BankAccount SenderAccountWithCurrency => BankTransferCommonDetails.SenderAccountWithCurrency;
    public long DocumentNumber => BankTransferCommonDetails.DocumentNumber;
    public decimal Amount => BankTransferCommonDetails.Amount;
    public string BeneficiaryName => BankTransferCommonDetails.BeneficiaryName;
    public string? PersonalNumber => BankTransferCommonDetails.PersonalNumber;
    public string Description => BankTransferCommonDetails.Description;
    public string? AdditionalDescription => BankTransferCommonDetails.AdditionalDescription;
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
    public BankAccount RecipientAccountWithCurrency { get; }
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