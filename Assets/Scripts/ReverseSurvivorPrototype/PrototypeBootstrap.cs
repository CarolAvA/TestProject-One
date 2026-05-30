using UnityEngine;

namespace ReverseSurvivorPrototype
{
    public sealed class PrototypeBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreatePrototype()
        {
            if (FindFirstObjectByType<GameDirector>() != null)
            {
                return;
            }

            var bootstrapObject = new GameObject("Prototype Bootstrap");
            bootstrapObject.AddComponent<PrototypeBootstrap>();
        }

        private void Awake()
        {
            if (FindFirstObjectByType<GameDirector>() != null)
            {
                return;
            }

            var directorObject = new GameObject("Reverse Survivor Prototype");
            directorObject.AddComponent<GameDirector>();
        }
    }
}
