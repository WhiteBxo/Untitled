/*
 * File: EntityBase.cs
 * 
 * 游戏实体基类，所有在地图内的实体都应继承自该类。
 * 自定义Postion属性，将世界坐标转换为地图坐标。
 * 
 */

using UnityEngine;

namespace Untitled.Core
{
    public abstract class EntityBase : MonoBehaviour
    {
        public Vector2 Coord
        {
            get => coord;
            set
            {
                coord = value;
                transform.position = new Vector3(coord.x, transform.position.y, coord.y);
            }
        }

        private Vector2 coord;
        
        protected virtual void Start()
        {
        }

        protected virtual void Update()
        {

        }

        protected virtual void Initialize()
        {
        }

        protected void MoveCoord(Vector2 moveDelta)
        {
            Coord += moveDelta;
        }
    }
}