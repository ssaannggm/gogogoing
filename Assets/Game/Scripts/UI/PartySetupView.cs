// Assets/Game/Scripts/UI/PartySetupView.cs (최종 수정본)
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace Game.UI
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class PartySetupView : MonoBehaviour
    {
        [Header("Slots")]
        [SerializeField] RectTransform slotsParent;
        [SerializeField] GameObject slotButtonPrefab;

        [Header("Selection Colors")]
        [Tooltip("선택되지 않았을 때 버튼 색상")]
        [SerializeField] private Color _normalColor = Color.white;
        [Tooltip("선택되었을 때 버튼 색상")]
        [SerializeField] private Color _selectedColor = Color.yellow;

        // --- 나머지 변수들은 기존과 동일 ---
        readonly List<GameObject> _slotButtons = new();

        // --- 이하 다른 함수들은 기존과 동일 ---

        /// <summary>
        /// [핵심 추가] 선택된 슬롯의 색상을 변경하는 함수
        /// </summary>
        public void SetSelection(int selectedIndex)
        {
            for (int i = 0; i < _slotButtons.Count; i++)
            {
                var buttonImage = _slotButtons[i].GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = (i == selectedIndex) ? _selectedColor : _normalColor;
                }
            }
        }


        // ------------------------------------------------------------------
        // 아래의 함수들은 수정할 필요가 없습니다. 그대로 두시면 됩니다.
        // ------------------------------------------------------------------

        public void BuildSlots(int max, System.Func<int, string> getLabel, System.Action<int> onClickSlot)
        {
            if (!isActiveAndEnabled || !gameObject.activeInHierarchy || !ParentReady(slotsParent))
            {
                StartCoroutine(BuildSlotsNextFrame(max, getLabel, onClickSlot));
                return;
            }

            Clear(_slotButtons, slotsParent);
            if (max <= 0 || !slotsParent || !slotButtonPrefab) return;

            for (int i = 0; i < max; i++)
            {
                var go = SafeSpawn(slotButtonPrefab, slotsParent, $"Slot_{i}");
                if (!go) continue;

                var btn = go.GetComponent<Button>();
                var tmp = go.GetComponentInChildren<TMP_Text>(true);

                string label = getLabel != null ? getLabel(i) : $"Slot {i + 1}";
                if (tmp) tmp.text = label;

                int idx = i;
                if (btn)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => onClickSlot?.Invoke(idx));
                }
                _slotButtons.Add(go);
            }
            ForceLayout(slotsParent);
        }

        public void SetSlotLabel(int index, string label)
        {
            if (index < 0 || index >= _slotButtons.Count) return;
            var go = _slotButtons[index];
            var tmp = go.GetComponentInChildren<TMP_Text>(true);
            if (tmp) tmp.text = label;
        }

        // --- 이하 다른 헬퍼 함수들은 기존과 동일 ---
        IEnumerator BuildSlotsNextFrame(int max, System.Func<int, string> getLabel, System.Action<int> onClickSlot)
        {
            yield return null;
            BuildSlots(max, getLabel, onClickSlot);
        }
        void Clear(List<GameObject> list, RectTransform parent)
        {
            foreach (var go in list) if (go) Destroy(go);
            list.Clear();
        }
        GameObject SafeSpawn(GameObject prefab, RectTransform parent, string nameHint)
        {
            if (!parent || !ParentReady(parent) || !prefab) return null;
            var go = Instantiate(prefab, parent, false);
            go.name = $"{prefab.name}_{nameHint}";
            go.transform.localScale = Vector3.one;
            return go;
        }
        static bool ParentReady(RectTransform parent) => parent && parent.gameObject.scene.isLoaded && parent.gameObject.activeInHierarchy;
        static void ForceLayout(RectTransform rt)
        {
            if (!rt) return;
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }
    }
}