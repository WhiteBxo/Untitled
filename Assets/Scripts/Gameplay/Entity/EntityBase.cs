using UnityEngine;

namespace Untitled
{
    public abstract class EntityBase : MonoBehaviour
    {
        public Vector2 Coord
        {
            get => new(transform.position.x, transform.position.z);
            set => transform.position = new Vector3(value.x, transform.position.y, value.y);
        }

        protected virtual void Start()
        {
        }

        protected virtual void Update()
        {
        }

        protected virtual void Initialize()
        {
        }
    }
}
