﻿using System.Xml;

namespace AppifySheets.TBC.IntegrationService.Client;

public record PerformedActionSoapEnvelope(XmlDocument Document, PerformedActionSoapEnvelope.TBCServiceAction Action)
{
    public override string ToString() => Action.ToString();

    public enum TBCServiceAction
    {
        GetAccountMovements,
        ImportSinglePaymentOrders,
        GetPaymentOrderStatus,
        ChangePassword
    }
}