namespace VLCNP.Actions
{
    public interface ICollisionAction
    {
        public bool IsAction { get; set; }
        public bool IsAutoStart();
        public void Execute();
        public void ShowInformation();
        public void HideInformation();
    }
}
