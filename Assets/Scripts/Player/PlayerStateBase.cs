public abstract class PlayerStateBase
{
    protected PlayerStateGlobal _global;

    // ── Lifecycle ──────────────────────────────────────────────────────────────
    public void OnEnter(PlayerStateGlobal global)
    {
        _global = global;
        OnStateEnter();
    }

    public void OnExit()
    {
        OnStateExit();
        _global = null;
    }

    protected virtual void OnStateEnter() { }
    protected virtual void OnStateExit()  { }

    // ── Frame loop ─────────────────────────────────────────────────────────────
    public virtual void Update()
    {
        _global.Update();
        CheckSwitchState();
    }

    protected abstract void CheckSwitchState();
}
