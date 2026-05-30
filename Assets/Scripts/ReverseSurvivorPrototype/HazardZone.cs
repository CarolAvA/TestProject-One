using UnityEngine;

namespace ReverseSurvivorPrototype
{
    public sealed class HazardZone : MonoBehaviour
    {
        public float Radius { get; private set; }
        public float DamagePerSecond { get; private set; }
        public float SpeedMultiplier { get; private set; }
        public Vector2 Position => transform.position;

        public void Initialize(float radius, float damagePerSecond, float speedMultiplier)
        {
            Radius = radius;
            DamagePerSecond = damagePerSecond;
            SpeedMultiplier = speedMultiplier;
        }
    }
}
