using System;
using System.IO;
using UnityEngine;

namespace MainMenu
{
    /// <summary>
    /// 세이브 데이터 저장/로드 (JSON 파일, persistentDataPath)
    /// </summary>
    public static class SaveSystem
    {
        private const string SaveFileName = "save.json";

        public static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        /// <summary>
        /// 저장 데이터가 존재하는지
        /// </summary>
        public static bool HasSaveData()
        {
            return File.Exists(SavePath);
        }

        /// <summary>
        /// 현재 게임 상태를 저장
        /// </summary>
        public static void Save(GameSaveData data)
        {
            if (data == null) return;
            data.lastPlayTimeUtc = DateTime.UtcNow.ToString("o");
            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(SavePath, json);
                Debug.Log("[SaveSystem] 저장 완료: " + SavePath);
            }
            catch (Exception e)
            {
                Debug.LogError("[SaveSystem] 저장 실패: " + e.Message);
            }
        }

        /// <summary>
        /// 저장 데이터 로드. 없으면 null
        /// </summary>
        public static GameSaveData Load()
        {
            if (!File.Exists(SavePath))
            {
                Debug.Log("[SaveSystem] 저장 파일 없음");
                return null;
            }
            try
            {
                string json = File.ReadAllText(SavePath);
                var data = JsonUtility.FromJson<GameSaveData>(json);
                Debug.Log("[SaveSystem] 로드 완료 - 챕터:" + data.lastClearedChapter + " 골드:" + data.gold);
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError("[SaveSystem] 로드 실패: " + e.Message);
                return null;
            }
        }

        /// <summary>
        /// 저장 파일 삭제 (뉴 게임 시 사용 가능)
        /// </summary>
        public static void DeleteSave()
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
                Debug.Log("[SaveSystem] 저장 삭제됨");
            }
        }
    }
}
