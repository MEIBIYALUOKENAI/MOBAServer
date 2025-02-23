/*-------------------------------------------------------------------------
* 命名空间名称/文件名:    MOBAServer._02System._02RoomSys/PVPRoom 
* 功 能：       N/A
* 类 名：       PVPRoom
* 创建时间：  2024/8/11 13:21:53
* 创建人:        Meibiyaluokenai
*-------------------------------------------------------------------------*/

using KCPExampleServer;
using MOBAProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOBAServer
{
    public class PVPRoom
    {
        public uint roomID;
        public PVPEnum pvpEnum = PVPEnum.None;
        public ServerSession[] sessionArr;

        private Dictionary<RoomStateEnum, RoomStateBase> fsm = new Dictionary<RoomStateEnum, RoomStateBase>();
        private RoomStateEnum currentRoomStateEnum = RoomStateEnum.None;

        /// <summary>
        /// 用于加载界面数据的显示
        /// </summary>
        private SelectData[] selectArr = null;
        public SelectData[] SelectArr
        {
            set
            {
                selectArr = value;
            }
            get
            {
                return selectArr;
            }
        }

        //匹配成功，开房间进入确认阶段
        public PVPRoom(uint roomID, PVPEnum pvpEnum, ServerSession[] sessionArr)
        {
            this.roomID = roomID;
            this.pvpEnum = pvpEnum;
            this.sessionArr = sessionArr;

            fsm.Add(RoomStateEnum.Confirm, new RoomStateConfirm(this));
            fsm.Add(RoomStateEnum.Select, new RoomStateSelect(this));
            fsm.Add(RoomStateEnum.Load, new RoomStateLoad(this));
            fsm.Add(RoomStateEnum.Fight, new RoomStateFight(this));
            fsm.Add(RoomStateEnum.End, new RoomStateEnd(this));

            ChangeRoomState(RoomStateEnum.Confirm);
        }
        public void ChangeRoomState(RoomStateEnum targetState)
        {
            if (currentRoomStateEnum == targetState)
            {
                return;
            }

            if (fsm.ContainsKey(targetState))
            {
                if (currentRoomStateEnum != RoomStateEnum.None)
                {
                    fsm[currentRoomStateEnum].Exit();
                }
                fsm[targetState].Enter();
                currentRoomStateEnum = targetState;
            }
        }
        //广播数据给房间所有客户端，许多状态都需要广播当前房间的各种消息
        public void BroadcastMsg(MOBAMsg msg)
        {
            //先把要统一发送的数据 序列化解压出来 再发送  优化
            byte[] bytes = KCPTool.Compress(KCPTool.Serialize(msg));
            if (bytes != null)
            {
                for (int i = 0; i < sessionArr.Length; i++)
                {
                    sessionArr[i].SendMsg(bytes,true);
                }
            }
        }


        int GetPosIndex(ServerSession session)
        {
            int posIndex = 0;
            for (int i = 0; i < sessionArr.Length; i++)
            {
                if (sessionArr[i].Equals(session))
                {
                    posIndex = i;break;
                }
            }
            return posIndex;
        }

        public void SndConfirm(ServerSession session)
        {
            if (currentRoomStateEnum == RoomStateEnum.Confirm)
            {
                if (fsm[currentRoomStateEnum] is RoomStateConfirm state)
                {
                    state.UpdateConfirmState(GetPosIndex(session));
                }
            }
        }

        public void SndSelect(ServerSession session, int heroID)
        {
            if (currentRoomStateEnum == RoomStateEnum.Select)
            {
                if (fsm[currentRoomStateEnum] is RoomStateSelect state)
                {
                    state.UpdateHeroSelect(GetPosIndex(session), heroID);
                }
            }
        }

        public void SndLoadPrg(ServerSession session, int percent)
        {
            if (currentRoomStateEnum == RoomStateEnum.Load)
            {
                if (fsm[currentRoomStateEnum] is RoomStateLoad state)
                {
                    state.UpdateLoadState(GetPosIndex(session), percent);
                }
            }
        }

        public void ReqBattleStart(ServerSession session)
        {
            if (currentRoomStateEnum == RoomStateEnum.Load)
            {
                if (fsm[currentRoomStateEnum] is RoomStateLoad state)
                {
                    state.UpdateLoadDone(GetPosIndex(session));
                }
            }
        }

        public void SndOpKey(OpKey opKey)
        {
            if (currentRoomStateEnum == RoomStateEnum.Fight)
            {
                if (fsm[currentRoomStateEnum] is RoomStateFight state)
                {
                    state.UpdateOpKey(opKey);
                }
            }
        }

        public void SndChat(string chatMsg)
        {
            MOBAMsg msg = new MOBAMsg
            {
                cmd = CMD.NtfChat,
                ntfChat = new NtfChat
                {
                    chatMsg = chatMsg
                }
            };
            BroadcastMsg(msg);
        }
        //游戏结束
        public void ReqBattleEnd(ServerSession session)
        {
            if (currentRoomStateEnum == RoomStateEnum.Fight)
            {
                if (fsm[currentRoomStateEnum] is RoomStateFight state)
                {
                    state.UpdateEndState(GetPosIndex(session));
                }
            }
        }
        //清理房间数据
        public void Clear()
        {
            SelectArr = null;
            sessionArr = null;
            fsm = null;
        }
    }
}


