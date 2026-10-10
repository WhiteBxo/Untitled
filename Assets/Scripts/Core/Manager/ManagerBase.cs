using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Untitled.Core
{
    public abstract class ManagerBase<T> where T : ManagerBase<T>
    {
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.LogError($"Instance of {typeof(T)} is not initialized.");
                }
                return instance;
            }
        }

        private static T instance;

        public virtual void Initialize()
        {
            instance = (T)this;
        }
    }
}