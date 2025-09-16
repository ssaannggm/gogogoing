// Assets/Game/Scripts/UI/PartySetupView.cs
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
        [SerializeField] RectTransform slotsParent;           // 반드시 Canvas 하위 RectTransform
        [SerializeField] GameObject slotButtonPrefab;         // UI 버튼 프리팹(루트에 RectTransform 필수)

        [Header("Roster")]
        [SerializeField] RectTransform rosterParent;          // 반드시 RectTransform
        [SerializeField] GameObject rosterButtonPrefab;       // UI 버튼 프리팹(루트에 RectTransform 필수)

        [Header("Footer")]
        [SerializeField] Button saveButton;
        [SerializeField] Button clearButton;
        [SerializeField] TMP_Text hintText;

        readonly List<GameObject> _slotButtons = new();
        readonly List<GameObject> _rosterButtons = new();

        public void BuildSlots(int max, System.Func<int, string> getLabel, System.Action<int> onClickSlot)
        {
            DumpParentState(slotsParent, "BuildSlots(BEFORE)");

            if (!isActiveAndEnabled || !gameObject.activeInHierarchy || !ParentReady(slotsParent))
            {
                Debug.LogWarning("[PartySetupView] BuildSlots called while view/parent inactive. Deferring to next frame.");
                StartCoroutine(BuildSlotsNextFrame(max, getLabel, onClickSlot));
                return;
            }

            Debug.Log($"[PartySetupView] BuildSlots start | max={max} " +
                      $"slotsParent={(slotsParent ? slotsParent.name : "NULL")} " +
                      $"prefab={(slotButtonPrefab ? slotButtonPrefab.name : "NULL")} " +
                      $"childCount(before)={(slotsParent ? slotsParent.childCount : -1)}");

            Clear(_slotButtons, slotsParent);

            if (max <= 0) { Placeholder(slotsParent, "[슬롯 0개]"); ForceLayout(slotsParent); return; }
            if (!slotsParent || !slotButtonPrefab)
            {
                Placeholder(slotsParent, "[슬롯 프리팹/부모 없음]");
                ForceLayout(slotsParent);
                return;
            }

            for (int i = 0; i < max; i++)
            {
                var go = SafeSpawn(slotButtonPrefab, slotsParent, $"Slot_{i}");
                if (!go) continue;

                var btn = go.GetComponent<Button>();
                var tmp = go.GetComponentInChildren<TMP_Text>(true);
                var legacy = tmp ? null : go.GetComponentInChildren<Text>(true);

                string label = getLabel != null ? getLabel(i) : $"Slot {i + 1}";
                if (tmp) tmp.text = label;
                else if (legacy) legacy.text = label;
                else Debug.LogWarning($"[PartySetupView] SlotButton에 텍스트 컴포넌트가 없습니다: {go.name}");

                int idx = i;
                if (btn)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => onClickSlot?.Invoke(idx));
                }

                _slotButtons.Add(go);
            }

            ForceLayout(slotsParent);
            Debug.Log($"[PartySetupView] BuildSlots end | childCount(after)={slotsParent.childCount}");
            DumpParentState(slotsParent, "BuildSlots(AFTER)");
        }

        public void BuildRoster(IReadOnlyList<string> labels, System.Action<int> onPick)
        {
            DumpParentState(rosterParent, "BuildRoster(BEFORE)");

            if (!isActiveAndEnabled || !gameObject.activeInHierarchy || !ParentReady(rosterParent))
            {
                Debug.LogWarning("[PartySetupView] BuildRoster called while view/parent inactive. Deferring to next frame.");
                StartCoroutine(BuildRosterNextFrame(labels, onPick));
                return;
            }

            int count = labels?.Count ?? 0;
            Debug.Log($"[PartySetupView] BuildRoster start | count={count} " +
                      $"rosterParent={(rosterParent ? rosterParent.name : "NULL")} " +
                      $"prefab={(rosterButtonPrefab ? rosterButtonPrefab.name : "NULL")} " +
                      $"childCount(before)={(rosterParent ? rosterParent.childCount : -1)}");

            Clear(_rosterButtons, rosterParent);

            if (count == 0)
            {
                Placeholder(rosterParent, "[후보 없음: RosterSO 확인]");
                ForceLayout(rosterParent);
                return;
            }
            if (!rosterParent || !rosterButtonPrefab)
            {
                Placeholder(rosterParent, "[로스터 프리팹/부모 없음]");
                ForceLayout(rosterParent);
                return;
            }

            for (int i = 0; i < count; i++)
            {
                var go = SafeSpawn(rosterButtonPrefab, rosterParent, $"Roster_{i}");
                if (!go) continue;

                var btn = go.GetComponent<Button>();
                var tmp = go.GetComponentInChildren<TMP_Text>(true);
                var legacy = tmp ? null : go.GetComponentInChildren<Text>(true);

                if (tmp) tmp.text = labels[i];
                else if (legacy) legacy.text = labels[i];
                else Debug.LogWarning($"[PartySetupView] RosterButton에 텍스트 컴포넌트가 없습니다: {go.name}");

                int idx = i;
                if (btn)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => onPick?.Invoke(idx));
                }

                _rosterButtons.Add(go);
            }

            ForceLayout(rosterParent);
            Debug.Log($"[PartySetupView] BuildRoster end | childCount(after)={rosterParent.childCount}");
            DumpParentState(rosterParent, "BuildRoster(AFTER)");
        }

        public void SetSlotLabel(int index, string label)
        {
            if (index < 0 || index >= _slotButtons.Count) return;
            var go = _slotButtons[index];
            var tmp = go.GetComponentInChildren<TMP_Text>(true);
            if (tmp) { tmp.text = label; return; }
            var legacy = go.GetComponentInChildren<Text>(true);
            if (legacy) legacy.text = label;
        }

        public void SetHint(string msg)
        {
            if (hintText) hintText.text = msg ?? "";
        }

        public void WireFooter(System.Action onSave, System.Action onClear)
        {
            if (saveButton)
            {
                saveButton.onClick.RemoveAllListeners();
                saveButton.onClick.AddListener(() => onSave?.Invoke());
            }
            if (clearButton)
            {
                clearButton.onClick.RemoveAllListeners();
                clearButton.onClick.AddListener(() => onClear?.Invoke());
            }
        }

        IEnumerator BuildSlotsNextFrame(int max, System.Func<int, string> getLabel, System.Action<int> onClickSlot)
        {
            yield return null; // 부모 활성/씬 로드 완료 대기
            BuildSlots(max, getLabel, onClickSlot);
        }

        IEnumerator BuildRosterNextFrame(IReadOnlyList<string> labels, System.Action<int> onPick)
        {
            yield return null; // 부모 활성/씬 로드 완료 대기
            BuildRoster(labels, onPick);
        }

        void Clear(List<GameObject> list, RectTransform parent)
        {
            for (int i = 0; i < list.Count; i++)
                if (list[i]) Destroy(list[i]);
            list.Clear();

#if UNITY_EDITOR
            // 개발 시 이전 프레임 Destroy 대기 없이 즉시 정리하고 싶을 때, 필요 시 아래 한 줄을 임시로 사용하십시오.
            // for (int i = parent ? parent.childCount - 1 : -1; i >= 0; --i) DestroyImmediate(parent.GetChild(i).gameObject);
#endif
        }

        GameObject SafeSpawn(GameObject prefab, RectTransform parent, string nameHint)
        {
            if (!parent)
            {
                Debug.LogWarning("[PartySetupView] SafeSpawn skip: parent NULL");
                return null;
            }
            if (!ParentReady(parent))
            {
                Debug.LogWarning($"[PartySetupView] SafeSpawn skip: parent not ready | parent={parent.name} " +
                                 $"| active={parent.gameObject.activeInHierarchy} | sceneLoaded={parent.gameObject.scene.isLoaded}");
                return null;
            }

            try
            {
                var prefabRT = prefab ? prefab.transform as RectTransform : null;
                if (!prefab || prefabRT == null)
                {
                    Debug.LogError($"[PartySetupView] Prefab invalid or root has no RectTransform: {(prefab ? prefab.name : "NULL")}");
                    return null;
                }

                var go = Instantiate(prefab, parent, false);
                go.name = $"{prefab.name}_{nameHint}";

                var rt = (RectTransform)go.transform;
                rt.localScale = Vector3.one;
                rt.anchoredPosition3D = Vector3.zero;

                if (rt.sizeDelta == Vector2.zero && !go.GetComponent<LayoutElement>())
                    rt.sizeDelta = new Vector2(160, 40);

                go.layer = parent.gameObject.layer;
                go.SetActive(true);

                Debug.Log($"[PartySetupView] Spawned {go.name} -> parent={parent.name} " +
                          $"| parentScene={parent.gameObject.scene.name} | objScene={go.scene.name}");

                return go;
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }

        static bool ParentReady(RectTransform parent)
        {
            return parent && parent.gameObject.scene.IsValid() && parent.gameObject.scene.isLoaded && parent.gameObject.activeInHierarchy;
        }

        void Placeholder(RectTransform parent, string msg)
        {
            if (!parent) { Debug.LogWarning($"[PartySetupView] {msg} (parent NULL)"); return; }
            var go = new GameObject("Dummy");
            var rt = go.AddComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.sizeDelta = new Vector2(420, 40);
            var txt = go.AddComponent<TextMeshProUGUI>();
            txt.text = msg;
            txt.fontSize = 20f;
            txt.alignment = TextAlignmentOptions.Center;
            var img = go.AddComponent<Image>();
            img.color = new Color(1, 1, 1, 0.05f);
        }

        static void ForceLayout(RectTransform rt)
        {
            if (!rt) return;
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            Canvas.ForceUpdateCanvases();
        }

        static void DumpParentState(RectTransform p, string tag)
        {
            if (!p)
            {
                Debug.LogWarning($"[{tag}] parent NULL");
                return;
            }
            var cg = p.GetComponentInParent<CanvasGroup>();
            var canvas = p.GetComponentInParent<Canvas>();
            Debug.Log($"[{tag}] parent={p.name} active={p.gameObject.activeInHierarchy} " +
                      $"sceneLoaded={p.gameObject.scene.isLoaded} childCount={p.childCount} " +
                      $"Canvas={(canvas ? canvas.name : "none")}, enabled={(canvas && canvas.enabled)} " +
                      $"CG.alpha={(cg ? cg.alpha : -1f)}, CG.blocksRaycasts={(cg && cg.blocksRaycasts)}");
        }

#if UNITY_EDITOR
        [ContextMenu("DEV: Validate Inspector")]
        void DevValidate()
        {
            Debug.Log($"[PartySetupView] slotsParent={(slotsParent ? slotsParent.name : "NULL")}, rosterParent={(rosterParent ? rosterParent.name : "NULL")}");
            Debug.Log($"[PartySetupView] slotPrefab={(slotButtonPrefab ? slotButtonPrefab.name : "NULL")}, rosterPrefab={(rosterButtonPrefab ? rosterButtonPrefab.name : "NULL")}");
        }
#endif
    }
}
