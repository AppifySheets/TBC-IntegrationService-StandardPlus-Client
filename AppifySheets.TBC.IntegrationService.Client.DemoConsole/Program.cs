﻿using System.Diagnostics;
using AppifySheets.Immutable.BankIntegrationTypes;
using AppifySheets.TBC.IntegrationService.Client.ApiConfiguration;
using AppifySheets.TBC.IntegrationService.Client.SoapInfrastructure.GetAccountMovements;
using AppifySheets.TBC.IntegrationService.Client.SoapInfrastructure.ImportSinglePaymentOrders;
using AppifySheets.TBC.IntegrationService.Client.TBC_Services;

var credentials = new TBCApiCredentials("Username", "Password");
var tbcSoapCaller = new TBCSoapCaller("certificate.pfx", "CertificatePassword", credentials);

var accountMovements = await Worker.GetDeserialized(new GetAccountMovementsDeserializer(tbcSoapCaller,
    new Period(new DateTime(2023, 9, 1), new DateTime(2023, 9, 26))));

Debugger.Break();

var withinBankGel = await Worker
    .GetDeserialized(new ImportSinglePaymentOrdersDeserializer(tbcSoapCaller,
        new SoapImportSinglePaymentOrders(
            new TransferWithinBankPaymentOrderIo("TEST", "TEST",
                BankAccountWithCurrencyV.Create(new BankAccountV("GE86TB1144836120100002"), CurrencyV.GEL).Value)
            {
                DocumentNumber = 123,
                Amount = 0.01m,
                BeneficiaryName = "TEST",
                SenderAccountWithCurrency = BankAccountWithCurrencyV.Create(new BankAccountV("GE20TB7467945067800004"), CurrencyV.GEL).Value
            })));

var withinBankCurrency = await Worker
    .GetDeserialized(new ImportSinglePaymentOrdersDeserializer(tbcSoapCaller,
        new SoapImportSinglePaymentOrders(
            new TransferWithinBankPaymentOrderIo("TEST", "TEST",
                BankAccountWithCurrencyV.Create(new BankAccountV("GE86TB1144836120100002"), CurrencyV.USD).Value)
            {
                DocumentNumber = 123,
                Amount = 0.01m,
                BeneficiaryName = "TEST",
                SenderAccountWithCurrency = BankAccountWithCurrencyV.Create(new BankAccountV("GE20TB7467945067800004"), CurrencyV.USD).Value
            })));
var toAnotherBankGel = await Worker
    .GetDeserialized(new ImportSinglePaymentOrdersDeserializer(tbcSoapCaller, new SoapImportSinglePaymentOrders(new TransferToOtherBankNationalCurrencyPaymentOrderIo(
        BankAccountWithCurrencyV.Create(new BankAccountV("GE33BG0000000263255500"), CurrencyV.GEL).Value, "TEST", "TEST", "123123123")
    {
        DocumentNumber = 123,
        Amount = 0.01m,
        BeneficiaryName = "TEST",
        SenderAccountWithCurrency = BankAccountWithCurrencyV.Create(new BankAccountV("GE20TB7467945067800004"), CurrencyV.GEL).Value
    })));

var toAnotherBankCurrencyGood = await Worker
    .GetDeserialized(new ImportSinglePaymentOrdersDeserializer(tbcSoapCaller, new SoapImportSinglePaymentOrders(
        new TransferToOtherBankForeignCurrencyPaymentOrderIo("TEST", "TEST",
            "test", "test", "SHA", "TEST",
            BankAccountWithCurrencyV.Create(new BankAccountV("GE33BG0000000263255500"), CurrencyV.USD).Value)
        {
            DocumentNumber = 123,
            Amount = 100m,
            BeneficiaryName = "TEST",
            SenderAccountWithCurrency = BankAccountWithCurrencyV.Create(new BankAccountV("GE47TB7467936170100001"), CurrencyV.USD).Value
        })));

var toAnotherBankCurrencyBad = await Worker
    .GetDeserialized(new ImportSinglePaymentOrdersDeserializer(tbcSoapCaller,
        new SoapImportSinglePaymentOrders(
            new TransferToOtherBankForeignCurrencyPaymentOrderIo("TEST", "TEST",
                "test", "test", "SHA", "TEST",
                BankAccountWithCurrencyV.Create(new BankAccountV("GE33BG0000000263255500"), CurrencyV.USD).Value)
            {
                DocumentNumber = 123,
                Amount = 100m,
                BeneficiaryName = "TEST",
                SenderAccountWithCurrency = BankAccountWithCurrencyV.Create(new BankAccountV("GE20TB7467945067800004"), CurrencyV.USD).Value
            })));

var toTreasury = await Worker
    .GetDeserialized(new ImportSinglePaymentOrdersDeserializer(tbcSoapCaller,
        new SoapImportSinglePaymentOrders(
            new TreasuryTransferPaymentOrderIo(101001000, "TEST")
            {
                DocumentNumber = 123,
                Amount = 0.01m,
                BeneficiaryName = "TEST",
                SenderAccountWithCurrency = BankAccountWithCurrencyV.Create(new BankAccountV("GE31TB7467936080100003"), CurrencyV.GEL).Value
            })));

var allResults = new[] { toTreasury, toAnotherBankCurrencyBad, toAnotherBankCurrencyGood, toAnotherBankGel, withinBankCurrency, withinBankGel };

Debugger.Break();

// var response = await tbcSoapCaller.CallTBCService(new TBCSoapCaller.PerformedAction(TBCSoapCaller.CreateGetAccountsMovementSoapEnvelope(credentials,
//         new Period(new DateTime(2023, 9, 1), new DateTime(2023, 9, 26)), 0),
//     TBCSoapCaller.TBCServiceAction.GetAccountMovements));