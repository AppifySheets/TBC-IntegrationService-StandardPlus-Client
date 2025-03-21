﻿using JetBrains.Annotations;

namespace AppifySheets.TBC.IntegrationService.Client.SoapInfrastructure.PasswordChangeRelated;

[UsedImplicitly]
public record ChangePasswordRequestIo(string NewPassword, string DigipassCode) : RequestSoap<ChangePasswordResponseIo>()
{
    public override string SoapXml()
        => $"""
            <myg:ChangePasswordRequestIo>
               <myg:newPassword>{NewPassword}</myg:newPassword>
             </myg:ChangePasswordRequestIo>
            """;

    public override string Nonce => DigipassCode;
    public override TBCServiceAction TBCServiceAction => TBCServiceAction.ChangePassword;
}