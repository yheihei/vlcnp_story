namespace VLCNP.Combat
{
    public interface IProjectile
    {
        bool IsStucking { get; }
        
        void SetDirection(bool isLeft);
        void SetDamage(float damage);
        void ImpactAndDestroy();
    }
}