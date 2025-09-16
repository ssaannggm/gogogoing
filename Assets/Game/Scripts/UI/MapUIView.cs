// Assets/Game/Scripts/UI/MapUIView.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

using Game.Data; // MapNodeSO

namespace Game.UI
{
    public sealed class MapUIView : MonoBehaviour
    {
        [Header("Wire in Inspector")]
        [SerializeField] TMP_Text titleText;
        [SerializeField] TMP_Text descText;
        [SerializeField] Transform choicesParent;
        [SerializeField] GameObject choiceButtonPrefab; // NodeChoiceButton.prefab

        public void Render(MapNodeSO node, Action<MapNodeSO> onPick)
        {
            if (!node) { ClearChoices(); SetTitleDesc("(없음)", "노드가 지정되지 않았습니다."); return; }

            SetTitleDesc(node.title, node.desc);

            ClearChoices();
            var next = node.nextNodes ?? System.Array.Empty<MapNodeSO>();
            if (next.Length == 0)
            {
                MakeButton("끝 (진행 불가)", null, null, interactable: false);
                return;
            }

            foreach (var n in next)
            {
                string label = n ? (string.IsNullOrEmpty(n.title) ? n.name : n.title) : "(null)";
                MakeButton(label, n, onPick, interactable: n != null);
            }
        }

        void SetTitleDesc(string t, string d)
        {
            if (titleText) titleText.text = t ?? "";
            if (descText) descText.text = d ?? "";
        }

        void ClearChoices()
        {
            if (!choicesParent) return;
            for (int i = choicesParent.childCount - 1; i >= 0; i--)
                Destroy(choicesParent.GetChild(i).gameObject);
        }

        void MakeButton(string label, MapNodeSO node, Action<MapNodeSO> onPick, bool interactable)
        {
            var go = Instantiate(choiceButtonPrefab, choicesParent);
            var btn = go.GetComponent<Button>();
            var txt = go.GetComponentInChildren<TMP_Text>();
            if (txt) txt.text = label;
            if (btn)
            {
                btn.interactable = interactable;
                btn.onClick.AddListener(() => { if (interactable) onPick?.Invoke(node); });
            }
        }
    }
}
