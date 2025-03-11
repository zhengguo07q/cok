
using Cysharp.Threading.Tasks;
using GameBase;
using GameBase.Layer;
using GameBase.Scene;
using GameBase.Utility;
using GameLogic.Bag;
using GameLogic.Country;
using GameLogic.Country.View;
using GameLogic.Login;
using GameLogic.Task;
using GameLogic.Town;
using GameProto;

/// <summary>
/// 游戏启动类
/// </summary>
[GameObjectBinding(path: "[GameModule]/Root/GameStartup")]
public class GameStartup :BehaviourSingletonGameObject<GameStartup>
{
    /// <summary>
    /// 开始游戏业务层逻辑。
    /// <remarks>显示UI、加载场景等。</remarks>
    /// </summary>
    public void Startup()
    {
        CameraUtility.SetUICameraAsMainCamera();
        WindowLayerManager.Instance.Initialize();
        SceneSwitchManager.Instance.EnterScene<CountryScene>();
        //SceneSwitchManager.Instance.EnterScene<LoginScene>();
    }

    /// <summary>
    /// 初始化游戏数据模块
    /// </summary>
    public void InitlizeGameDataModules(){
        TaskManager.Instance.Initialize();
    }

    /// <summary>
    /// 注册同步协议
    /// </summary>
    public async UniTask RegisterSyncProtobuf() 
    {
        CountryRepo.Instance.SyncMapEvents();
        await BuildingRepo.Instance.ListBuildings();
        await TaskRepo.Instance.SyncTask();
    }
}
