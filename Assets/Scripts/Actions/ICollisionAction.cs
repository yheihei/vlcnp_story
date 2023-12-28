namespace VLCNP.Actions
{
    public interface ICollisionAction
    {
        public bool IsAction { get; set; }
        public bool IsCollisionStart();
        public void Execute();
        public void ExecuteCollisionStart();
        public void ShowInformation();
        public void HideInformation();
    }
}
