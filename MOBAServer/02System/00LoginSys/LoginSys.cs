/*-------------------------------------------------------------------------
* 命名空间名称/文件名:    MOBAServer._02System._00LoginSys/LoginSys 
* 功 能：       N/A
* 类 名：       LoginSys
* 创建时间：  2024/8/10 13:23:40
* 创建人:        Meibiyaluokenai
*-------------------------------------------------------------------------*/

using MOBAProtocol;
using PEUtils;

namespace MOBAServer
{
    public class LoginSys:SystemRoot<LoginSys>
    {
        public override void Init()
        {
            base.Init();

            PELog.Log("LoginSys Init Done");
        }

        public override void Update()
        {
            base.Update();
        }

            //处理请求登录消息
            public void ReqLogin(MsgPack pack)
            {
                ReqLogin data = pack._msg.reqLogin;//获取消息体
                
                //准备回应消息
                MOBAMsg msg = new MOBAMsg
                {
                    cmd = CMD.RspLogin
                };
                //1.判断是否已经登录
                if (cacheSvc.IsAcctOnLine(data.acct))
                {
                    //已上线，返回错误信息
                    msg.errorCode = ErrorCode.AcctIsOnline;
                }
                else
                {
                    //2.获得唯一标识，用于标识客户端
                    uint sid = pack._session.GetSessionID();
                    //模拟发送账号游戏相关数据
                    UserData ud = new UserData
                    {
                        Id = sid,
                        Name = "玩家_" + sid,
                        Lv = 30,
                        Exp = 18888,
                        Coin = 999,
                        Diamond = 666,
                        Ticket = 0,
                        heroSelectDatas = new List<HeroSelectData> {
                        new HeroSelectData {
                            HeroID = 101,
                        },
                        new HeroSelectData {
                            HeroID =102
                        }
                    }
                    };

                    msg.rsqLogin = new RspLogin
                    {
                        userData = ud
                    };
                //3.使用cache缓存已经上线的账号
                    cacheSvc.AcctOnline(data.acct, pack._session, ud);
                }
                //4.回应登录消息
                pack._session.SendMsg(msg);
            }
        
    }
}


