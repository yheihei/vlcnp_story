using Fungus;

namespace VLCNP.Actions
{
    public interface ICollisionAction
    {
        public bool IsAction { get; set; }
        public void Execute();
    }
}
