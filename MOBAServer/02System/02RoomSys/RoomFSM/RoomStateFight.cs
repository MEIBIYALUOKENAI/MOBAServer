/*-------------------------------------------------------------------------
* 命名空间名称/文件名:    MOBAServer
* 功 能：       房间战斗状态管理
* 类 名：       RoomStateFight
* 创建时间：  2024/8/11 13:57:34
* 创建人:        Meibiyaluokenai
*-------------------------------------------------------------------------*/

using MOBAProtocol;
using SULog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOBAServer
{
    public class RoomStateFight : RoomStateBase
    {
        uint frameID = 0;
        List<OpKey> opkeyLst = new List<OpKey>();
        int checkTaskID;

        private bool[] endArr;
        public RoomStateFight(PVPRoom room) : base(room)
        {
            int len = room.sessionArr.Length;
            endArr = new bool[len];
        }

        public override void Enter()
        {
            opkeyLst.Clear();
            //开始定时发送数据
            checkTaskID = TimerSvc.Instance.AddTask(ServerConfig.ServerLogicFrameIntervelMs, SyncLogicFrame, null, 0);
        }
        //每66毫秒发送接收到的所有操作帧   移动、技能
        void SyncLogicFrame(int tid)
        {
            ++frameID;
            MOBAMsg msg = new MOBAMsg
            {
                cmd = CMD.NtfOpKey,
                isEmpty = true,
                ntfOpKey = new NtfOpKey
                {
                    frameID = frameID,
                    keyList = new List<OpKey>()
                }
            };

            int count = opkeyLst.Count;
            if (count > 0)
            {
                msg.isEmpty = false;
                msg.ntfOpKey.keyList.AddRange(opkeyLst);//将收集到客户端的操作帧全部分发
            }
            opkeyLst.Clear();
            room.BroadcastMsg(msg);
        }

        public override void Exit()
        {
            checkTaskID = 0;
            opkeyLst.Clear();
            endArr = null;
        }

        public override void Update() { }

        public void UpdateOpKey(OpKey key)
        {
            opkeyLst.Add(key);
        }
        //一个客户端点击继续进入大厅会导致所有客户端进入大厅，这里应该做计数，等待所有客户端进入大厅再销毁房间
        public void UpdateEndState(int posIndex)
        {
            endArr[posIndex] = true;
            //
            if (TimerSvc.Instance.DeleteTask(checkTaskID))
            {
                SULogger.LogColor(LogColorEnum.Green, "Delete Sync Task Success.");
            }
            else
            {
                SULogger.LogWarn("Delete Sync Task Failed.");
            }
            room.ChangeRoomState(RoomStateEnum.End);
        }
    }
}


