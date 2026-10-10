using UnityEngine;

namespace Untitled
{
    public class Bootstrap : MonoBehaviour
    {
        private void Awake()
        {
            RegisterManager(new InputManager());
        }

        private void Update()
        {

        }

        private void RegisterManager<T>(ManagerBase<T> manager) where T : ManagerBase<T>
        {
            manager.Initialize();
        }
    }
}