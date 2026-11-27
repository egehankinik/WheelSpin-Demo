using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Core.Events;
using WheelOfFortune.UI.Events;

namespace WheelOfFortune.UI.Rewards
{
    public sealed class RewardFlyAnimatorMono : MonoBehaviour
    {
        #region Fields

        [SerializeField] private RectTransform _flyLayer;
        [SerializeField] private Image _iconPrefab;
        [SerializeField, Min(1)] private int _iconCount = 5;
        [SerializeField] private Vector2 _iconSize = new Vector2(90f, 90f);
        [SerializeField, Min(0f)] private float _burstRadius = 120f;
        [SerializeField, Min(0f)] private float _burstDuration = 0.25f;
        [SerializeField, Min(0f)] private float _flyDuration = 0.5f;
        [SerializeField, Min(0f)] private float _stagger = 0.06f;
        [SerializeField, Range(0f, 1f)] private float _arrivalScale = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _shrinkStartRatio = 0.6f;
        [SerializeField] private Ease _burstScaleEase = Ease.OutBack;
        [SerializeField] private Ease _burstMoveEase = Ease.OutCubic;
        [SerializeField] private Ease _flyEase = Ease.InCubic;
        [SerializeField] private Ease _shrinkEase = Ease.OutQuad;

        private readonly List<Image> _icons = new List<Image>();
        private IEventBus _events;
        private Sequence _sequence;
        private Action _onComplete;

        #endregion

        #region Unity Methods

        private void OnDisable()
        {
            var isInterrupted = _sequence.IsActive();
            _sequence?.Kill();
            _onComplete = null;
            HideIcons();
            if (isInterrupted)
            {
                _events?.Publish(new SpinAnimationInterruptedEvent());
            }
        }

        #endregion

        #region Public Methods

        public void Initialize(IEventBus events)
        {
            _events = events;
        }

        public void Play(Sprite icon, RectTransform from, RectTransform to, Action onComplete)
        {
            _sequence?.Kill(true);
            _onComplete = onComplete;
            _sequence = DOTween.Sequence().SetLink(gameObject);

            var start = from.position;
            var end = to.position;
            var worldBurstRadius = _burstRadius * _flyLayer.lossyScale.x;

            for (var i = 0; i < _iconCount; i++)
            {
                var rect = PrepareIcon(i, icon, start);
                var burstPoint = start + (Vector3)(UnityEngine.Random.insideUnitCircle * worldBurstRadius);
                var delay = i * _stagger;
                var flyStart = delay + _burstDuration;
                var shrinkStart = flyStart + _flyDuration * _shrinkStartRatio;

                _sequence.Insert(delay, rect.DOScale(1f, _burstDuration).SetEase(_burstScaleEase));
                _sequence.Insert(delay, rect.DOMove(burstPoint, _burstDuration).SetEase(_burstMoveEase));
                _sequence.Insert(flyStart, rect.DOMove(end, _flyDuration).SetEase(_flyEase));
                _sequence.Insert(shrinkStart, rect.DOScale(_arrivalScale, _flyDuration * (1f - _shrinkStartRatio)).SetEase(_shrinkEase));
            }

            _sequence.OnComplete(HandleSequenceCompleted);
        }

        #endregion

        #region Private Methods

        private RectTransform PrepareIcon(int index, Sprite sprite, Vector3 position)
        {
            while (_icons.Count <= index)
            {
                var icon = Instantiate(_iconPrefab, _flyLayer);
                icon.name = _iconPrefab.name;
                _icons.Add(icon);
            }

            var image = _icons[index];
            image.sprite = sprite;
            image.gameObject.SetActive(true);

            var rect = image.rectTransform;
            rect.sizeDelta = _iconSize;
            rect.position = position;
            rect.localScale = Vector3.zero;
            return rect;
        }

        private void HideIcons()
        {
            foreach (var icon in _icons)
            {
                icon.gameObject.SetActive(false);
            }
        }

        private void HandleSequenceCompleted()
        {
            HideIcons();
            var onComplete = _onComplete;
            _onComplete = null;
            onComplete?.Invoke();
        }

        #endregion
    }
}
