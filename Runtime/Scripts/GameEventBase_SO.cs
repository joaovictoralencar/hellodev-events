using System;
using System.Collections.Generic;
using HelloDev.Utils;
using UnityEngine;

namespace HelloDev.Events
{
    /// <summary>
    /// Non-generic base class for all GameEvent ScriptableObjects.
    /// This allows a single custom editor for all event types.
    /// </summary>
    /// <remarks>
    /// Inherits from RuntimeScriptableObject to auto-clear listeners between play sessions.
    /// </remarks>
    public abstract class GameEventBase_SO : RuntimeScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField] protected bool _logRaises = false;
        protected readonly Stack<string> _callStack = new Stack<string>();
#endif

        /// <summary>
        /// Gets the number of listeners subscribed to this event.
        /// </summary>
        public abstract int ListenerCount { get; }

        /// <summary>
        /// Returns true if at least one listener is subscribed.
        /// </summary>
        public bool HasListeners => ListenerCount > 0;

        /// <summary>
        /// Gets the type of parameter this event uses. Returns null for parameterless events.
        /// </summary>
        public abstract Type ParameterType { get; }

        /// <summary>
        /// Removes all listeners from this event.
        /// </summary>
        public abstract void RemoveAllListeners();

        /// <summary>
        /// Gets the listeners collection for editor display purposes.
        /// </summary>
        public abstract System.Collections.IList GetListenersForEditor();

#if UNITY_EDITOR
        /// <summary>
        /// Gets the call stack for debugging purposes.
        /// </summary>
        public Stack<string> GetCallStack() => _callStack;
#endif

        protected override void OnScriptableObjectReset()
        {
            RemoveAllListeners();
        }
    }
}
