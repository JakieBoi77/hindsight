using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.UI
{
    /// <summary>
    /// Lightweight confetti for Screen Space Overlay canvases, where ParticleSystems do not render.
    /// Only updates while a burst is playing.
    /// </summary>
    public sealed class UiConfetti : MonoBehaviour
    {
        [SerializeField] private Image pieceTemplate;
        [SerializeField] private int pieceCount = 60;
        [SerializeField] private float lifetimeSeconds = 3.5f;
        [SerializeField] private float gravity = -1400f;
        [SerializeField]
        private Color[] palette =
        {
            new Color(0.98f, 0.36f, 0.42f),
            new Color(1f, 0.78f, 0.25f),
            new Color(0.36f, 0.75f, 0.45f),
            new Color(0.32f, 0.62f, 0.95f),
            new Color(0.68f, 0.45f, 0.9f),
        };

        private readonly List<Piece> pieces = new List<Piece>();
        private float elapsed;

        private void Awake()
        {
            pieceTemplate.gameObject.SetActive(false);
            enabled = false;
        }

        public void Burst()
        {
            Clear();
            elapsed = 0f;
            for (var i = 0; i < pieceCount; i++)
            {
                var image = Instantiate(pieceTemplate, transform);
                image.gameObject.SetActive(true);
                image.color = palette[Random.Range(0, palette.Length)];
                var rect = image.rectTransform;
                rect.anchoredPosition = new Vector2(Random.Range(-120f, 120f), Random.Range(-40f, 40f));
                rect.localEulerAngles = new Vector3(0f, 0f, Random.Range(0f, 360f));
                pieces.Add(new Piece
                {
                    Rect = rect,
                    Velocity = new Vector2(Random.Range(-900f, 900f), Random.Range(700f, 1500f)),
                    Spin = Random.Range(-540f, 540f),
                });
            }

            enabled = true;
        }

        private void Update()
        {
            var dt = Time.unscaledDeltaTime;
            elapsed += dt;
            foreach (var piece in pieces)
            {
                piece.Velocity += new Vector2(0f, gravity * dt);
                piece.Velocity *= 1f - (0.6f * dt);
                piece.Rect.anchoredPosition += piece.Velocity * dt;
                piece.Rect.localEulerAngles += new Vector3(0f, 0f, piece.Spin * dt);
            }

            if (elapsed >= lifetimeSeconds)
            {
                Clear();
                enabled = false;
            }
        }

        private void Clear()
        {
            foreach (var piece in pieces)
            {
                if (piece.Rect != null)
                {
                    Destroy(piece.Rect.gameObject);
                }
            }

            pieces.Clear();
        }

        private sealed class Piece
        {
            public RectTransform Rect;
            public Vector2 Velocity;
            public float Spin;
        }
    }
}
