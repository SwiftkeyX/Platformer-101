public abstract class PlayerStateBase
{
    public virtual void OnEnter(PlayerBlackboard board) { }

    public virtual void Update(PlayerBlackboard board)
    {
        board.Update();
        CheckSwitchState(board);
    }

    protected abstract void CheckSwitchState(PlayerBlackboard board);

    public virtual void OnExit(PlayerBlackboard board) { }
}
