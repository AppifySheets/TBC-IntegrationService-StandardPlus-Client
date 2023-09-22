using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AppifySheets.Immutable.BankIntegrationTypes;
using AppifySheets.TBC.IntegrationService.Client.TBC_Services;
using CSharpFunctionalExtensions;
using Serilog;

namespace AppifySheets.TBC.IntegrationService.Client.SoapInfrastructure.GetAccountMovements;

public abstract record GetAccountMovementsHelper
{
    public static async Task<Result<IReadOnlyCollection<AccountMovement>>> GetAccountMovement(Period period, TBCSoapCaller tbcSoapCaller)
    {
        Log.Information("TBC - getting data for {Period}", period);

        var tbcServiceResult = await GetData(0);
        if (tbcServiceResult.IsFailure) return tbcServiceResult.ConvertFailure<IReadOnlyCollection<AccountMovement>>();

        var deserializedData = tbcServiceResult.Value;

        Log.Information("TBC - {ToBeReceivedAccountMovementsCount} records are being received", deserializedData.ResultXml!.TotalCount);

        var pagesTotal = Convert.ToInt32(Math.Ceiling((double)deserializedData.ResultXml.TotalCount / deserializedData.ResultXml.Pager.PageSize));

        var accountMovements = deserializedData.AccountMovement;

        for (var i = 1; i < pagesTotal; i++)
        {
            var tbcServiceResult1 = await GetData(i);
            if (tbcServiceResult1.IsFailure) return tbcServiceResult1.ConvertFailure<IReadOnlyCollection<AccountMovement>>();

            var deserializedData1 = tbcServiceResult1.Value;

            accountMovements.AddRange(deserializedData1.AccountMovement);
        }

        if (accountMovements.Count != deserializedData.ResultXml.TotalCount)
            throw new InvalidOperationException($"Received AccountMovements count of [{accountMovements.Count}] differs from expected of [{deserializedData.ResultXml.TotalCount}]");

        Log.Information("TBC - {ReceivedAccountMovementsCount} records have been received", accountMovements.Count);

        return accountMovements;

        async Task<Result<GetAccountMovementsResponseIo>> GetData(int index)
        {
            var result = await tbcSoapCaller.CallTBCServiceAsync(new GetAccountMovementsRequestIo(period, index));
            return result.IsFailure
                ? result.ConvertFailure<GetAccountMovementsResponseIo>()
                : result.Value.DeserializeInto<GetAccountMovementsResponseIo>();
        }
    }

    // protected override RequestSoapGetAccountMovements GetSoapBase() => throw new NotImplementedException();

    // public override Task<Result<IReadOnlyCollection<AccountMovement>>> GetDeserialized() => GetAccountMovement(Period);
}