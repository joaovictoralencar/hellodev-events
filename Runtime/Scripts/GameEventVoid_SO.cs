using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace HelloDev.Events
{
    /// <summary>
    /// Parameterless GameEvent for simple notifications without data.
    /// </summary>
    /// <remarks>
    /// Use for events like OnGameStart, OnPause, OnResume, OnLevelComplete, etc.
    /// where no data needs to be passed to listeners.
    /// </remarks>
    [CreateAssetMenu(fileName = "GameEvent", menuName = "HelloDev/Events/Game Event (Void)")]
    public class GameEventVoid_SO : GameEventBase_SO
    {
        [SerializeField] private UnityEvent _unityEvent = new UnityEvent();

        // HashSet for O(1) contains/add/remove operations
        private readonly HashSet<UnityAction> _listenerSet = new HashSet<UnityAction>();
        // List maintained for editor display (preserves order)
        private readonly List<UnityAction> _listenerList = new List<UnityAction>();

        private bool _hasBeenRaised;

        /// <summary>
        /// The number of listeners currently subscribed to this event.
        /// </summary>
        public override int ListenerCount => _listenerSet.Count;

        /// <summary>
        /// Returns null since this event has no parameter type.
        /// </summary>
        public override Type ParameterType => null;

        /// <summary>
        /// True if Raise has been called at least once since reset.
        /// </summary>
        public bool HasBeenRaised => _hasBeenRaised;

        /// <summary>
        /// Adds a listener to this event. Duplicate listeners are ignored.
        /// </summary>
        /// <param name="listener">The callback to invoke when the event is raised.</param>
        public void AddListener(UnityAction listener)
        {
            if (listener == null) return;

            if (_listenerSet.Add(listener))
            {
                _listenerList.Add(listener);
                _unityEvent.AddListener(listener);
            }
        }

        /// <summary>
        /// Removes a listener from this event.
        /// </summary>
        /// <param name="listener">The callback to remove.</param>
        public void RemoveListener(UnityAction listener)
        {
            if (listener == null) return;

            if (_listenerSet.Remove(listener))
            {
                _listenerList.Remove(listener);
                _unityEvent.RemoveListener(listener);
            }
        }

        /// <summary>
        /// Subscribes to the event, removing any existing subscription first to prevent duplicates.
        /// </summary>
        /// <param name="listener">The callback to subscribe.</param>
        public void SafeSubscribe(UnityAction listener)
        {
            RemoveListener(listener);
            AddListener(listener);
        }

        /// <summary>
        /// Unsubscribes from the event. Null-safe.
        /// </summary>
        /// <param name="listener">The callback to unsubscribe.</param>
        public void SafeUnsubscribe(UnityAction listener)
        {
            RemoveListener(listener);
        }

        /// <summary>
        /// Raises the event, invoking all subscribed listeners.
        /// </summary>
#if ODIN_INSPECTOR
        [Button]
#endif
        public virtual void Raise()
        {
            _hasBeenRaised = true;

#if UNITY_EDITOR
            if (_logRaises)
            {
                Debug.Log($"GameEvent '{name}' raised with {_listenerSet.Count} listeners", this);
                _callStack.Push(UnityEngine.StackTraceUtility.ExtractStackTrace());
            }
#endif

            try
            {
                _unityEvent.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error invoking GameEvent '{name}': {e.Message}", this);
            }
        }

        /// <summary>
        /// Removes all listeners from this event.
        /// </summary>
        public override void RemoveAllListeners()
        {
            _unityEvent.RemoveAllListeners();
            _listenerSet.Clear();
            _listenerList.Clear();
            _hasBeenRaised = false;
        }

        /// <summary>
        /// Gets the listeners collection for editor display purposes.
        /// </summary>
        public override System.Collections.IList GetListenersForEditor()
        {
            return _listenerList;
        }
    }
}
