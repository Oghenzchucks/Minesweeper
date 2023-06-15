using System.Collections;
using System.Collections.Generic;
using Enums;
using Events;
using Models;
using State;
using UnityEngine;

namespace Controllers
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private GameState gameState;
        [SerializeField] private int countDownRate;
        
        private InputController _inputController;
        private TileStateSetter _tileStateSetter;
        private TileStatesActivator _tileStatesActivator;
        
        private bool _gameOver;
        private Coroutine _countDownCoroutine;

        private void Awake()
        {
            gameState.Initialize();
            
            _inputController = new InputController();
            
            _tileStateSetter = new TileStateSetter();
            _tileStateSetter.SetTileStatesType(gameState);

            _tileStatesActivator = new TileStatesActivator();
            _tileStatesActivator.Initialize(gameState);
            _tileStatesActivator.OnOpenTileStates += OnOpenTileStates;

            GameEventSystem.Instance.InputActionChange += OnInputActionChange;
            GameEventSystem.Instance.OnTileClick += OnTileClick;
        }

        private void Start()
        {
            _inputController.handleInput = true;
            GameEventSystem.Instance.OnInitializeUI?.Invoke(gameState.GetTileStates, gameState.totalMines);
            GameEventSystem.Instance.OnTimeCountUpdate?.Invoke(gameState.currentTime);
            _countDownCoroutine = StartCoroutine(CountTime());
        }

        private void OnDisable()
        {
            _tileStatesActivator.OnOpenTileStates -= OnOpenTileStates;
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

            switch (_inputController.GetInputAction)
            {
                case InputAction.FindMine:
                    HandleFindMineAction(tileState);
                    break;
                case InputAction.MarkMine:
                    HandleMarkMineAction(tileState);
                    break;
            }
        }

        private void HandleFindMineAction(TileState tileState)
        {
            if (tileState.isFlagged)
            {
                return;
            }

            _inputController.handleInput = false;
            switch (tileState.tileType)
            {
                case TileTypes.Empty:
                    _tileStatesActivator.AddToCheckTilesQueue(tileState);
                    return;
                case TileTypes.Mine:
                    tileState.tileType = TileTypes.HitMine;
                    foreach (var tile in gameState.GetTileStates)
                    {
                        tile.isOpen = true;
                    }
                    GameEventSystem.Instance.OpenAllTiles?.Invoke(gameState.GetTileStates);
                    _inputController.handleInput = true;
                    return;
                default:
                    tileState.isOpen = true;
                    GameEventSystem.Instance.OnTilesToOpen?.Invoke(new List<TileState>() { tileState });
                    _inputController.handleInput = true;
                    return;
            }
        }

        private void OnOpenTileStates(List<TileState> tileStates)
        {
            GameEventSystem.Instance.OnTilesToOpen?.Invoke(tileStates);
            _inputController.handleInput = true;
        }

        private void HandleMarkMineAction(TileState tileState)
        {
            tileState.isFlagged = !tileState.isFlagged;
            if (tileState.isFlagged && gameState.totalMinesFlagged <= gameState.totalMines)
            {
                gameState.totalMinesFlagged++;
            }
            else if (!tileState.isFlagged && gameState.totalMinesFlagged >= 0)
            {
                gameState.totalMinesFlagged--;
            }
            GameEventSystem.Instance.OnMarkMine?.Invoke(tileState);
            UpdateMineCount();
        }

        private void UpdateMineCount()
        {
            GameEventSystem.Instance.OnMineCountUpdate?.Invoke(gameState.totalMinesFlagged + " / " + gameState.totalMines);
        }

        private IEnumerator CountTime()
        {
            yield return new WaitForSeconds(countDownRate);
            gameState.currentTime++;
            GameEventSystem.Instance.OnTimeCountUpdate?.Invoke(gameState.currentTime);
            _countDownCoroutine = !_gameOver ? StartCoroutine(CountTime()) : null;
        }

        private void StopCountDown()
        {
            if (_countDownCoroutine == null)
            {
                return;
            }
            
            StopCoroutine(_countDownCoroutine);
            _countDownCoroutine = null;
        }
    }
}
