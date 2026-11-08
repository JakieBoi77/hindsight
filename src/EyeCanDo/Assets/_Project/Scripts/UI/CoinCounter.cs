using System.Collections;
using Hindsight.Ophthalmology.Core;
using TMPro;
using UnityEngine;

namespace Hindsight.Ophthalmology.UI
{
    /// <summary>The coin purse: shows the current player's coins and counts up when coins arrive.</summary>
    public sealed class CoinCounter : MonoBehaviour
    {
        [SerializeField] TMP_Text label;

        int shown;

        void OnEnable()
        {
            GameServices.ProgressChanged += Refresh;
            Refresh();
        }

        void OnDisable() => GameServices.ProgressChanged -= Refresh;

        public void Refresh() => Set(GameServices.Progress.coins);

        void Set(int value)
        {
            shown = value;
            label.text = value.ToString();
        }

        /// <summary>Stops following the save, keeping the number shown (coins about to fly in).</summary>
        public void Hold() => GameServices.ProgressChanged -= Refresh;

        /// <summary>Shows a number with a little bounce (one coin landing).</summary>
        public void Bump(int value)
        {
            Set(value);
            transform.localScale = Vector3.one * 1.15f;
        }

        /// <summary>Follows the save again.</summary>
        public void Release()
        {
            transform.localScale = Vector3.one;
            GameServices.ProgressChanged -= Refresh;
            if (isActiveAndEnabled) GameServices.ProgressChanged += Refresh;
            Refresh();
        }

        public int Shown => shown;

        void Update()
        {
            if (transform.localScale.x > 1f) transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.one, Time.deltaTime);
        }

        /// <summary>Counts up to <paramref name="target"/> with a little bounce per step.</summary>
        public IEnumerator CountTo(int target)
        {
            GameServices.ProgressChanged -= Refresh;
            var start = shown;
            var steps = Mathf.Clamp(target - start, 0, 20);
            for (var i = 1; i <= steps; i++)
            {
                Set(Mathf.RoundToInt(Mathf.Lerp(start, target, (float)i / steps)));
                transform.localScale = Vector3.one * 1.12f;
                for (var t = 0f; t < 0.04f; t += Time.deltaTime) yield return null;
                transform.localScale = Vector3.one;
            }
            Set(target);
            if (isActiveAndEnabled) GameServices.ProgressChanged += Refresh;
        }

#if UNITY_EDITOR
        public void EditorInit(TMP_Text text) => label = text;
#endif
    }
}
