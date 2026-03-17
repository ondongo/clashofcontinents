namespace ClashOfContinents
{
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;

    public class UI_SelectedBuilding : MonoBehaviour
    {

        [SerializeField] public GameObject _elements = null;
        [SerializeField] public RectTransform _buildingName = null;
        [SerializeField] public TextMeshProUGUI _buildingNameText = null;

        [SerializeField] private float buildingNameHeight = 0.06f; 
        [SerializeField] private float buildingNameAspect = 6f;
        private Vector2 buildingNameSize = Vector2.one;

        private static UI_SelectedBuilding _instance = null; public static UI_SelectedBuilding instance { get { return _instance; } }

        private SelectableBuilding trackedGridBuilding = null;

        private void Awake()
        {
            _instance = this;
            _elements.SetActive(false);
        }

        private void Start()
        {
            _buildingName.anchorMin = Vector3.zero;
            _buildingName.anchorMax = Vector3.zero;
            buildingNameSize = new Vector2(Screen.height * buildingNameHeight * buildingNameAspect, Screen.height * buildingNameHeight);

            if (CameraController.instanse != null)
                _buildingName.sizeDelta = buildingNameSize * CameraController.instanse.zoomScale;
            else
                _buildingName.sizeDelta = buildingNameSize;
        }

        public void OpenForGridBuilding(SelectableBuilding sb)
        {
            trackedGridBuilding = sb;
            if (_buildingNameText != null)
                _buildingNameText.text = sb.buildingID.ToString();
            _elements.SetActive(true);
        }

        public void SetStatus(bool active)
        {
            if (!active) trackedGridBuilding = null;
            _elements.SetActive(active);
        }

        private void Update()
        {
            if (Building.selectedInstanse != null && CameraController.instanse != null)
            {
                _buildingName.sizeDelta = buildingNameSize / CameraController.instanse.zoomScale;

                Vector3 end = UI_Main.instanse._grid.GetEndPosition(Building.selectedInstanse);

                Vector3 planDownLeft = CameraController.instanse.planDownLeft;
                Vector3 planTopRight = CameraController.instanse.planTopRight;

                float w = planTopRight.x - planDownLeft.x;
                float h = planTopRight.y - planDownLeft.y;

                if (Mathf.Abs(w) > 0.001f && Mathf.Abs(h) > 0.001f)
                {
                    float endW = end.x - planDownLeft.x;
                    float endH = end.y - planDownLeft.y;
                    Vector2 screenPoint = new Vector2(endW / w * Screen.width, endH / h * Screen.height);
                    Vector2 pos = screenPoint;
                    pos.y += (_buildingName.rect.height / 2f);
                    _buildingName.anchoredPosition = pos;
                }
                return;
            }

            if (trackedGridBuilding != null && Camera.main != null)
            {
                _buildingName.sizeDelta = buildingNameSize;

                Vector3 worldTop = trackedGridBuilding.transform.position + Vector3.up * 2f;
                Vector3 screen   = Camera.main.WorldToScreenPoint(worldTop);

                if (screen.z < 0)
                {
                    _elements.SetActive(false);
                    return;
                }

                _elements.SetActive(true);
                Vector2 pos = new Vector2(screen.x, screen.y);
                pos.y += _buildingName.rect.height / 2f;
                _buildingName.anchoredPosition = pos;
            }
        }

    }
}