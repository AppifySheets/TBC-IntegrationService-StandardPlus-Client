using System;
using AppifySheets.TBC.IntegrationService.Client.SoapInfrastructure.ImportSinglePaymentOrders;

namespace AppifySheets.TBC.IntegrationService.Client;

public static class Extensions
{
    public static string? Is<T>(this TransferTypeRecord transferType, Func<T, string?> xmlBody)
    {
        return transferType is T t
            ? xmlBody(t)
            : "";
    }
}