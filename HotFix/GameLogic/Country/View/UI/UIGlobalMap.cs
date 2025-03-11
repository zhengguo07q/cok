using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameLogic.Country.Model;
using TEngine;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic.Country.View.UI
{
    [Window(UILayer.UI, location: "Country_ui_globalmap")]
    internal class UIGlobalMap :UIWindow
    {
        #region 脚本工具生成的代码
        private GameObject m_goMoveableBg;
        private Button m_btnReturn;
        private Button m_btnClose;
        private GameObject m_itemLine;
        private GameObject m_itemKingdom;
        protected override void ScriptGenerator()
        {
            m_goMoveableBg = FindChild("m_goMoveableBg").gameObject;
            m_btnReturn = FindChildComponent<Button>("head_bg/m_btnReturn");
            m_btnClose = FindChildComponent<Button>("head_bg/m_btnClose");
            m_itemLine = FindChild("m_itemLine").gameObject;
            m_itemKingdom = FindChild("m_itemKingdom").gameObject;
            m_btnReturn.onClick.AddListener(OnClickReturnBtn);
            m_btnClose.onClick.AddListener(OnClickCloseBtn);
        }
        #endregion

        #region 事件
        private void OnClickReturnBtn()
        {
        }
        private void OnClickCloseBtn()
        {
        }
        #endregion


        public int gridSize = 0;     // 总共要生成的网格数量
        public float spacing = 30f;   // 网格之间的间距
        private Dictionary<Vector2, GameObject> placedObjects = new Dictionary<Vector2, GameObject>();

        /// <summary>
        /// 创建窗口
        /// </summary>
        protected override void OnCreate()
        {
            UpdateGrid();
        }

        /// <summary>
        /// 更新网格
        /// </summary>
        void UpdateGrid() {
            gridSize = KingdomManager.Instance.KingdomList.Count;
            GenerateSpiralGrid();
        }

        /// <summary>
        /// 产生网格
        /// </summary>
        void GenerateSpiralGrid()
        {
            int x = 0, y = 0; // 中心开始
            int num = 0;
            int step = 1;
            int direction = 0; // 0: 右, 1: 下, 2: 左, 3: 上

            while (num < gridSize)
            {
                for (int i = 0; i < step; i++)
                {
                    if (num >= gridSize) break;
                    PlaceObject(x, y, num);
                    num++;
                    Move(ref x, ref y, direction);
                }
                direction = (direction + 1) % 4;
                if (direction % 2 == 0) step++; // 每两次转向增加一步
                for (int i = 0; i < step; i++)
                {
                    if (num >= gridSize) break;
                    PlaceObject(x, y, num);
                    num++;
                    Move(ref x, ref y, direction);
                }
                direction = (direction + 1) % 4;
            }

            // 在所有对象放置完毕后，连接相邻的对象
            ConnectAdjacentObjects();
        }

        /// <summary>
        /// 占位对象
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="index"></param>
        async void PlaceObject(int x, int y, int index)
        {
            Vector2 position = new Vector2(x * spacing, y * spacing);
            position += new Vector2(spacing / 4, spacing / 4);
            GameObject gridObject = await GameModule.Resource.LoadGameObjectAsync("", m_itemKingdom.transform, gameObject.GetCancellationTokenOnDestroy());
            var rectTransform = gridObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = position;
            placedObjects[position] = gridObject;
        }


        /// <summary>
        /// 移动方向
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="direction"></param>
        void Move(ref int x, ref int y, int direction)
        {
            switch (direction)
            {
                case 0: x++; break; // 向右
                case 1: y--; break; // 向下
                case 2: x--; break; // 向左
                case 3: y++; break; // 向上
            }
        }

        /// <summary>
        /// 连接王国之间的线条
        /// </summary>
        void ConnectAdjacentObjects()
        {
            foreach (var pos in placedObjects.Keys)
            {
                // 尝试连接到右边的对象
                Vector2 rightPos = pos + new Vector2(spacing, 0);
                if (placedObjects.ContainsKey(rightPos))
                {
                    DrawLine(pos, rightPos);
                }

                // 如果需要，可以在这里添加其他方向的检查逻辑
            }
        }

        /// <summary>
        /// 绘制连线
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        async void DrawLine(Vector2 start, Vector2 end)
        {
            GameObject line = await GameModule.Resource.LoadGameObjectAsync("", m_itemLine.transform, gameObject.GetCancellationTokenOnDestroy());
            RectTransform lineRectTransform = line.GetComponent<RectTransform>();

            // 设置线段的位置和旋转
            Vector2 midPoint = (start + end) / 2;
            lineRectTransform.anchoredPosition = midPoint;

            // 计算角度并旋转线段
            float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;
            lineRectTransform.rotation = Quaternion.Euler(0, 0, angle);

            // 设置线段的长度
            float distance = Vector2.Distance(start, end);
            lineRectTransform.sizeDelta = new Vector2(distance, lineRectTransform.sizeDelta.y);
        }
    }
}
