using System;
using Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class TileView : MonoBehaviour
    {
        [SerializeField] private Image tileIcon;
        [SerializeField] private TextMeshProUGUI tilePositionText;
        [SerializeField] private Button button;

        public TileState GetTileState { get; private set; }

        public Action<TileState> OnClick;

        private void Awake()
        {
            button.onClick.AddListener(() => OnClick?.Invoke(GetTileState));
        }

        private void OnDisable()
        {
            button.onClick.RemoveAllListeners();
        }

        public void UpdateData(TileState tileState, bool showPosition)
        {
            GetTileState = tileState;
            tilePositionText.text = tileState.tilePosition.row + "-" + tileState.tilePosition.column;
            tilePositionText.gameObject.SetActive(showPosition);
        }

        public void UpdateSprite(string spriteName)
        {
            tileIcon.sprite = SpriteLoader.GetSprite(spriteName);
        }
    }
}