using System;
using System.Xml.Linq;
using JetBrains.Annotations;

namespace AppifySheets.TBC.IntegrationService.Client.SoapInfrastructure.ImportSinglePaymentOrders;

[UsedImplicitly]
public record ImportSinglePaymentOrdersRequestIo(TransferTypeRecord TransferType)
    : RequestSoap<ImportSinglePaymentOrdersResponseIo>
{
    public string SoapText => SoapXml().FormatXml();
    public override string SoapXml()
        => $"""
            <myg:ImportSinglePaymentOrdersRequestIo>
                <myg:singlePaymentOrder xsi:type="myg:{TransferType.GetType().Name}">
                    {TransferType.Is<IRecipient>(b =>
                        $"""
                                   <myg:creditAccount>
                                      <myg:accountNumber>{b.RecipientAccountWithCurrency.BankAccountNumber}</myg:accountNumber>
                                      {TransferType.Is<TransferToOwnAccountPaymentOrderIo>(t
                                          => $"<myg:accountCurrencyCode>{b.RecipientAccountWithCurrency.CurrencyV}</myg:accountCurrencyCode>")}
                                   </myg:creditAccount>
                         """)}
                      <myg:debitAccount>
                         <myg:accountNumber>{TransferType.SenderAccountWithCurrency.BankAccountNumber}</myg:accountNumber>
                         <myg:accountCurrencyCode>{TransferType.SenderAccountWithCurrency.CurrencyV}</myg:accountCurrencyCode>
                      </myg:debitAccount>
                      <myg:documentNumber>{TransferType.DocumentNumber}</myg:documentNumber>
                      <myg:amount>
                         <myg:amount>{TransferType.Amount:F2}</myg:amount>
                         <myg:currency>{TransferType.SenderAccountWithCurrency.CurrencyV}</myg:currency>
                      </myg:amount>
                      <myg:position>3</myg:position>
                      <myg:additionalDescription>{TransferType.AdditionalDescription}</myg:additionalDescription>
                      <myg:description>{TransferType.Description}</myg:description>
                      <myg:beneficiaryName>{TransferType.BeneficiaryName}</myg:beneficiaryName>
                      {TransferType.Is<IBeneficiaryTaxCode>(b => $"<myg:beneficiaryTaxCode>{b.BeneficiaryTaxCode}</myg:beneficiaryTaxCode>")}
                      {TransferType.Is<IBeneficiaryForCurrencyTransfer>(b
                          => $"""
                              <myg:beneficiaryAddress>{b.BeneficiaryAddress}</myg:beneficiaryAddress>
                              <myg:beneficiaryBankCode>{b.BeneficiaryBankCode}</myg:beneficiaryBankCode>
                              <myg:beneficiaryBankName>{b.BeneficiaryBankName}</myg:beneficiaryBankName>
                              <myg:intermediaryBankCode />
                              <myg:intermediaryBankName />
                              <myg:chargeDetails>{b.ChargeDetails}</myg:chargeDetails>
                              """)}
                      {TransferType.Is<ITreasury>(b => $"<myg:treasuryCode>{b.TreasuryCode}</myg:treasuryCode>")}
                </myg:singlePaymentOrder>
            </myg:ImportSinglePaymentOrdersRequestIo>
            """.FormatXml();

    public override TBCServiceAction TBCServiceAction => TBCServiceAction.ImportSinglePaymentOrders;
}

public static class UseExtensions
{
    public static TY Use<T, TY>(this T t, Func<T, TY> tTy) => tTy(t);
    
    public static string FormatXml(this string xml)
    {
        try
        {
            var doc = XDocument.Parse(xml);
            return doc.ToString();
        }
        catch (Exception)
        {
            // Handle and throw if fatal exception here; don't just ignore them
            return xml;
        }
    }
}