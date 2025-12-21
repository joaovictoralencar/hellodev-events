using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace HelloDev.Events
{
    [CreateAssetMenu(fileName = "GameEventFloat", menuName = "HelloDev/Events/Float Game Event")]
    public class GameEventFloat_SO : GameEvent_SO<float>
    {
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Button]
        private void RaiseEvent(float parameter)
        {
            Raise(parameter);
        }
#endif
    }
}