using Robust.Shared.Serialization;
using Robust.Shared.GameObjects;
using Robust.Shared.ViewVariables;
using Content.Shared.Alert;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server._Orienta.Purchase;

// PR 63 Покупка за ориентики. Нужен пустой компонент для покупок, чтобы определять Ориенталинк. Не забывайте добавить в ориенталинк этот компонент
[RegisterComponent]
public sealed partial class OrientaLinkComponent : Component
{
}
