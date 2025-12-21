using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace HelloDev.Events
{
    [CreateAssetMenu(fileName = "GameEventString", menuName = "HelloDev/Events/String Game Event")]
    public class GameEventString_SO : GameEvent_SO<string>
    {
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Button]
        private void RaiseEvent(string parameter)
        {
            Raise(parameter);
        }
#endif
    }
}