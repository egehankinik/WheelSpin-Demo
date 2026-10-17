using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace WheelOfFortune.UI.Rewards
{
    public sealed class RewardFlyAnimator : MonoBehaviour
    {
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

        private readonly List<Image> _icons = new List<Image>();
        private Sequence _sequence;

        public void Play(Sprite icon, RectTransform from, RectTransform to, Action onComplete)
        {
            _sequence?.Kill(true);
            _sequence = DOTween.Sequence();

            var start = from.position;
            var end = to.position;
            var worldBurstRadius = _burstRadius * _flyLayer.lossyScale.x;

            for (var i = 0; i < _iconCount; i++)
            {
                var rect = PrepareIcon(i, icon, start);
                var burstPoint = start + (Vector3)(UnityEngine.Random.insideUnitCircle * worldBurstRadius);
                var delay = i * _stagger;
                var flyStart = delay + _burstDuration;

                _sequence.Insert(delay, rect.DOScale(1f, _burstDuration).SetEase(Ease.OutBack));
                _sequence.Insert(delay, rect.DOMove(burstPoint, _burstDuration).SetEase(Ease.OutCubic));
                _sequence.Insert(flyStart, rect.DOMove(end, _flyDuration).SetEase(Ease.InCubic));
                _sequence.Insert(flyStart + _flyDuration * _shrinkStartRatio, rect.DOScale(_arrivalScale, _flyDuration * (1f - _shrinkStartRatio)));
            }

            _sequence.OnComplete(() =>
            {
                HideIcons();
                onComplete?.Invoke();
            });
        }

        private RectTransform PrepareIcon(int index, Sprite sprite, Vector3 position)
        {
            while (_icons.Count <= index)
            {
                _icons.Add(Instantiate(_iconPrefab, _flyLayer));
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

        private void OnDisable()
        {
            _sequence?.Kill();
            HideIcons();
        }
    }
}
