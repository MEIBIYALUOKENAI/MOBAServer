/*-------------------------------------------------------------------------
* 命名空间名称/文件名:    MOBAServer._02System._02RoomSys.RoomFSM/RoomStateSelect 
* 功 能：       选择英雄阶段
* 类 名：       RoomStateSelect
* 创建时间：  2024/8/11 13:58:36
* 创建人:        Meibiyaluokenai
*-------------------------------------------------------------------------*/

using MOBAProtocol;
using SULog;

namespace MOBAServer
{
    public class RoomStateSelect:RoomStateBase
    {
        private SelectData[] selectArr = null;
        private int checkTaskID = -1;
        private bool isAllSelected = false;
        public RoomStateSelect(PVPRoom room) : base(room) { }

        public override void Enter()
        {
            int len = room.sessionArr.Length;
            selectArr = new SelectData[len];
            for (int i = 0; i < len; i++)
            {
                selectArr[i] = new SelectData
                {
                    selectHeroID = 0,
                    selectDone = false
                };
            }

            MOBAMsg msg = new MOBAMsg
            {
                cmd = CMD.NtfSelect,
            };

            room.BroadcastMsg(msg);
            checkTaskID = TimerSvc.Instance.AddTask(ServerConfig.SelectCountDown * 1000 , ReachTimeLimit);
        }
        //倒计时结束，还有玩家为确认英雄选择 处理
        void ReachTimeLimit(int tid)
        {
            if (isAllSelected)
            {
                return;
            }
            else
            {
                SULogger.LogWarn($"房间[RoomID]->{room.roomID} 玩家超时未确认选择，默认英雄。");
                for (int i = 0; i < selectArr.Length; i++)
                {
                    if (selectArr[i].selectDone == false)
                    {
                        selectArr[i].selectHeroID = GetDefaultHeroSelect(i);
                        selectArr[i].selectDone = true;
                    }
                }

                room.SelectArr = selectArr;//选择状态结束前将客户端选择的英雄数据存储起来
                room.ChangeRoomState(RoomStateEnum.Load);
            }
        }

        //这里没有广播给客户端做选择英雄更新显示，可以根据选择做拓展
        //每次有客户端确认英雄选择都进入
        public void UpdateHeroSelect(int posIndex, int heroID)
        {
            selectArr[posIndex].selectHeroID = heroID;
            selectArr[posIndex].selectDone = true;
            CheckSelectState();
            if (isAllSelected)
            {
                //进入load状态
                if (TimerSvc.Instance.DeleteTask(checkTaskID))
                {
                    SULogger.LogColor(LogColorEnum.Green, $"RoomID:{room.roomID}所有玩家选择英雄完成，进入游戏加载。");
                }
                else
                {
                    SULogger.LogWarn("删除选择超时任务失败.");
                }

                room.SelectArr = selectArr;
                room.ChangeRoomState(RoomStateEnum.Load);
            }
        }
        //默认选择可选英雄表的第一个
        int GetDefaultHeroSelect(int posIndex)
        {
            UserData ud = CacheSvc.Instance.GetUserDataBySession(room.sessionArr[posIndex]);
            if (ud != null)
            {
                return ud.heroSelectDatas[0].HeroID;
            }
            return 0;
        }


        void CheckSelectState()
        {
            for (int i = 0; i < selectArr.Length; i++)
            {
                if (selectArr[i].selectDone == false)
                {
                    return;
                }
            }
            isAllSelected = true;
        }
        //确认流程完毕、进入加载阶段
        public override void Exit()
        {
            selectArr = null;
            checkTaskID = 0;
            isAllSelected = false;
        }

        public override void Update()
        {
        }
    }
}


