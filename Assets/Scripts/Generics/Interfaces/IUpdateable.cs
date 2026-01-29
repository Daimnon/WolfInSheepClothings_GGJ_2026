namespace Generics
{
    public interface IUpdateable
    {
        public IUpdatingParent UpdatingParent { get; set; }
        public void Tick(float deltaTime);
        public void FixedTick(float fixedDeltaTime);
        public void ForceUnregister()
        {
            UpdatingParent.UnregisterUpdateable(this);
        }
        
        public void ForceRegisterBoth()
        {
            UpdatingParent.RegisterUpdateable(this, UpdateType.Both, UpdatePriority.Medium);
        }
    }
}