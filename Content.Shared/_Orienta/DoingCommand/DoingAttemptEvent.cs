namespace Content.Shared._Orienta.DoingCommand;

public sealed class DoingAttemptEvent : CancellableEntityEventArgs
{
    public DoingAttemptEvent(EntityUid uid)
    {
        Uid = uid;
    }

    public EntityUid Uid { get; }
}
