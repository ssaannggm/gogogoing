using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Services;
using Game.UI;
using Game.Runtime;
using Game.Items; // StartingKitSO를 사용하기 위해 추가

public class RecruitmentManager : MonoBehaviour, IGameMode
{
    [Header("UI 참조")]
    [SerializeField] private RecruitmentUI _recruitmentUI;
    [SerializeField] private PartySetupView _partySetupView;

    // [추가] 시작 아이템 키트
    [Header("데이터")]
    [Tooltip("플레이어가 시작 시 기본으로 받을 아이템 키트")]
    [SerializeField] private StartingKitSO _startingKit;

    private GameFlowController _flow;
    private List<RecruitmentInfoSO> _sessionCandidates;
    private int _recruitmentCount = 0;
    private int _maxPartySize = 4;
    private readonly List<UnitSO> _currentParty = new();

    public void Setup(GameFlowController flow)
    {
        _flow = flow;
    }

    public void EnterMode()
    {
        Debug.Log("[RecruitmentManager] 영입 페이즈 시작");
        gameObject.SetActive(true);
        _recruitmentCount = 0;
        _currentParty.Clear();

        var allInfos = GameManager.I?.Data?.GetAllRecruitmentInfos();
        if (allInfos == null || allInfos.Count < 2)
        {
            Debug.LogError("DataCatalog에 영입 정보(RecruitmentInfoSO)가 2개 미만입니다!");
            EndRecruitment();
            return;
        }

        int candidatesToPull = _maxPartySize * 2;
        _sessionCandidates = allInfos.OrderBy(x => Random.value).Take(candidatesToPull).ToList();

        _partySetupView.BuildSlots(_maxPartySize, SlotLabel, OnClickSlot);
        RefreshSlots();
        ShowNextChoice();
    }

    public void ExitMode()
    {
        gameObject.SetActive(false);
    }

    private void ShowNextChoice()
    {
        if (_currentParty.Count >= _maxPartySize || _sessionCandidates.Count < 2)
        {
            EndRecruitment();
            return;
        }

        var choiceA = _sessionCandidates[0];
        var choiceB = _sessionCandidates[1];
        _recruitmentUI.ShowChoice(choiceA, choiceB, OnCharacterChosen);
    }

    private void OnCharacterChosen(UnitSO chosenUnit, RecruitmentInfoSO chosenInfo, RecruitmentInfoSO otherInfo)
    {
        if (_currentParty.Count < _maxPartySize)
        {
            _currentParty.Add(chosenUnit);
        }

        _recruitmentCount++;
        Debug.Log($"{chosenUnit.displayName}을(를) 파티에 영입했습니다! ({_currentParty.Count}/{_maxPartySize})");

        _sessionCandidates.Remove(chosenInfo);
        _sessionCandidates.Remove(otherInfo);

        RefreshSlots();
        ShowNextChoice();
    }

    private void EndRecruitment()
    {
        Debug.Log("파티원 구성 완료! 맵으로 이동합니다.");

        var run = GameManager.I?.CurrentRun;
        if (run != null)
        {
            // 최종 파티 구성을 RunManager에 저장
            run.SetPartyFromUnitSOs(_currentParty);

            // --- ✨ 여기가 핵심 수정 부분 ✨ ---
            // 시작 키트에 아이템이 있다면 인벤토리에 추가
            if (_startingKit != null)
            {
                foreach (var itemSO in _startingKit.startingItems)
                {
                    if (itemSO != null)
                    {
                        run.AddItemToInventory(itemSO);
                    }
                }
                Debug.Log($"시작 키트 '{_startingKit.name}'의 아이템들을 추가했습니다.");
            }
        }

        _flow?.RequestMap();
    }

    private string SlotLabel(int i)
    {
        if (i < _currentParty.Count && _currentParty[i] != null)
            return _currentParty[i].displayName;
        return "(빈 슬롯)";
    }

    private void RefreshSlots()
    {
        for (int i = 0; i < _maxPartySize; i++)
            _partySetupView.SetSlotLabel(i, SlotLabel(i));
    }

    private void OnClickSlot(int index)
    {
        Debug.Log("영입 중에는 파티원을 뺄 수 없습니다.");
    }
}