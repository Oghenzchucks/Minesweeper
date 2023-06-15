using System.Collections;
using System.Collections.Generic;
using Config;
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

        private ConfigService _configService;
        private InputController _inputController;
        private TileStateSetter _tileStateSetter;
        private TileStatesActivator _tileStatesActivator;
        
        private bool _gameOver, _isShowingWarning;
        private Coroutine _countDownCoroutine;

        private void Awake()
        {
            _configService = new ConfigService();
            _inputController = new InputController();
            _tileStateSetter = new TileStateSetter();

            _tileStatesActivator = new TileStatesActivator();
            _tileStatesActivator.OnOpenTileStates += OnOpenTileStates;

            GameEventSystem.Instance.StartGame += StartGame;
            GameEventSystem.Instance.ExitGame += OnExit;
            
            GameEventSystem.Instance.InputActionChange += OnInputActionChange;
            GameEventSystem.Instance.OnTileClick += OnTileClick;
        }

        private void OnDestroy()
        {
            GameEventSystem.Instance.StartGame -= StartGame;
            GameEventSystem.Instance.ExitGame -= OnExit;
            
            _tileStatesActivator.OnOpenTileStates -= OnOpenTileStates;
            GameEventSystem.Instance.InputActionChange -= OnInputActionChange;
            GameEventSystem.Instance.OnTileClick -= OnTileClick;
        }

        private void StartGame()
        {
            StopCountDown();
            
            gameState.Initialize(_configService.LoadFromJson());
            _tileStateSetter.SetTileStatesType(gameState);
            _tileStatesActivator.Initialize(gameState);

            _inputController.handleInput = true;
            GameEventSystem.Instance.OnInitializeUI?.Invoke(gameState.GetTileStates, gameState.totalMines);
            GameEventSystem.Instance.OnTimeCountUpdate?.Invoke(gameState.currentTime);
            _countDownCoroutine = StartCoroutine(CountTime());
        }

        private void OnExit()
        {
            StopCountDown();
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

            _inputController.handleInput = false;
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
                    StopCountDown();
                    ShowResult(false);
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
            if (_isShowingWarning && !tileState.isFlagged)
            {
                return;
            }

            if (_isShowingWarning)
            {
                ShowWarningMessage(false);
            }

            UpdateMarkedMineCount(tileState);
            UpdateMineCount();
            GameEventSystem.Instance.OnMarkMine?.Invoke(tileState);

            EndGameCheck();
        }

        private void UpdateMarkedMineCount(TileState tileState)
        {
            tileState.isFlagged = !tileState.isFlagged;
            if (tileState.isFlagged && gameState.totalMinesFlagged < gameState.totalMines)
            {
                gameState.totalMinesFlagged++;
            }
            else if (!tileState.isFlagged && gameState.totalMinesFlagged > 0)
            {
                gameState.totalMinesFlagged--;
            }
        }

        private void EndGameCheck()
        {
            if (gameState.IsMatched)
            {
                if (gameState.IsGameFinished())
                {
                    ShowResult(true);
                }
                else
                {
                    ShowWarningMessage(true);
                }
            }
            else
            {
                _inputController.handleInput = true;
            }
        }

        private void ShowWarningMessage(bool showWarning)
        {
            _inputController.handleInput = true;
            _isShowingWarning = showWarning;
            GameEventSystem.Instance.OnWarningAction?.Invoke(showWarning);
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
        
        private void ShowResult(bool hasPassed)
        {
            _gameOver = false;
            _isShowingWarning = false;
            GameEventSystem.Instance.OnShowResult?.Invoke(hasPassed);
        }
    }
}
