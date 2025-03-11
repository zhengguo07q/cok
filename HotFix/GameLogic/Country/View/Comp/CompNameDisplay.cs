using GameBase.Utility;
using GameLogic.Country.View.Object;
using TEngine;
using TMPro;
using UnityEngine;

namespace GameLogic.Country.View.Component
{
    public class CompNameDisplay : ComponentBase
    {
        private GameObject fullNameGo;
        private GameObject shortNameGo;

        // 持有 SceneObject 的引用
        public SceneObject SceneObject { get; set; }

        public override void Initialize()
        {
            if (SceneObject == null)
            {
                Debug.LogError("NameDisplayComponent.Initialize: SceneObject is not set!");
                return;
            }
            LoadFullName();
            LoadShortName();
        }

        private void LoadFullName()
        {
            var fullName = "UI_FullName";
            GameModule.Resource.LoadAsset<GameObject>(fullName, goPrefab =>
            {
                if (goPrefab == null) return;

                fullNameGo = GameObject.Instantiate(goPrefab);
                fullNameGo.name = fullName;
                fullNameGo.transform.SetParent(SceneObject.ObjectView.transform, false); // 使用 SceneObject.ObjectView

                // 配置 Canvas
                var canvas = fullNameGo.GetComponent<Canvas>();
                if (canvas == null)
                {
                    canvas = fullNameGo.AddComponent<Canvas>();
                }
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = Camera.main;
                canvas.sortingOrder = 2;

                var tilemap = SceneObject.SceneRef.Scene.TileLayer.Tilemap; // 使用 SceneObject.SceneRef
                if (tilemap == null) return;

                var cellSize = tilemap.cellSize;
                var canvasRT = canvas.GetComponent<RectTransform>();
                float referencePixelsPerUnit = 100f;
                float worldUnitsPerTile = cellSize.x;
                float uiTileRatio = 0.5f;
                float scaleFactor = (worldUnitsPerTile * uiTileRatio) / referencePixelsPerUnit;
                canvasRT.localScale = Vector3.one * scaleFactor;
                float aspectRatio = 0.3f;
                canvasRT.sizeDelta = new Vector2(100, 100 * aspectRatio);

                SpriteRenderer objectSprite = SceneObject.ObjectView.transform.GetComponentInChildren<SpriteRenderer>(); // 使用 SceneObject.ObjectView
                if (objectSprite != null && objectSprite.sprite != null)
                {
                    var bounds = objectSprite.sprite.bounds;
                    float verticalOffset = bounds.extents.y + (worldUnitsPerTile * 0.2f);
                    canvasRT.localPosition = new Vector3(0, verticalOffset, 0);
                }
                else
                {
                    canvasRT.localPosition = new Vector3(0, worldUnitsPerTile * 0.5f, 0);
                }

                var nameText = TransformUtility.FindChildComponent<TextMeshProUGUI>(fullNameGo.transform, "nameBg/m_TxtName");
                if (nameText != null)
                {
                    nameText.text = SceneObject.SceneObjectInfo.Name; // 使用 SceneObject.SceneObjectInfo
                    nameText.fontSize = 24 / scaleFactor;
                }

                var levelText = TransformUtility.FindChildComponent<TextMeshProUGUI>(fullNameGo.transform, "levelBg/m_TxtLevel");
                if (levelText != null)
                {
                    levelText.text = $"{SceneObject.SceneObjectInfo.Id}"; // 使用 SceneObject.SceneObjectInfo
                    levelText.fontSize = 24 / scaleFactor;
                }

                LayerUtility.SetCameraIndex(fullNameGo, SceneObject.LayerIndexInfo.CameraIndex);
                LayerUtility.SetLayerIndexInRender(fullNameGo, SceneObject.LayerIndexInfo.LayerIndex);
            });
        }

        private void LoadShortName()
        {
            var shortName = "UI_ShortName";
            GameModule.Resource.LoadAsset<GameObject>(shortName, goPrefab =>
            {
                if (goPrefab == null) return;

                shortNameGo = GameObject.Instantiate(goPrefab);
                shortNameGo.name = shortName;
                shortNameGo.transform.SetParent(SceneObject.IconView.transform, false); // 使用 SceneObject.IconView

                // 配置 Canvas
                var canvas = shortNameGo.GetComponent<Canvas>();
                if (canvas == null)
                {
                    canvas = shortNameGo.AddComponent<Canvas>();
                }
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = Camera.main;
                canvas.sortingOrder = 2;

                var tilemap = SceneObject.SceneRef.Scene.TileLayer.Tilemap; // 使用 SceneObject.SceneRef
                if (tilemap == null) return;

                var cellSize = tilemap.cellSize;
                var canvasRT = canvas.GetComponent<RectTransform>();
                float referencePixelsPerUnit = 100f;
                float worldUnitsPerTile = cellSize.x;
                float uiTileRatio = 0.3f;
                float scaleFactor = (worldUnitsPerTile * uiTileRatio) / referencePixelsPerUnit;
                canvasRT.localScale = Vector3.one * scaleFactor;
                float aspectRatio = 0.4f;
                canvasRT.sizeDelta = new Vector2(100, 100 * aspectRatio);

                SpriteRenderer iconSprite = SceneObject.IconView.transform.GetComponentInChildren<SpriteRenderer>(); // 使用 SceneObject.IconView
                if (iconSprite != null && iconSprite.sprite != null)
                {
                    var bounds = iconSprite.sprite.bounds;
                    float verticalOffset = -bounds.extents.y - (worldUnitsPerTile * 0.1f);
                    canvasRT.localPosition = new Vector3(0, verticalOffset, 0);
                }
                else
                {
                    canvasRT.localPosition = new Vector3(0, -worldUnitsPerTile * 0.3f, 0);
                }

                var shortLevelTxt = TransformUtility.FindChildComponent<TextMeshProUGUI>(shortNameGo.transform, "levelBg/m_TxtLevel");
                if (shortLevelTxt != null)
                {
                    shortLevelTxt.text = $"{SceneObject.SceneObjectInfo.Level}"; // 使用 SceneObject.SceneObjectInfo
                    shortLevelTxt.fontSize = 24 / scaleFactor;
                }

                LayerUtility.SetCameraIndex(shortNameGo, SceneObject.LayerIndexInfo.CameraIndex);
                LayerUtility.SetLayerIndexInRender(shortNameGo, SceneObject.LayerIndexInfo.LayerIndex);
            });
        }

        public override void Dispose()
        {
            if (fullNameGo != null)
            {
                GameObject.Destroy(fullNameGo);
                fullNameGo = null;
            }
            if (shortNameGo != null)
            {
                GameObject.Destroy(shortNameGo);
                shortNameGo = null;
            }
            base.Dispose();
        }
    }
}