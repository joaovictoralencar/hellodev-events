using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HelloDev.Events
{
    /// <summary>
    /// Generic GameEvent for typed parameters.
    /// </summary>
    /// <typeparam name="T">The type of parameter passed when the event is raised.</typeparam>
    /// <remarks>
    /// Use this as a base class for custom typed events.
    /// Built-in types: GameEventBool_SO, GameEventInt_SO, GameEventFloat_SO, GameEventString_SO.
    /// </remarks>
    public abstract class GameEvent_SO<T> : GameEventBase_SO
    {
        [SerializeField] private UnityEvent<T> _unityEvent = new UnityEvent<T>();

        // HashSet for O(1) contains/add/remove operations
        private readonly HashSet<UnityAction<T>> _listenerSet = new HashSet<UnityAction<T>>();
        // List maintained for editor display (preserves order)
        private readonly List<UnityAction<T>> _listenerList = new List<UnityAction<T>>();

        private T _lastValue;
        private bool _hasBeenRaised;

        /// <summary>
        /// The number of listeners currently subscribed to this event.
        /// </summary>
        public override int ListenerCount => _listenerSet.Count;

        /// <summary>
        /// The type of parameter this event uses.
        /// </summary>
        public override Type ParameterType => typeof(T);

        /// <summary>
        /// The most recent value passed to Raise. Check HasBeenRaised first.
        /// </summary>
        public T LastValue => _lastValue;

        /// <summary>
        /// True if Raise has been called at least once since reset.
        /// </summary>
        public bool HasBeenRaised => _hasBeenRaised;

        /// <summary>
        /// Adds a listener to this event. Duplicate listeners are ignored.
        /// </summary>
        /// <param name="listener">The callback to invoke when the event is raised.</param>
        public void AddListener(UnityAction<T> listener)
        {
            if (listener == null) return;

            // O(1) check with HashSet
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
        public void RemoveListener(UnityAction<T> listener)
        {
            if (listener == null) return;

            // O(1) check with HashSet
            if (_listenerSet.Remove(listener))
            {
                _listenerList.Remove(listener);
                _unityEvent.RemoveListener(listener);
            }
        }

        /// <summary>
        /// Subscribes to the event, removing any existing subscription first to prevent duplicates.
        /// Mirrors the SafeSubscribe pattern from HelloDev.Utils.
        /// </summary>
        /// <param name="listener">The callback to subscribe.</param>
        public void SafeSubscribe(UnityAction<T> listener)
        {
            RemoveListener(listener);
            AddListener(listener);
        }

        /// <summary>
        /// Unsubscribes from the event. Null-safe.
        /// Mirrors the SafeUnsubscribe pattern from HelloDev.Utils.
        /// </summary>
        /// <param name="listener">The callback to unsubscribe.</param>
        public void SafeUnsubscribe(UnityAction<T> listener)
        {
            RemoveListener(listener);
        }

        /// <summary>
        /// Raises the event, invoking all subscribed listeners with the given parameter.
        /// </summary>
        /// <param name="parameter">The value to pass to all listeners.</param>
        public virtual void Raise(T parameter)
        {
            _lastValue = parameter;
            _hasBeenRaised = true;

#if UNITY_EDITOR
            if (_logRaises)
            {
                Debug.Log($"GameEvent '{name}' raised with parameter: {parameter} and {_listenerSet.Count} listeners", this);
                _callStack.Push(UnityEngine.StackTraceUtility.ExtractStackTrace());
            }
#endif

            try
            {
                _unityEvent.Invoke(parameter);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error invoking GameEvent '{name}': {e.Message}", this);
            }
        }

        /// <summary>
        /// Removes all listeners from this event and resets LastValue state.
        /// </summary>
        public override void RemoveAllListeners()
        {
            _unityEvent.RemoveAllListeners();
            _listenerSet.Clear();
            _listenerList.Clear();
            _lastValue = default;
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
