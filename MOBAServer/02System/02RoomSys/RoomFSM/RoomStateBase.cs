/*-------------------------------------------------------------------------
* 命名空间名称/文件名:    MOBAServer._02System._02RoomSys.RoomFSM/RoomStateBase 
* 功 能：       N/A
* 类 名：       RoomStateBase
* 创建时间：  2024/8/11 13:50:24
* 创建人:        Meibiyaluokenai
*-------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOBAServer
{
    public interface IRoomState
    {
        void Enter();
        void Update();
        void Exit();
    }

    public abstract class RoomStateBase : IRoomState
    {
        public PVPRoom room;
        public RoomStateBase(PVPRoom room)
        {
            this.room = room;
        }

        public abstract void Enter();

        public abstract void Exit();

        public abstract void Update();
    }

    public enum RoomStateEnum
    {
        None = 0,
        Confirm,    //确认
        Select,     //选择
        Load,       //加载
        Fight,      //战斗
        End,        //完成
    }
}


