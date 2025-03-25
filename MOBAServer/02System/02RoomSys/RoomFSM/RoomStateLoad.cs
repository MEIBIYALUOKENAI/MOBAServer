/*-------------------------------------------------------------------------
* 命名空间名称/文件名:    MOBAServer._02System._02RoomSys.RoomFSM/RoomStateLoad 
* 功 能：       房间加载阶段
* 类 名：       RoomStateLoad
* 创建时间：  2024/8/11 13:58:07
* 创建人:        Meibiyaluokenai
*-------------------------------------------------------------------------*/

using MOBAProtocol;
using SULog;

namespace MOBAServer
{
    public class RoomStateLoad : RoomStateBase
    {
        //存储每一个客户端的加载进度 、是否加载完成
        private int[] percentArr;
        private bool[] loadArr;

        public RoomStateLoad(PVPRoom room) : base(room)
        {
        }

        public override void Enter()
        {
            int len = room.sessionArr.Length;
            percentArr = new int[len];
            loadArr = new bool[len];
            //发送通知加载场景资源消息
            MOBAMsg msg = new MOBAMsg
            {
                cmd = CMD.NtfLoadRes,
                ntfLoadRes = new NtfLoadRes
                {
                    mapID = 101,//默认地图
                    heroList = new List<BattleHeroData>(),
                }
            };
            for (int i = 0; i < room.SelectArr.Length; i++)
            {
                SelectData sd = room.SelectArr[i];
                BattleHeroData hero = new BattleHeroData
                {
                    heroID = sd.selectHeroID,
                    userName = GetUserName(i)
                };
                msg.ntfLoadRes.heroList.Add(hero);
            }
            //每个客户端需要知道自己选择了哪个英雄，因此不能发送数据需要一个客户端一个客户端发送
            for (int i = 0; i < len; i++)
            {
                msg.ntfLoadRes.posIndex = i;//告诉客户端你是哪个
                room.sessionArr[i].SendMsg(msg);
            }
        }

        public void UpdateLoadState(int posIndex, int percent)
        {
            percentArr[posIndex] = percent;
            MOBAMsg msg = new MOBAMsg
            {
                cmd = CMD.NtfLoadPrg,
                ntfLoadPrg = new NtfLoadPrg
                {
                    percentLst = new List<int>()
                }
            };
            //这里可以做优化，我既然知道哪个客户端加载了多少，那我只需要广播该客户端的加载进度就行了，而不是全部都发
            for (int i = 0; i < percentArr.Length; i++)
            {
                msg.ntfLoadPrg.percentLst.Add(percentArr[i]);
            }

            room.BroadcastMsg(msg);
        }
        //每当有客户端加载完毕 发送请求进入战斗都会进入该函数
        public void UpdateLoadDone(int posIndex)
        {
            loadArr[posIndex] = true;

            for (int i = 0; i < loadArr.Length; i++)
            {
                if (loadArr[i] == false)
                {
                    return;
                }
            }

            //全部加载完成,到达该代码段,通知所有客户端开始战斗
            MOBAMsg msg = new MOBAMsg
            {
                cmd = CMD.RspBattleStart
            };
            room.BroadcastMsg(msg);

            room.ChangeRoomState(RoomStateEnum.Fight);
            SULogger.LogColor(LogColorEnum.Green, $"RoomID:{room.roomID} 所有玩家加载完成，进入战斗。");
        }

        public override void Exit()
        {
            percentArr = null;
            loadArr = null;
        }

        public override void Update()
        {
        }
        //根据房间client下标访问对应session和用户名
        string GetUserName(int posIndex)
        {
            UserData ud = CacheSvc.Instance.GetUserDataBySession(room.sessionArr[posIndex]);
            if (ud != null)
            {
                return ud.Name;
            }
            return "";
        }
    }
}



