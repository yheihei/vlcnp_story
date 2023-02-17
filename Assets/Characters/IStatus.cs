public interface IStatus
{
    public int hitPoints { get; set; }
    public int attackPoints { get; set; }
    public int defencePoints { get; set; }
    void addDamage(IStatus status);
}
