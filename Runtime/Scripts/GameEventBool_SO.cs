using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace HelloDev.Events
{
    [CreateAssetMenu(fileName = "GameEventBool", menuName = "HelloDev/Events/Bool Game Event")]
    public class GameEventBool_SO : GameEvent_SO<bool>
    {
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Button]
        private void RaiseEvent(bool parameter)
        {
            Raise(parameter);
        }
#endif
    }
}