using Robust.Shared.Configuration;

namespace Content.Shared._Orienta.OCCVars;

// ReSharper disable once InconsistentNaming
[CVarDefs]
public sealed class OCCVars
{
    public static readonly CVarDef<string> OrienteerApiToken =
    CVarDef.Create("orienteer.api_token", "", CVar.SERVERONLY | CVar.ARCHIVE);

    public static readonly CVarDef<string> OrienteerApiUrl =
    CVarDef.Create("orienteer.api_url", "", CVar.SERVERONLY | CVar.ARCHIVE);

    public static readonly CVarDef<bool> AutoGetUp =
    CVarDef.Create("laying.auto_get_up", true, CVar.CLIENT | CVar.ARCHIVE | CVar.REPLICATED);
}
