using System.Collections.Generic;
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
