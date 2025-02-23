/*-------------------------------------------------------------------------
* 命名空间名称/文件名:    MOBAServer._02System._01MatchSys/MatchSys 
* 功 能：       N/A
* 类 名：       MatchSys
* 创建时间：  2024/8/10 13:24:37
* 创建人:        Meibiyaluokenai
*-------------------------------------------------------------------------*/

using KCPExampleServer;
using MOBAProtocol;
using PEUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOBAServer
{
    public class MatchSys:SystemRoot<MatchSys>
    {
        private Queue<ServerSession> que1V1 = null;
        private Queue<ServerSession> que2V2 = null;
        private Queue<ServerSession> que5V5 = null;
        public override void Init()
        {
            base.Init();
            que1V1 = new Queue<ServerSession>();
            que2V2 = new Queue<ServerSession>();
            que5V5 = new Queue<ServerSession>();

            PELog.Log("MatchSys Init Done");

            TimerSvc.Instance.AddTask(5000, CheckStatus, null, 0);
        }
        void CheckStatus(int id)
        {
            this.ColorLog(PEUtils.LogColor.Yellow, $"匹配队列负载：1v1=>{que1V1.Count}人，2v2=>{que2V2.Count}人，5v5=>{que5V5.Count}人");
        }
        //实时判断匹配队列中  是否匹配成功
        public override void Update()
        {
            base.Update();

            //1.根据各模式玩家数来开房间

            while (que1V1.Count >= 2)
            {
                ServerSession[] sessionArr = new ServerSession[2];
                for (int i = 0; i < 2; i++)
                {
                    sessionArr[i] = que1V1.Dequeue();
                }
                RoomSys.Instance.AddPVPRoom(sessionArr, PVPEnum._1V1);
            }

            while (que2V2.Count >= 4)
            {
                ServerSession[] sessionArr = new ServerSession[4];
                for (int i = 0; i < 4; i++)
                {
                    sessionArr[i] = que2V2.Dequeue();
                }
                RoomSys.Instance.AddPVPRoom(sessionArr, PVPEnum._2V2);
            }

            while (que5V5.Count >= 10)
            {
                ServerSession[] sessionArr = new ServerSession[10];
                for (int i = 0; i < 10; i++)
                {
                    sessionArr[i] = que5V5.Dequeue();
                }
                RoomSys.Instance.AddPVPRoom(sessionArr, PVPEnum._5V5);
            }
        }
        //客户端请求匹配
        public void ReqMatch(MsgPack pack)
        {
            ReqMatch data = pack._msg.reqMatch;
            PVPEnum pvpEnum = data.pvpEnum;
            //1.判断是什么模式，进入不同的匹配队列
            switch (pvpEnum)
            {
                case PVPEnum._1V1:
                    que1V1.Enqueue(pack._session);
                    break;
                case PVPEnum._2V2:
                    que2V2.Enqueue(pack._session);
                    break;
                case PVPEnum._5V5:
                    que5V5.Enqueue(pack._session);
                    break;
                case PVPEnum.None:
                default:
                    this.Error("不存在该匹配类型" + pvpEnum.ToString());
                    break;
            }
            //2.回应匹配消息，让客户端开始匹配，并根据当前模式正在匹配的人数发送预测时间
            MOBAMsg msg = new MOBAMsg
            {
                cmd = CMD.RspMatch,
                rspMatch = new RspMatch
                {
                    predictTime = GetPredictTime(pvpEnum),
                }
            };
            pack._session.SendMsg(msg);
        }

        //客户端请求取消匹配
        public void ReqCancel(MsgPack pack)
        {
            
        }

        private int GetPredictTime(PVPEnum pvpEnum)
        {
            int waitCount;
            switch (pvpEnum)
            {
                case PVPEnum._1V1:
                    waitCount = 2 - que1V1.Count;
                    if (waitCount < 0)
                    {
                        waitCount = 0;
                    }
                    return waitCount * 10 + 5;
                case PVPEnum._2V2:
                    waitCount = 4 - que2V2.Count;
                    if (waitCount < 0)
                    {
                        waitCount = 0;
                    }
                    return waitCount * 10 + 5;
                case PVPEnum._5V5:
                    waitCount = 10 - que5V5.Count;
                    if (waitCount < 0)
                    {
                        waitCount = 0;
                    }
                    return waitCount * 10 + 5;
                default:
                    return 0;
            }
        }
    }
}


