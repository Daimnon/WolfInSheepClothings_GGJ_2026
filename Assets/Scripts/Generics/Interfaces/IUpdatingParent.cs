using System;

namespace Generics
{
    public interface IUpdatingParent
    {
        bool RegisterUpdateable(IUpdateable updateable, UpdateType updateType, UpdatePriority updatePriority);
        void UnregisterUpdateable(IUpdateable updateable);
        void OnUpdateFailed(IUpdateable updateable);
    }

    [Flags]
    public enum UpdateType
    {
        None = 0,
        Regular = 1 << 0,
        Fixed = 1 << 1,
        Both = Regular | Fixed
    }

    public enum UpdatePriority
    {
        Low = 0,
        Medium = 1,
        High = 2
    }
}