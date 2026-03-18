using UnityEngine;

namespace Match3Puzzle.Core
{
    /// <summary>
    /// 배틀 씬 로드 시 보드 초기화 및 Playing 상태로 시작.
    /// LevelManager의 Moves Remaining(최대 턴)은 Inspector에서 20으로 설정.
    /// </summary>
    public class BattleSceneStarter : MonoBehaviour
    {
        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameState.Playing);
                GameManager.Instance.StartGame();
            }
        }
    }
}