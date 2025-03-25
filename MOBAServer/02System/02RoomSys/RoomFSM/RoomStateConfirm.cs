/*-------------------------------------------------------------------------
* 命名空间名称/文件名:    MOBAServer._02System._02RoomSys.RoomFSM/RoomStateConfirm 
* 功 能：       N/A
* 类 名：       RoomStateConfirm
* 创建时间：  2024/8/11 13:55:31
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
    //有限状态机模式，将房间的各个流程分为不同状态来解耦合、模块化编程
    public class RoomStateConfirm:RoomStateBase
    {
        private ConfirmData[] confirmArr = null;
        private int checkTaskID = -1;
        private bool isAllConfirmed = false;
        public RoomStateConfirm(PVPRoom room) : base(room)
        {
        }
        //进入等待确认初始化阶段
        public override void Enter()
        {
            int len = room.sessionArr.Length;
            confirmArr = new ConfirmData[len];
            for (int i = 0; i < len; i++)
            {
                confirmArr[i] = new ConfirmData
                {
                    iconIndex = i,
                    confirmDone = false
                };
            }
            
            MOBAMsg msg = new MOBAMsg
            {
                cmd = CMD.NtfConfirm,
                ntfConfirm = new NtfConfirm
                {
                    roomID = room.roomID,
                    dissmiss = false,
                    confirmArr = confirmArr
                }
            };
            //发送每个玩家的确认消息告诉客户端
            room.BroadcastMsg(msg);
            checkTaskID = TimerSvc.Instance.AddTask(30 * 1000, ReachTimeLimit);//创建
        }
        //计时结束还有玩家未准备处理 解散房间
        void ReachTimeLimit(int tid)
        {
            if (isAllConfirmed)
            {
                return;
            }
            else
            {
                SULogger.LogColor(LogColorEnum.Yellow, "RoomID:{0} 确认超时，解散房间，重新匹配。", room.roomID);
                MOBAMsg msg = new MOBAMsg
                {
                    cmd = CMD.NtfConfirm,
                    ntfConfirm = new NtfConfirm
                    {
                        dissmiss = true
                    }
                };
                //发送解散房间的消息给客户端
                room.BroadcastMsg(msg);
                room.ChangeRoomState(RoomStateEnum.End);
            }
        }
        //每次收到客户端确认准备的消息就判断所有客户端是否准备
        public void UpdateConfirmState(int posIndex)
        {
            confirmArr[posIndex].confirmDone = true;
            CheckConfirmState();

            if (isAllConfirmed)
            {
                if (TimerSvc.Instance.DeleteTask(checkTaskID))
                {
                    SULogger.LogColor(LogColorEnum.Green, "RoomID:{0} 所有玩家确认完成，进入英雄选择。", room.roomID);
                }
                else
                {
                    SULogger.LogWarn($"RoomID{room.roomID} 所有玩家确认完成，删除超时函数回调失败");
                }
                room.ChangeRoomState(RoomStateEnum.Select);
            }
            else
            {
                MOBAMsg msg = new MOBAMsg
                {
                    cmd = CMD.NtfConfirm,
                    ntfConfirm = new NtfConfirm
                    {
                        roomID = room.roomID,
                        dissmiss = false,
                        confirmArr = confirmArr
                    }
                };

                room.BroadcastMsg(msg);
            }
        }

        void CheckConfirmState()
        {
            for (int i = 0; i < confirmArr.Length; i++)
            {
                if (confirmArr[i].confirmDone == false)
                {
                    return;
                }
            }
            isAllConfirmed = true;
        }
        public override void Exit()
        {
            confirmArr = null;
            checkTaskID = -1;
            isAllConfirmed = false;
        }

        public override void Update()
        {
        }
    }
}


