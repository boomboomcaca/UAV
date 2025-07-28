namespace Core.Define;

/// <summary>
///     待扩展
/// </summary>
public class CrondDataPort
{
    public delegate bool DelTriggerStarting();

    public event DelTriggerStarting TriggerStarting;

    public void OnTriggerStarting()
    {
        TriggerStarting?.Invoke();
    }
}