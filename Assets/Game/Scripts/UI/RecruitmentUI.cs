// Assets/Game/Scripts/UI/RecruitmentUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Data;
using System;

namespace Game.UI
{
    public class RecruitmentUI : MonoBehaviour
    {
        [Header("���� UI ���")]
        [SerializeField] private Image _bgA;
        [SerializeField] private TextMeshProUGUI _storyA;
        [SerializeField] private Button _buttonA;
        [SerializeField] private TextMeshProUGUI _nameA;

        [Header("���� UI ���")]
        [SerializeField] private Image _bgB;
        [SerializeField] private TextMeshProUGUI _storyB;
        [SerializeField] private Button _buttonB;
        [SerializeField] private TextMeshProUGUI _nameB;

        // �ݹ�: (���õ� ����, ���õ� ���� ����, ���õ��� ���� ���� ����)
        private Action<UnitSO, RecruitmentInfoSO, RecruitmentInfoSO> _onChosenCallback;
        private RecruitmentInfoSO _infoA;
        private RecruitmentInfoSO _infoB;

        void Awake()
        {
            _buttonA.onClick.AddListener(OnChooseA);
            _buttonB.onClick.AddListener(OnChooseB);
            gameObject.SetActive(false); // ó������ ��Ȱ��ȭ
        }

        public void ShowChoice(RecruitmentInfoSO infoA, RecruitmentInfoSO infoB, Action<UnitSO, RecruitmentInfoSO, RecruitmentInfoSO> callback)
        {
            _infoA = infoA;
            _infoB = infoB;
            _onChosenCallback = callback;

            // UI�� ������ ä���
            _bgA.sprite = _infoA.backgroundImage;
            _storyA.text = _infoA.story;
            _nameA.text = _infoA.unit.displayName;

            _bgB.sprite = _infoB.backgroundImage;
            _storyB.text = _infoB.story;
            _nameB.text = _infoB.unit.displayName;

            gameObject.SetActive(true);
        }

        private void OnChooseA()
        {
            gameObject.SetActive(false);
            _onChosenCallback?.Invoke(_infoA.unit, _infoA, _infoB);
        }

        private void OnChooseB()
        {
            gameObject.SetActive(false);
            _onChosenCallback?.Invoke(_infoB.unit, _infoB, _infoA);
        }
    }
}