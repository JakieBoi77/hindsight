using System;
using System.Collections.Generic;
using UnityEngine;

namespace EyeCanDo.Content
{
    [Serializable]
    public sealed class StopInfo
    {
        public string id;
        public string title;

        [Tooltip("Art name of the stop's icon, such as Stops/drops.")]
        public string icon;

        [Tooltip("Only playable stops can be opened. The rest show a Coming soon ribbon.")]
        public bool playable;
    }

    /// <summary>The stops on the clinic map, in the order of a typical visit (GameDesign §8.2).</summary>
    [CreateAssetMenu(menuName = "EyeCanDo/Stop Catalog", fileName = "StopCatalog")]
    public sealed class StopCatalog : ScriptableObject
    {
        public const string EyeDropsId = "special_drops";

        public List<StopInfo> stops = new List<StopInfo>();
    }
}
