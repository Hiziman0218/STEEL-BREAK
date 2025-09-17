using Ilumisoft.RadarSystem.UI;
using System.Collections.Generic;
using UnityEngine;

namespace Ilumisoft.RadarSystem
{
    [AddComponentMenu("Radar System/Radar")]
    [DefaultExecutionOrder(-10)]
    public class Radar : MonoBehaviour
    {
        readonly Dictionary<LocatableComponent, LocatableIconComponent> locatableIconDictionary = new();

        [SerializeField]
        private RectTransform iconContainer;

        [SerializeField, Min(1)]
        private float range = 20;

        [SerializeField]
        private bool applyRotation = true;

        public float Range { get => range; set => range = value; }
        public bool ApplyRotation { get => applyRotation; set => applyRotation = value; }

        public  Player player;

        private void Start()
        {
            //player = GetComponent<Player>();
            player = FindObjectOfType<Player>();
        }

        private void OnEnable()
        {
            LocatableManager.OnLocatableAdded += OnLocatableAdded;
            LocatableManager.OnLocatableRemoved += OnLocatableRemoved;
        }

        private void OnDisable()
        {
            LocatableManager.OnLocatableAdded -= OnLocatableAdded;
            LocatableManager.OnLocatableRemoved -= OnLocatableRemoved;
        }

        private void OnLocatableAdded(LocatableComponent locatable)
        {
            if (locatable != null && !locatableIconDictionary.ContainsKey(locatable))
            {
                // "Enemy"タグのオブジェクトだけ表示
                if (!locatable.CompareTag("Enemy")) return;

                var icon = locatable.CreateIcon();
                icon.transform.SetParent(iconContainer.transform, false);
                locatableIconDictionary.Add(locatable, icon);
            }
        }

        private void OnLocatableRemoved(LocatableComponent locatable)
        {
            if (locatable != null && locatableIconDictionary.TryGetValue(locatable, out LocatableIconComponent icon))
            {
                locatableIconDictionary.Remove(locatable);
                Destroy(icon.gameObject);
            }
        }

        private void Update()
        {
            if (player != null)
            {
                UpdateLocatableIcons();
            }
        }

        private void UpdateLocatableIcons()
        {
            foreach (var locatable in locatableIconDictionary.Keys)
            {
                if (locatableIconDictionary.TryGetValue(locatable, out var icon))
                {
                    if (TryGetIconLocation(locatable, out var iconLocation))
                    {
                        icon.SetVisible(true);
                        var rectTransform = icon.GetComponent<RectTransform>();
                        rectTransform.anchoredPosition = iconLocation;
                    }
                    else
                    {
                        icon.SetVisible(false);
                    }
                }
            }
        }

        private bool TryGetIconLocation(LocatableComponent locatable, out Vector2 iconLocation)
        {
            iconLocation = GetDistanceToPlayer(locatable);
            float radarSize = GetRadarUISize();
            var scale = radarSize / Range;
            iconLocation *= scale;

            if (ApplyRotation)
            {
                var forward = Vector3.ProjectOnPlane(player.transform.forward, Vector3.up);
                var rotation = Quaternion.LookRotation(forward);
                var euler = rotation.eulerAngles;
                euler.y = -euler.y;
                rotation.eulerAngles = euler;
                var rotated = rotation * new Vector3(iconLocation.x, 0.0f, iconLocation.y);
                iconLocation = new Vector2(rotated.x, rotated.z);
            }

            if (iconLocation.sqrMagnitude < radarSize * radarSize || locatable.ClampOnRadar)
            {
                iconLocation = Vector2.ClampMagnitude(iconLocation, radarSize);
                return true;
            }

            return false;
        }

        private float GetRadarUISize()
        {
            return iconContainer.rect.width / 2f;
        }

        private Vector2 GetDistanceToPlayer(LocatableComponent locatable)
        {
            Vector3 distance = locatable.transform.position - player.transform.position;
            return new Vector2(distance.x, distance.z);
        }
    }
}
