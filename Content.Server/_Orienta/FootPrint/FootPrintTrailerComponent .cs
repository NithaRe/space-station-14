using Robust.Shared.Prototypes;

namespace Content.Server._Orienta.FootPrint;
/// <summary>
/// Регистрирует компонент, который отвечает за возможность предмета оставлять следы.
/// </summary>
[RegisterComponent, Access(typeof(FootPrintSystem))]
public sealed partial class FootPrintTrailerComponent : Component
{
    /// <summary>
    /// Отпределение типа следа. 
    /// </summary>
    [DataField]
    public FootPrintComponent? holder = null;
}
