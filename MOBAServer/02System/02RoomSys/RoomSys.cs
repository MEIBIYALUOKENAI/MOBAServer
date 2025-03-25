/*-------------------------------------------------------------------------
* 命名空间名称/文件名:    MOBAServer._02System._02RoomSys/RoomSys 
* 功 能：       N/A
* 类 名：       RoomSys
* 创建时间：  2024/8/10 13:26:02
* 创建人:        Meibiyaluokenai
*-------------------------------------------------------------------------*/

using KCPExampleServer;
using MOBAProtocol;
using SULog;

namespace MOBAServer
{
    public class RoomSys : SystemRoot<RoomSys>
    {
        List<PVPRoom> pvpRoomLst = null;
        Dictionary<uint, PVPRoom> pvpRoomDic = null;
        public override void Init()
        {
            base.Init();
            pvpRoomLst = new List<PVPRoom>();
            pvpRoomDic = new Dictionary<uint, PVPRoom>();
            SULogger.Log("RoomSys Init Done.");

            TimerSvc.Instance.AddTask(5000, CheckStatus, null, 0);
        }
        void CheckStatus(int id)
        {
            SULogger.LogColor(LogColorEnum.Magenta, $"对战房间负载：{pvpRoomLst.Count}个");
        }

        //创建一个房间，根据模式
        public void AddPVPRoom(ServerSession[] sessionArr, PVPEnum pvp)
        {
            //1.获取房间唯一标识
            uint roomID = GetUniqueRoomID();
            //2.
            PVPRoom room = new PVPRoom(roomID, pvp, sessionArr);
            pvpRoomLst.Add(room);
            pvpRoomDic.Add(roomID, room);
        }

        public override void Update()
        {
            base.Update();
        }

        uint roomID = 0;
        public uint GetUniqueRoomID()
        {
            roomID += 1;
            return roomID;
        }

        public void SndConfirm(MsgPack pack)
        {
            SndConfirm req = pack._msg.sndConfirm;
            if (pvpRoomDic.TryGetValue(req.roomID, out PVPRoom room))
            {
                room.SndConfirm(pack._session);
            }
            else
            {
                SULogger.LogWarn($"该房间{req.roomID}不存在 或者已经销毁");
            }
        }

        public void SndSelect(MsgPack pack)
        {
            SndSelect req = pack._msg.sndSelect;
            if (pvpRoomDic.TryGetValue(req.roomID, out PVPRoom room))
            {
                room.SndSelect(pack._session, req.heroID);
            }
            else
            {
                SULogger.LogWarn($"选择英雄房间->{roomID}  已被摧毁");
            }
        }

        public void SndLoadPrg(MsgPack pack)
        {
            SndLoadPrg req = pack._msg.sndLoadPrg;
            if (pvpRoomDic.TryGetValue(req.roomID, out PVPRoom room))
            {
                room.SndLoadPrg(pack._session, req.percent);
            }
            else
            {
                SULogger.LogWarn($"加载进度房间->{roomID}  已被摧毁");
            }
        }

        public void ReqBattleStart(MsgPack pack)
        {
            ReqBattleStart req = pack._msg.reqBattleStart;
            if (pvpRoomDic.TryGetValue(req.roomID, out PVPRoom room))
            {
                room.ReqBattleStart(pack._session);
            }
            else
            {
                SULogger.LogWarn($"通知房间该客户端加载完成->{roomID}  已被摧毁");
            }
        }

        public void SndOpKey(MsgPack pack)
        {
            SndOpKey snd = pack._msg.sndOpKey;
            if (pvpRoomDic.TryGetValue(snd.roomID, out PVPRoom room))
            {
                room.SndOpKey(snd.opKey);
            }
            else
            {
                SULogger.LogWarn("PVPRoom ID:" + snd.roomID + " is not exist.");
            }
        }

        public void SndChat(MsgPack pack)
        {
            SndChat snd = pack._msg.sndChat;
            if (pvpRoomDic.TryGetValue(snd.roomID, out PVPRoom room))
            {
                room.SndChat(snd.chatMsg);
            }
            else
            {
                SULogger.LogWarn("PVPRoom ID:" + snd.roomID + " is not exist.");
            }
        }

        public void ReqBattleEnd(MsgPack pack)
        {
            ReqBattleEnd snd = pack._msg.reqBattleEnd;
            if (pvpRoomDic.TryGetValue(snd.roomID, out PVPRoom room))
            {
                room.ReqBattleEnd(pack._session);
            }
            else
            {
                SULogger.LogWarn("PVPRoom ID:" + snd.roomID + " is not exist.");
            }
        }

        //根据id销毁房间，通常是房间状态结束后调用
        public void DestroyRoom(uint roomID)
        {
            if (pvpRoomDic.TryGetValue(roomID, out PVPRoom room))
            {
                room.Clear();

                int index = -1;
                for (int i = 0; i < pvpRoomLst.Count; i++)
                {
                    if (pvpRoomLst[i].roomID == roomID)
                    {
                        index = i;
                        break;
                    }
                }
                if (index >= 0)
                {
                    pvpRoomLst.RemoveAt(index);
                }
                pvpRoomDic.Remove(roomID);
            }
            else
            {
                SULogger.LogError("PVPRoom is not exist ID:" + roomID);
            }
        }
    }
}


