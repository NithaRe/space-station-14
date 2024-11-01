using Content.Shared.FixedPoint;

namespace Content.Shared._Orienta.Skills;

[RegisterComponent]
public sealed partial class ShotRequiresSkillsComponent : Component, IRequiresSkills
{
    [DataField("skills")]
    public Dictionary<string, FixedPoint2> Skills { get; set; } = new()
    {
        { "Guns", 0.5f },
    };
}
