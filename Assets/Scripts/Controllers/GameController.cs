using Events;
using State;
using UnityEngine;

namespace Controllers
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private GameState gameState;

        private void Awake()
        {
            gameState.Initialize();
        }

        private void Start()
        {
            GameEventSystem.Instance.OnInitializeUI?.Invoke(gameState.GetTileStates, gameState.totalMines);
        }
    }
}
