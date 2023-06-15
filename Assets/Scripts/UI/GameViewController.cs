using System.Collections.Generic;
using System.Linq;
using Enums;
using Events;
using Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class GameViewController : MonoBehaviour
    {
        [SerializeField] private GameObject startScreen, inGameScreen, resultScreen, restartButtonParent;
        [SerializeField] private TextMeshProUGUI message;
        [SerializeField] private Button start, restart, exit;
        
        [SerializeField] private TileView tileViewPrefab;
        [SerializeField] private Transform tileViewParent;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI minesCountDataText;
        
        [SerializeField] private Image inputActionDisplay;
        [SerializeField] private Button inputActionButton;
        
        [SerializeField] private bool showTileViewsPosition;
        [SerializeField] private bool showTiles;

        private List<TileView> _tileViews = new();

        private void Awake()
        {
            GameEventSystem.Instance.OnInitializeUI += OnInitializeUI;
            GameEventSystem.Instance.InputActionUpdate += UpdateInputActionSprite;
            GameEventSystem.Instance.OnTilesToOpen += OnUpdateTileViews;
            GameEventSystem.Instance.OpenAllTiles += OpenAllTiles;
            GameEventSystem.Instance.OnMarkMine += OnMarkMine;
            GameEventSystem.Instance.OnMineCountUpdate += OnMineCountUpdate;
            GameEventSystem.Instance.OnTimeCountUpdate += UpdateTimer;
            GameEventSystem.Instance.OnShowResult += OnShowResult;
            GameEventSystem.Instance.OnWarningAction += OnWarningAction;
            
            start.onClick.AddListener(OnStart);
            restart.onClick.AddListener(OnRestart);
            exit.onClick.AddListener(OnExit);
            
            inputActionButton.onClick.AddListener(() => GameEventSystem.Instance.InputActionChange?.Invoke());
        }

        private void OnDestroy()
        {
            GameEventSystem.Instance.OnInitializeUI -= OnInitializeUI;
            GameEventSystem.Instance.InputActionUpdate -= UpdateInputActionSprite;
            GameEventSystem.Instance.OnTilesToOpen -= OnUpdateTileViews;
            GameEventSystem.Instance.OpenAllTiles -= OpenAllTiles;
            GameEventSystem.Instance.OnMarkMine -= OnMarkMine;
            GameEventSystem.Instance.OnMineCountUpdate -= OnMineCountUpdate;
            GameEventSystem.Instance.OnTimeCountUpdate -= UpdateTimer;
            GameEventSystem.Instance.OnShowResult -= OnShowResult;
            GameEventSystem.Instance.OnWarningAction -= OnWarningAction;

            start.onClick.RemoveAllListeners();
            restart.onClick.RemoveAllListeners();
            exit.onClick.RemoveAllListeners();
            inputActionButton.onClick.RemoveAllListeners();
        }

        private void OnStart()
        {
            message.text = "";
            ActivateScreens(false, true, false);
            restartButtonParent.SetActive(true);
            GameEventSystem.Instance.StartGame?.Invoke();
        }

        private void OnRestart()
        {
            message.text = "";
            restartButtonParent.SetActive(true);
            ActivateScreens(false, true, false);
            GameEventSystem.Instance.StartGame?.Invoke();
        }

        private void OnExit()
        {
            ActivateScreens(true, false, false);
            GameEventSystem.Instance.ExitGame?.Invoke();
        }

        private void OnInitializeUI(List<TileState> tileStates, int totalCount)
        {
            minesCountDataText.text = "0 / " + totalCount;
            LoadTileViews(tileStates);
        }

        private void LoadTileViews(List<TileState> tileStates)
        {
            DestroyOldTiles();
            
            foreach (var tileState in tileStates)
            {
                var tileView = CreateTileView(tileState);
                tileView.OnClick += OnTileClick;
                _tileViews.Add(tileView);
                
                if (showTiles)
                {
                    tileView.UpdateSprite(GetTileTypeSprite(tileState.tileType));
                }
            }
        }

        private void DestroyOldTiles()
        {
            foreach (var tileView in _tileViews)
            {
                tileView.OnClick = null;
                Destroy(tileView.gameObject);
            }
            _tileViews.Clear();
        }

        private TileView CreateTileView(TileState tileState)
        {
            var tileView = Instantiate(tileViewPrefab, tileViewParent);
            tileView.UpdateData(tileState, showTileViewsPosition);
            return tileView;
        }

        private void OnTileClick(TileState tileState)
        {
            GameEventSystem.Instance.OnTileClick?.Invoke(tileState);
        }
        
        private void OnMarkMine(TileState tileState)
        {
            var tileView = _tileViews.First(x => x.GetTileState.tilePosition.IsEqual(tileState.tilePosition));
            tileView.UpdateSprite(tileState.isFlagged ? "flag" : "unclicked");
        }

        private void OnUpdateTileViews(List<TileState> tileStates)
        {
            foreach (var tileState in tileStates)
            {
                var tileView = _tileViews.First(x => x.GetTileState.tilePosition.IsEqual(tileState.tilePosition));
                tileView.UpdateSprite(GetTileTypeSprite(tileView.GetTileState.tileType));
            }
        }
        
        private void OpenAllTiles(List<TileState> tileStates)
        {
            foreach (var tileState in tileStates)
            {
                var tileView = _tileViews.First(x => x.GetTileState.tilePosition.IsEqual(tileState.tilePosition));
                tileView.UpdateSprite(tileState.isFlagged && tileState.tileType != TileTypes.Mine ? "wrongmine" : GetTileTypeSprite(tileView.GetTileState.tileType));
            }
        }

        private string GetTileTypeSprite(TileTypes tileType)
        {
            switch (tileType)
            {
                case TileTypes.Empty:
                    return "empty";
                case TileTypes.Mine:
                    return "mine";
                case TileTypes.HitMine:
                    return "redmine";
                case TileTypes.One:
                    return "1";
                case TileTypes.Two:
                    return "2";
                case TileTypes.Three:
                    return "3";
                case TileTypes.Four:
                    return "4";
                case TileTypes.Five:
                    return "5";
                case TileTypes.Six:
                    return "6";
                case TileTypes.Seven:
                    return "7";
                case TileTypes.Eight:
                    return "8";
            }
            
            return "";
        }

        private void UpdateTimer(int time)
        {
            timerText.text = time.ToString();
        }

        private void OnMineCountUpdate(string minesData)
        {
            minesCountDataText.text = minesData;
        }

        private void UpdateInputActionSprite(InputAction inputAction)
        {
            inputActionDisplay.sprite = SpriteLoader.GetSprite(GetInputActionSprite(inputAction));
        }
        
        private string GetInputActionSprite(InputAction inputAction)
        {
            switch (inputAction)
            {
                case InputAction.FindMine:
                    return "mine";
                case InputAction.MarkMine:
                    return "flag";
            }
            
            return "";
        }
        
        private void OnShowResult(bool hasPassed)
        {
            restartButtonParent.SetActive(false);
            message.text = hasPassed ? "YOU WON" : "YOU LOST";
            message.color = hasPassed ? Color.green : Color.red;
            resultScreen.SetActive(true);
        }
        
        private void OnWarningAction(bool showWarning)
        {
            message.text = showWarning ? "Keep Trying" : "";
            message.color = Color.yellow;
        }
        
        private void ActivateScreens(bool showStart, bool showInGame, bool showResult)
        {
            startScreen.SetActive(showStart);
            inGameScreen.SetActive(showInGame);
            resultScreen.SetActive(showResult);
        }
    }
}
