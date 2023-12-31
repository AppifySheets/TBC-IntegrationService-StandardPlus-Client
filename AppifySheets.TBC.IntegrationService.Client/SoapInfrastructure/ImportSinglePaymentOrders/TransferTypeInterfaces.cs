﻿using AppifySheets.Immutable.BankIntegrationTypes;

namespace AppifySheets.TBC.IntegrationService.Client.SoapInfrastructure.ImportSinglePaymentOrders;

public abstract record TransferTypeRecord
{
    public required BankAccountWithCurrencyV SenderAccountWithCurrency { get; init; }
    public required long DocumentNumber { get; init; }
    public required decimal Amount { get; init; }
    public required string BeneficiaryName { get; init; }
}

public interface IDescription
{
    public string Description { get; }
}

public interface IAdditionalDescription
{
    public string AdditionalDescription { get; }
}

public interface IBeneficiaryName
{
    public string BeneficiaryName { get; }
}

public interface ITreasury
{
    public long TreasuryCode { get; }
}

public interface IRecipient
{
    public BankAccountWithCurrencyV RecipientAccountWithCurrency { get; }
}

public interface IBeneficiaryTaxCode : IBeneficiaryName
{
    public string BeneficiaryTaxCode { get; }
}

public interface IBeneficiaryForCurrencyTransfer : IBeneficiaryName
{
    public string BeneficiaryAddress { get; }
    public string BeneficiaryBankCode { get; }
    public string BeneficiaryBankName { get; }
    // public string IntermediaryBankCode { get; }
    // public string IntermediaryBankName { get; }
    public string ChargeDetails { get; }
}