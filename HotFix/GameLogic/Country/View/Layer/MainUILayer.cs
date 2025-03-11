using GameBase.Layer;
using UnityEngine;
using UnityEngine.UI;
using TMPro;  // 添加 TextMeshPro 引用
using TEngine;
using GameBase.Scene;
using GameLogic.MainTown;

namespace GameLogic.Country.View.Layer
{
    [LayerBinding(layerName: LayerName.MainUILayer, location: "Country_ui_main_ui_layer")]
    public class MainUILayer : WindowLayerBase
    {
        public override void Initialize()
        {
        }

        #region 脚本工具生成的代码
        private Button m_btnExit;
        protected override void ScriptGenerator()
        {
            m_btnExit = FindChildComponent<Button>("m_btnExit");
            m_btnExit.onClick.AddListener(OnClickExitBtn);
        }
        #endregion


        private void OnClickExitBtn()
        {
            // 切换到主城场景
            // SceneSwitchManager.Instance.EnterScene<MainTownScene>();
            CountryRepo.Instance.CreateMockCollectEvent();


            Log.Info("退出国家场景");
        }

    }
}
