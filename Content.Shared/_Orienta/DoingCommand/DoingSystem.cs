namespace Content.Shared._Orienta.DoingCommand;

public sealed class DoingSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DoingAttemptEvent>(OnDoingAttempt);
    }

    public void SetDoing(EntityUid uid, bool value, DoingComponent? component = null)
    {
        if (value && !Resolve(uid, ref component))
            return;

        component = EnsureComp<DoingComponent>(uid);

        if (component.Enabled == value)
            return;

        Dirty(uid, component);
    }

    private void OnDoingAttempt(DoingAttemptEvent args)
    {
        if (!TryComp(args.Uid, out DoingComponent? doing) || !doing.Enabled)
            args.Cancel();
    }
}
