/*-------------------------------------------------------------------------
* 命名空间名称/文件名:    MOBAServer._01Service._02CacheSvc/CacheSvc 
* 功 能：       资源缓存服务
* 类 名：       CacheSvc
* 创建时间：  2024/8/10 13:08:41
* 创建人:        Meibiyaluokenai
*-------------------------------------------------------------------------*/

using KCPExampleServer;
using MOBAProtocol;
using SULog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOBAServer
{
    public class CacheSvc:Singleton<CacheSvc>
    {
        //acct-session
        private Dictionary<string, ServerSession> onLineAcctDic;
        //seesion-userdata
        private Dictionary<ServerSession, UserData> onLineSessionDic;

        public override void Init()
        {
            base.Init();
            onLineAcctDic = new Dictionary<string, ServerSession>();
            onLineSessionDic = new Dictionary<ServerSession, UserData>();
            SULogger.Log("CacheSvc Init Done");
        }

        public override void Update()
        {
            base.Update();
        }

        public bool IsAcctOnLine(string acct)
        {
            return onLineAcctDic.ContainsKey(acct);
        }

        public void AcctOnline(string acct, ServerSession session, UserData playerData)
        {
            onLineAcctDic.Add(acct, session);
            onLineSessionDic.Add(session, playerData);
        }

        public UserData GetUserDataBySession(ServerSession session)
        {
            if (onLineSessionDic.TryGetValue(session, out UserData playerData))
            {
                return playerData;
            }
            else
            {
                return null;
            }
        }
    }
}


