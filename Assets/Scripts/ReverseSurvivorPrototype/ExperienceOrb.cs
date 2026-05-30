using UnityEngine;

namespace ReverseSurvivorPrototype
{
    public sealed class ExperienceOrb : MonoBehaviour
    {
        public float Value { get; private set; }
        public Vector2 Position => transform.position;

        public void Initialize(float value)
        {
            Value = value;
        }

        public void MoveToward(Vector2 target, float step)
        {
            transform.position = Vector2.MoveTowards(Position, target, step);
        }
    }
}
