/*-------------------------------------------------------------------------
* 命名空间名称/文件名:    MOBAServer._02System._02RoomSys.RoomFSM/RoomStateEnd 
* 功 能：       N/A
* 类 名：       RoomStateEnd
* 创建时间：  2024/8/11 13:56:50
* 创建人:        Meibiyaluokenai
*-------------------------------------------------------------------------*/

using MOBAProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOBAServer
{
    public class RoomStateEnd:RoomStateBase
    {
        
        public RoomStateEnd(PVPRoom room) : base(room)
        {
        }

        public override void Enter()
        {
            MOBAMsg msg = new MOBAMsg
            {
                cmd = CMD.RspBattleEnd,
                rspBattleEnd = new RspBattleEnd
                {
                    //TOADD
                }
            };

            room.BroadcastMsg(msg);
            Exit();
        }

        public override void Exit()
        {
            //销毁当前房间
            RoomSys.Instance.DestroyRoom(room.roomID);
        }

        public override void Update()
        {
        }
        
    }
}


