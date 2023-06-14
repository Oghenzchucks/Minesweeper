using Events;
using Models;
using State;
using UnityEngine;

namespace Controllers
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private GameState gameState;
        private InputController _inputController;

        private void Awake()
        {
            if (!gameState.isInitialized)
            {
                gameState.Initialize();
            }

            _inputController = new InputController();
            
            GameEventSystem.Instance.InputActionChange += OnInputActionChange;
            GameEventSystem.Instance.OnTileClick += OnTileClick;
        }

        private void Start()
        {
            GameEventSystem.Instance.OnInitializeUI?.Invoke(gameState.GetTileStates, gameState.totalMines);
        }

        private void OnDisable()
        {
            GameEventSystem.Instance.InputActionChange -= OnInputActionChange;
            GameEventSystem.Instance.OnTileClick -= OnTileClick;
        }

        private void OnInputActionChange()
        {
            _inputController.ChangeInputAction();
            GameEventSystem.Instance.InputActionUpdate?.Invoke(_inputController.GetInputAction);
        }
        
        private void OnTileClick(TileState tileState)
        {
            if (!_inputController.handleInput)
            {
                return;
            }
            
            //Handle Click Action
        }
    }
}
