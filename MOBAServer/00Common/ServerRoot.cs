/*-------------------------------------------------------------------------
* 命名空间名称/文件名:    MOBAServer._00Common/ServerRoot 
* 功 能：       N/A
* 类 名：       ServerRoot
* 创建时间：  2024/8/10 11:35:39
* 创建人:        Meibiyaluokenai
*-------------------------------------------------------------------------*/
using SULog;
namespace MOBAServer
{
    public class ServerRoot:Singleton<ServerRoot>
    {
        public override void Init()
        {
            base.Init();
            //日志默认初始化，使用控制台打印消息
            SULogger.InitSettings();

            //服务
            NetSvc.Instance.Init();//网络服务
            TimerSvc.Instance.Init();
            CacheSvc.Instance.Init();
            //业务
            LoginSys.Instance.Init();
            MatchSys.Instance.Init();
            RoomSys.Instance.Init();

            SULogger.LogColor(LogColorEnum.Green, "ServerRoot Init Done");
        }
        /// <summary>
        /// 一秒钟刷新100次
        /// </summary>
        public override void Update()
        {
            base.Update();

            //服务
            NetSvc.Instance.Update();
            TimerSvc.Instance.Update();
            CacheSvc.Instance.Update();
            //业务
            LoginSys.Instance.Update();
            MatchSys.Instance.Update();
            RoomSys.Instance.Update();
        }
    }
}


