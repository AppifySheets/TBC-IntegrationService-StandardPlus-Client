using AppifySheets.Immutable.BankIntegrationTypes;
using JetBrains.Annotations;

namespace AppifySheets.TBC.IntegrationService.Client.SoapInfrastructure.GetAccountMovements;

[UsedImplicitly]
public record GetAccountMovementsRequestIo(Period Period, int PageIndex)
    : RequestSoap<GetAccountMovementsResponseIo>
{
    public override string SoapXml()
        => $"""
            <myg:GetAccountMovementsRequestIo>
                <myg:accountMovementFilterIo>
                    <myg:pager>
                        <myg:pageIndex>{PageIndex}</myg:pageIndex>
                        <myg:pageSize>700</myg:pageSize>
                    </myg:pager>
                    <myg:periodFrom>{Period.From:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}</myg:periodFrom>
                    <myg:periodTo>{Period.Till:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}</myg:periodTo>
                </myg:accountMovementFilterIo>
            </myg:GetAccountMovementsRequestIo>
            """;

    public override TBCServiceAction TBCServiceAction => TBCServiceAction.GetAccountMovements;
}