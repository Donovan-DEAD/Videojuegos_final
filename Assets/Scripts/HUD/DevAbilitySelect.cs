using UnityEngine;
using UnityEngine.UI;

namespace MadnessDreams
{
    /// <summary>Panel DEV: activa/desactiva habilidades y elige cuál queda desbloqueada.</summary>
    public class DevAbilitySelect : MonoBehaviour
    {
        [SerializeField] private float rowHeight = 30f;
        private PlayerAbilities abilities;
        private Text status;
        private Text unlockLabel;

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            abilities = player != null ? player.GetComponent<PlayerAbilities>() : null;
            Build();
            Refresh();
        }

        private Font Font() => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        private void Build()
        {
            foreach (Transform c in transform) Destroy(c.gameObject);

            status = MakeText("Status", "", new Vector2(0, 0), new Vector2(320, rowHeight));
            unlockLabel = MakeText("Unlock", "", new Vector2(0, -rowHeight), new Vector2(320, rowHeight));

            string[] keys = { "Q", "E", "R", "F" };
            for (int i = 0; i < keys.Length; i++)
            {
                int idx = i;
                MakeButton("Slot" + i, keys[i], new Vector2(0, -rowHeight * (i + 2)), () => ToggleSlot(idx));
            }

            MakeButton("CycleUnlock", "Desbloquear (ciclo)", new Vector2(0, -rowHeight * 6), CycleUnlock);
            MakeButton("AllOn", "Todas activas", new Vector2(0, -rowHeight * 7), AllUnlocked);
        }

        private Text MakeText(string name, string content, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(0, 1); rt.pivot = new Vector2(0, 1);
            rt.sizeDelta = size; rt.anchoredPosition = pos;
            var t = go.GetComponent<Text>(); t.font = Font(); t.fontSize = 16; t.color = Color.white; t.text = content;
            return t;
        }

        private void MakeButton(string name, string label, Vector2 pos, UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(0, 1); rt.pivot = new Vector2(0, 1);
            rt.sizeDelta = new Vector2(320, rowHeight - 4); rt.anchoredPosition = pos;
            go.GetComponent<Image>().color = new Color(0.2f, 0.18f, 0.35f, 1f);
            var txtGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            txtGo.transform.SetParent(go.transform, false);
            var trt = txtGo.GetComponent<RectTransform>(); trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one; trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
            var t = txtGo.GetComponent<Text>(); t.font = Font(); t.fontSize = 15; t.alignment = TextAnchor.MiddleCenter; t.color = Color.white; t.text = label;
            go.GetComponent<Button>().onClick.AddListener(action);
        }

        private void ToggleSlot(int index)
        {
            if (abilities == null) return;
            abilities.SetSlotLocked(index, !abilities.IsSlotLocked(index));
            Refresh();
        }

        private void CycleUnlock()
        {
            if (abilities == null) return;
            int next = 0;
            for (int i = 0; i < abilities.SlotCount; i++) if (abilities.IsSlotLocked(i)) { next = i; break; }
            for (int i = 0; i < abilities.SlotCount; i++) abilities.SetSlotLocked(i, i != next);
            Refresh();
        }

        private void AllUnlocked()
        {
            if (abilities == null) return;
            for (int i = 0; i < abilities.SlotCount; i++) abilities.SetSlotLocked(i, false);
            Refresh();
        }

        private void Refresh()
        {
            if (status == null || abilities == null) return;
            string s = "";
            for (int i = 0; i < abilities.SlotCount; i++)
            {
                var a = abilities.GetSlot(i);
                s += (i > 0 ? "  " : "") + (a != null ? a.name.Replace("Ability_", "") : "-") + (abilities.IsSlotLocked(i) ? "[X]" : "[OK]");
            }
            status.text = s;
            unlockLabel.text = "Selecciona: pulsa una Q/E/R/F para bloquear/desbloquear";
        }
    }
}
