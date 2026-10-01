using UnityEngine;

namespace Hindsight.App
{
    /// <summary>
    /// Plays interface sounds and (once recorded) narration. Narration is the hook for the
    /// audio-first requirement; lines without a clip simply play nothing.
    /// </summary>
    public sealed class UiAudio : MonoBehaviour
    {
        [SerializeField] private AudioSource effectsSource;
        [SerializeField] private AudioSource narrationSource;
        [SerializeField] private AudioClip tapClip;
        [SerializeField] private AudioClip successClip;
        [SerializeField] private AudioClip gentleErrorClip;

        public void PlayTap()
        {
            PlayEffect(tapClip);
        }

        public void PlaySuccess()
        {
            PlayEffect(successClip);
        }

        public void PlayGentleError()
        {
            PlayEffect(gentleErrorClip);
        }

        public void PlayNarration(AudioClip clip)
        {
            if (narrationSource == null)
            {
                return;
            }

            narrationSource.Stop();
            if (clip == null)
            {
                return;
            }

            narrationSource.clip = clip;
            narrationSource.Play();
        }

        private void PlayEffect(AudioClip clip)
        {
            if (effectsSource != null && clip != null)
            {
                effectsSource.PlayOneShot(clip);
            }
        }
    }
}
