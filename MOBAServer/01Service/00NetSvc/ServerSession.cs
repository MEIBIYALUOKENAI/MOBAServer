/*-------------------------------------------------------------------------
* 命名空间名称/文件名:    KCPExampleServer/ServerSession 
* 功 能：       服务端会话
* 类 名：       ServerSession
* 创建时间：  2024/8/9 17:18:05
* 创建人:        Meibiyaluokenai
*-------------------------------------------------------------------------*/

using MOBAProtocol;
using MOBAServer;
using SULog;
using SUNet;


namespace KCPExampleServer
{
    public class ServerSession : KCPSession<MOBAMsg>
    {
        protected override void OnConnected()
        {
            SULogger.LogColor(LogColorEnum.Green, $"客户端[SID]:{_sid}已连接");
        }

        protected override void OnDisConnected()
        {
            SULogger.LogColor(LogColorEnum.Green, $"客户端[SID]:{_sid}断开连接");
        }

        protected override void OnReceiveMsg(MOBAMsg msg)
        {
            SULogger.Log($"C->S 客户端[SID]:{_sid} [CMD]->{msg.cmd}");
            NetSvc.Instance.AddMsgPack(this,msg);
        }
        protected override void OnUpdate(DateTime now)
        {
           
        }

        
    }
}


