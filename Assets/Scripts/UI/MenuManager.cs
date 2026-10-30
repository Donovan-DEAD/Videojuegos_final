using UnityEngine;

namespace MadnessDreams
{
    /// <summary>Garantiza que solo haya un menú abierto a la vez.</summary>
    public class MenuManager : MonoBehaviour
    {
        private static MenuManager instance;

        public static MenuManager Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("MenuManager");
                    instance = go.AddComponent<MenuManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        private MonoBehaviour owner;
        private System.Action closeAction;

        public void Open(MonoBehaviour newOwner, System.Action close)
        {
            if (owner != null && owner != newOwner && closeAction != null)
            {
                var previous = closeAction;
                owner = null;
                closeAction = null;
                previous();
            }

            owner = newOwner;
            closeAction = close;
        }

        public void Release(MonoBehaviour who)
        {
            if (owner == who)
            {
                owner = null;
                closeAction = null;
            }
        }

        public void CloseCurrent()
        {
            var current = closeAction;
            owner = null;
            closeAction = null;
            if (current != null) current();
        }
    }
}
