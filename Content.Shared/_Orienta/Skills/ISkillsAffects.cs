using Content.Shared.FixedPoint;

namespace Content.Shared._Orienta.Skills;
public interface ISkillsAffects
{
    [DataField("skills")]
    public Dictionary<string, FixedPoint2> Skills { get; protected set; }
}
