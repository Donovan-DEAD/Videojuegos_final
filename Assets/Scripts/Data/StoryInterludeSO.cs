using UnityEngine;

namespace MadnessDreams
{
    /// <summary>Secuencia de páginas de historia mostrada entre mundos.</summary>
    [CreateAssetMenu(fileName = "StoryInterlude", menuName = "Madness Dreams/Story Interlude")]
    public class StoryInterludeSO : ScriptableObject
    {
        [System.Serializable]
        public class Page
        {
            public Sprite image;
            [TextArea] public string text;
            public float holdSeconds = 4f;
        }

        [SerializeField] private string title;
        [SerializeField] private Page[] pages;
        [SerializeField] private bool skippable = true;
        [SerializeField] private AudioClip music;

        public string Title => title;
        public Page[] Pages => pages;
        public bool Skippable => skippable;
        public AudioClip Music => music;
    }
}
