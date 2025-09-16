// Assets/Game/Scripts/Services/SaveService.cs
using UnityEngine;

namespace Game.Services
{
    public class SaveService : MonoBehaviour
    {
        const string Key = "SAVE_SLOT_0";
        public void TryAutoSave()
        {
            PlayerPrefs.SetString(Key, "stub"); // 이후 실제 세이브 데이터 직렬화
            PlayerPrefs.Save();
        }

        public bool TryLoad(out string raw)
        {
            raw = PlayerPrefs.GetString(Key, "");
            return !string.IsNullOrEmpty(raw);
        }
    }
}
