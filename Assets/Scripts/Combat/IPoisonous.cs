namespace VLCNP.Combat
{
    public interface IPoisonous
    {
        bool IsPoisonous { get; }
        float GetPoisonDamage();
        float GetPoisonDuration();
        float GetPoisonInterval();
    }
}