using UnityEngine;

public abstract class Amulet : MonoBehaviour
{
    public string amuletName;
    public abstract void ApplyEffect(Player player);
    public abstract void RemoveEffect(Player player);
}

public class StabilityAmulet : Amulet
{
    public override void ApplyEffect(Player player)
    {
        return;
    }
    public override void RemoveEffect(Player player)
    {
        return;
    }
}