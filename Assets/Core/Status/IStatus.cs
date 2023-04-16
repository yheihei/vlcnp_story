public interface IStatus
{
    public int HitPoints { get; set; }
    public int MaxHitPoints { get; set; }
    public int AttackPoints { get; set; }
    public int DefencePoints { get; set; }
    void AddDamage(IStatus status);
}
