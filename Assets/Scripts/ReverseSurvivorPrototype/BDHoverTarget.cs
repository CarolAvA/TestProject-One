using UnityEngine;
using UnityEngine.EventSystems;

namespace ReverseSurvivorPrototype
{
    public sealed class BDHoverTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private PrototypeHud hud;
        private int index;

        public void Initialize(PrototypeHud owner, int bdIndex)
        {
            hud = owner;
            index = bdIndex;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (hud != null)
            {
                hud.SetHoveredBD(index, true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (hud != null)
            {
                hud.SetHoveredBD(index, false);
            }
        }
    }
}
