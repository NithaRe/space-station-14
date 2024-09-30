using Robust.Shared.Configuration;

namespace Content.Shared._Orienta.OCCVars;

[CVarDefs]

public sealed class OCCVars
{
    public static readonly CVarDef<string> OrienteerApiToken =
    CVarDef.Create("orienteer.api_token", "", CVar.SERVERONLY | CVar.ARCHIVE | CVar.CONFIDENTIAL);

    public static readonly CVarDef<string> OrienteerApiUrl =
    CVarDef.Create("orienteer.api_url", "", CVar.SERVERONLY | CVar.ARCHIVE);
}
