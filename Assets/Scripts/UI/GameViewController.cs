using System;
using System.Collections.Generic;
using Enums;
using Events;
using Models;
using TMPro;
using UnityEngine;

namespace UI
{
    public class GameViewController : MonoBehaviour
    {
        [SerializeField] private TileView tileViewPrefab;
        [SerializeField] private Transform tileViewParent;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI minesDataText;
        [SerializeField] private bool showTileViewsPosition;
        [SerializeField] private bool showMines;

        private List<TileView> _tileViews = new List<TileView>();

        private void Awake()
        {
            GameEventSystem.Instance.OnInitializeUI += OnInitializeUI;
        }

        private void OnDisable()
        {
            GameEventSystem.Instance.OnInitializeUI -= OnInitializeUI;
        }

        private void OnInitializeUI(List<TileState> tileStates, int totalCount)
        {
            minesDataText.text = "0 / " + totalCount;
            LoadTileViews(tileStates);
        }

        private void LoadTileViews(List<TileState> tileStates)
        {
            foreach (var tileState in tileStates)
            {
                var tileView = CreateTileView(tileState);
                tileView.OnClick += OnTileClick;
                if (showMines)
                {
                    tileView.UpdateSprite(GetTileTypeSprite(tileState.tileType));
                }
                _tileViews.Add(tileView);
            }
        }

        private TileView CreateTileView(TileState tileState)
        {
            var tileView = Instantiate(tileViewPrefab, tileViewParent);
            tileView.UpdateData(tileState, showTileViewsPosition);
            return tileView;
        }

        private void OnTileClick(TileState tileState)
        {
            //Handle click
        }

        private string GetTileTypeSprite(TileTypes tileType)
        {
            switch (tileType)
            {
                case TileTypes.Empty:
                    return "empty";
                case TileTypes.Mine:
                    return "mine";
            }
            
            return "empty";
        }

        private void UpdateTimer(int time)
        {
            timerText.text = time.ToString();
        }

        private void UpdateMinesData(string minesData)
        {
            minesDataText.text = minesData;
        }
    }
}
