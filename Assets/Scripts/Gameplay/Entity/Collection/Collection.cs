using UnityEngine;

namespace Untitled
{
    public class Collection : EntityBase, IInteractable
    {
        public int ItemId;
        public int Num;
        public float Range = 2f;

        private bool inRange;

        protected override void Start()
        {
            SphereCollider rangeCollider = GetComponent<SphereCollider>();
            if (rangeCollider == null) rangeCollider = gameObject.AddComponent<SphereCollider>();
            rangeCollider.isTrigger = true;
            rangeCollider.radius = Range;
        }

        private void OnDestroy()
        {
            if (inRange) InteractManager.Instance.Exit(this);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (inRange || other.GetComponent<PlayerEntity>() == null) return;

            inRange = true;
            InteractManager.Instance.Enter(this);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!inRange || other.GetComponent<PlayerEntity>() == null) return;

            inRange = false;
            InteractManager.Instance.Exit(this);
        }

        public void Interact()
        {
            GameManager.Instance.GameData.PackageData.AddItem(ItemId, Num);
            EventCenter.Invoke(GameEventType.CollectItem, ItemId, Num);
            Destroy(gameObject);
        }
    }
}
