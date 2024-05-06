namespace VLCNP.Combat.EnemyAction
{
    public interface IEnemyAction
    {
        public void Execute();
        public bool IsExecuting { get; set; }
        public bool IsDone { get; set; }
        public uint Priority { get; }
        public void Reset();
    }
}
