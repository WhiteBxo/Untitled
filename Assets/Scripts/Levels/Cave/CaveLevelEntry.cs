using UnityEngine;

namespace Untitled
{
    public sealed class CaveLevelEntry : MonoBehaviour
    {
        [SerializeField] private CaveMap map;
        [SerializeField] private CharacterController player;

        private void Start()
        {
            if (map == null || player == null || !map.HasEditableLayout) return;
            if (!map.IsWalkable(map.SpawnPosition))
            {
                Debug.LogError("Cave spawn point must be on a floor cell.", map);
                return;
            }

            bool wasEnabled = player.enabled;
            player.enabled = false;
            player.transform.position = map.SpawnPosition;
            player.enabled = wasEnabled;
        }
    }
}
