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
        [SerializeField] private bool autoPlay;
        [SerializeField] private int waitTime;

        private ConfigService _configService;
        private InputController _inputController;
        private TileStateSetter _tileStateSetter;
        private TileStatesActivator _tileStatesActivator;
        private AutoPlayController _autoPlayController;
        
        private bool _gameOver, _isShowingWarning, _isWorking;
        private Coroutine _countDownCoroutine, _autoPlayCoroutine;

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
            GameEventSystem.Instance.OnTileClick += OnInputTileClick;
        }

        private void Update()
        {
            _inputController.OnUpdate();
        }

        private void OnDestroy()
        {
            GameEventSystem.Instance.StartGame -= StartGame;
            GameEventSystem.Instance.ExitGame -= OnExit;
            
            _tileStatesActivator.OnOpenTileStates -= OnOpenTileStates;
            GameEventSystem.Instance.InputActionChange -= OnInputActionChange;
            GameEventSystem.Instance.OnTileClick -= OnInputTileClick;
        }

        private void StartGame()
        {
            _gameOver = false;
            StopGameCoroutine(_countDownCoroutine);
            StopGameCoroutine(_autoPlayCoroutine);
            
            gameState.Initialize(_configService.LoadFromJson());
            _tileStateSetter.SetTileStatesType(gameState);
            _tileStatesActivator.Initialize(gameState);
            
            _autoPlayController = new AutoPlayController();

            GameEventSystem.Instance.OnInitializeUI?.Invoke(gameState.GetTileStates, gameState.totalMines);
            GameEventSystem.Instance.OnTimeCountUpdate?.Invoke(gameState.currentTime);
            _countDownCoroutine = StartCoroutine(CountTime());

            if (!autoPlay)
            {
                _inputController.handleInput = true;
            }
            else
            {
                StartAIMove();
            }
        }

        private void OnExit()
        {
            StopGameCoroutine(_countDownCoroutine);
            StopGameCoroutine(_autoPlayCoroutine);
        }

        private void OnInputActionChange()
        {
            _inputController.ChangeInputAction();
            GameEventSystem.Instance.InputActionUpdate?.Invoke(_inputController.GetInputAction);
        }

        private void OnInputTileClick(TileState tileState)
        {
            if (autoPlay)
            {
                return;
            }
            
            if (!_inputController.handleInput)
            {
                return;
            }

            OnTileClick(tileState);
        }
        
        private void OnTileClick(TileState tileState)
        {
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
                    StopGameCoroutine(_countDownCoroutine);
                    StopGameCoroutine(_autoPlayCoroutine);
                    ShowResult(false);
                    _isWorking = false;
                    return;
                default:
                    tileState.isOpen = true;
                    GameEventSystem.Instance.OnTilesToOpen?.Invoke(new List<TileState>() { tileState });
                    _inputController.handleInput = true;
                    _isWorking = false;
                    return;
            }
        }

        private void OnOpenTileStates(List<TileState> tileStates)
        {
            GameEventSystem.Instance.OnTilesToOpen?.Invoke(tileStates);
            _inputController.handleInput = true;
            _isWorking = false;
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
            _isWorking = false;

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

        private void StopGameCoroutine(Coroutine coroutine)
        {
            if (coroutine == null)
            {
                return;
            }
            
            StopCoroutine(coroutine);
        }
        
        private void ShowResult(bool hasPassed)
        {
            _gameOver = true;
            _isShowingWarning = false;
            GameEventSystem.Instance.OnShowResult?.Invoke(hasPassed);
        }
        
        private void StartAIMove()
        {
            var autoPlayData = _autoPlayController.GetAIMove(gameState);
            if (autoPlayData != null)
            {
                _isWorking = true;
                _inputController.UpdateInputAction(autoPlayData.inputAction);
                OnTileClick(autoPlayData.tileState);
                _autoPlayCoroutine = StartCoroutine(PlayAINextMove());
            }
            else
            {
                Debug.Log("no move");
            }
        }

        private IEnumerator PlayAINextMove()
        {
            yield return new WaitUntil(() => !_isWorking);
            yield return new WaitForSeconds(waitTime);
            if (!_gameOver)
            {
                StartAIMove();
            }
        }
    }
}
