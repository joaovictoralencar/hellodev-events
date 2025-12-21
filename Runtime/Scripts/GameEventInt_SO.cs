using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace HelloDev.Events
{
    [CreateAssetMenu(fileName = "GameEventInt", menuName = "HelloDev/Events/Int Game Event")]
    public class GameEventInt_SO : GameEvent_SO<int>
    {
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Button]
        private void RaiseEvent(int parameter)
        {
            Raise(parameter);
        }
#endif
    }
}