using System.Collections;
using System.Collections.Generic;
using Generics;
using UnityEngine;

namespace Singleton
{
    public class UpdatingManager : MonoBehaviour, IUpdatingParent
    {
        private struct Entry
        {
            public IUpdateable Updateable;
            public UpdateType Type;
            public UpdatePriority Priority;
        }

        public static UpdatingManager Instance { get; private set; }

        readonly List<Entry> entries = new();
        Entry[] snapshotCache = new Entry[32];
        bool needsSort = false;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public bool RegisterUpdateable(IUpdateable updateable, UpdateType updateType, UpdatePriority updatePriority)
        {
            if (updateable == null) return false;

            UnregisterUpdateable(updateable);
            updateable.UpdatingParent = this;
            entries.Add(new Entry { Updateable = updateable, Type = updateType, Priority = updatePriority });
            needsSort = true;
            return true;
        }

        public void UnregisterUpdateable(IUpdateable updateable)
        {
            if (updateable == null) return;

            for (var i = entries.Count - 1; i >= 0; i--)
            {
                if (ReferenceEquals(entries[i].Updateable, updateable))
                {
                    entries.RemoveAt(i);
                    break;
                }
            }
        }

        public void OnUpdateFailed(IUpdateable updateable)
        {
#if UNITY_EDITOR
            Debug.LogError($"UpdatingManager: update failed for {updateable?.GetType().Name}. Unregistering.");
#endif
            UnregisterUpdateable(updateable);
        }

        void EnsureSnapshotCapacity()
        {
            if (entries.Count > snapshotCache.Length)
            {
                snapshotCache = new Entry[entries.Count * 2];
            }
        }

        void SortIfNeeded()
        {
            if (needsSort)
            {
                entries.Sort((a, b) => b.Priority.CompareTo(a.Priority));
                needsSort = false;
            }
        }

        void ProcessUpdates(UpdateType targetType, float deltaTime)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                var e = snapshotCache[i];
                if ((e.Type & targetType) == 0) continue;

                try
                {
                    if (targetType == UpdateType.Regular)
                        e.Updateable.Tick(deltaTime);
                    else
                        e.Updateable.FixedTick(deltaTime);
                }
                catch
                {
                    OnUpdateFailed(e.Updateable);
                }
            }
        }

        void Update()
        {
            SortIfNeeded();
            EnsureSnapshotCapacity();
            entries.CopyTo(snapshotCache);
            ProcessUpdates(UpdateType.Regular, Time.deltaTime);
        }

        void FixedUpdate()
        {
            SortIfNeeded();
            EnsureSnapshotCapacity();
            entries.CopyTo(snapshotCache);
            ProcessUpdates(UpdateType.Fixed, Time.fixedDeltaTime);
        }
        
        // Coroutines
        public Coroutine RunCoroutine(IEnumerator routine)
        {
            return StartCoroutine(routine);
        }
    
        public void StopManagedCoroutine(Coroutine coroutine)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }
    
        public void StopManagedCoroutine(IEnumerator routine)
        {
            StopCoroutine(routine);
        }
    }
}
