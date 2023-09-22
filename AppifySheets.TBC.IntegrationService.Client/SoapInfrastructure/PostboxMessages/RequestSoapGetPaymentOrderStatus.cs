using JetBrains.Annotations;

namespace AppifySheets.TBC.IntegrationService.Client.SoapInfrastructure.PostboxMessages;

[UsedImplicitly]
public record GetPostboxMessagesRequestIo(MessageType MessageType)
    : RequestSoap<GetPostboxMessagesResponseIo>
{
    public override string SoapXml()
        => $"""
            <myg:GetPostboxMessagesRequestIo>
            	<myg:messageType>{MessageType}</myg:messageType>
            </myg:GetPostboxMessagesRequestIo>
            """;

    public override TBCServiceAction TBCServiceAction => TBCServiceAction.GetPaymentOrderStatus;
}

public enum MessageType
{
    // ReSharper disable once InconsistentNaming
    SIMPLE_MESSAGE = 0,
    // ReSharper disable once InconsistentNaming
    MOVEMENT_MESSAGE = 1,
}