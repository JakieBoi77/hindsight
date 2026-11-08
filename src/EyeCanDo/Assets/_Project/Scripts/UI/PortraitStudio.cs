using Hindsight.EyeCanDo.Core.Avatar;
using Hindsight.EyeCanDo.SceneObjects;
using UnityEngine;

namespace Hindsight.EyeCanDo.UI
{
    /// <summary>
    /// Three kids, far from the room, each filmed by its own camera into a render texture. The save
    /// cards show these, so every save shows its own dressed-up kid.
    /// </summary>
    public sealed class PortraitStudio : MonoBehaviour
    {
        [SerializeField] KidActor[] kids = new KidActor[0];

        public void Show(int slot, AvatarLook look)
        {
            if (slot < 0 || slot >= kids.Length || kids[slot] == null) return;
            kids[slot].Show(look != null);
            if (look != null) kids[slot].Apply(look);
        }

#if UNITY_EDITOR
        public void EditorInit(KidActor[] actors) => kids = actors;
#endif
    }
}
