using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Untitled
{
    public class BussinessManager : MonoBehaviour
    {
        [SerializeField]
        private float dayTime;

        private float timer;

        private void Update()
        {
            timer -= Time.deltaTime;
        }

        public void StartBussiness()
        {
            timer = dayTime;
        }

        public void EndBussiness()
        {
            // 显示经营结算界面

            // 进入夜晚冒险
        }
    }
}