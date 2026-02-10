
public interface IActivatable
{
    public void OnActivate();
    public void OnDeactivate();

    public bool IsActive();

    public bool MatchActivationGroup(UnityEngine.Color group);
}
