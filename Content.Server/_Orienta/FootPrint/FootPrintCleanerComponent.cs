using Robust.Shared.Prototypes;

namespace Content.Server._Orienta.FootPrint;
/// <summary>
/// Регистрирует компонент, который отвечает за возможность предмета убирать следы с тайлов.
/// </summary>
[RegisterComponent, Access(typeof(FootPrintSystem))]
public sealed partial class FootPrintCleanerComponent : Component
{
    [DataField]
    public float Range = 1.5f;
}
