
/// <summary>
/// StateMatchine
/// </summary>
public class IState
{
    public object Owner;
    public virtual void OnEnter() { }
    public virtual void OnUpdate() { }
    public virtual void OnExit() { }
}
