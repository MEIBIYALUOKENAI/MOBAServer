/*-------------------------------------------------------------------------
* 命名空间名称/文件名:    MOBAServer._01Service._00NetSvc/NetSvc 
* 功 能：       网络服务
* 类 名：       NetSvc
* 创建时间：  2024/8/10 13:05:41
* 创建人:        Meibiyaluokenai
*-------------------------------------------------------------------------*/

using KCPExampleServer;
using MOBAProtocol;
using SULog;
using SUNet;

namespace MOBAServer
{
    public class MsgPack
    {
        public ServerSession _session; //哪个会话
        public MOBAMsg _msg;

        //消息包， 具体的消息 哪个session，发来的什么消息
        public MsgPack(ServerSession session, MOBAMsg msg)
        {
            _session = session;
            _msg = msg;
        }
    }
    public class NetSvc:Singleton<NetSvc>
    {
        public static readonly string pkgQueue_Lock = "pkgQueue_Lock";

        public KCPNet<ServerSession,MOBAMsg> _server = new KCPNet<ServerSession, MOBAMsg> ();
        private Queue<MsgPack> _msgPackQueue = new Queue<MsgPack> ();

        public override void Init() { 
            base.Init();
            _msgPackQueue.Clear();
#if DEBUG
            _server.StartAsServer(ServerConfig.LocalDeveInnerIP, ServerConfig.UDPPort);//开启服务端
#else
            _server.StartAsServer(ServerConfig.RemoteServerIP, ServerConfig.UDPPort);
#endif


            SULogger.LogColor(LogColorEnum.Green,"NetSvc Init Done");
        }

        public void AddMsgPack(ServerSession session,MOBAMsg msg)
        {
            lock(pkgQueue_Lock)//为什么要加锁，保证线程安全，不同线程都有对该队列的修改
            {
                _msgPackQueue.Enqueue(new MsgPack(session, msg));
            }
            
        }

        public override void Update() {
            base.Update();
            //服务器会一秒钟收到几十几百个操作帧 ，通常是移动角度持续发生变化，而update在服务端是0.01刷新一次
            //使用if会导致延迟增高
            while( _msgPackQueue.Count > 0)//优化点1：使用if处理队列，导致帧发送延迟太长
            {
                lock (pkgQueue_Lock)
                {
                    MsgPack msg = _msgPackQueue.Dequeue();
                    HandleMsg(msg);
                }
            }
        }
        /// <summary>
        /// 根据消息类型，分发到对应系统进行处理
        /// </summary>
        /// <param name="msg"></param>
        private void HandleMsg(MsgPack msg)
        {
            switch (msg._msg.cmd)
            {
                case CMD.ReqLogin:
                    LoginSys.Instance.ReqLogin(msg);
                    break;
                case CMD.ReqMatch:
                    MatchSys.Instance.ReqMatch(msg);
                    break;
                case CMD.SndConfirm:
                    RoomSys.Instance.SndConfirm(msg);
                    break;
                case CMD.SndSelect:
                    RoomSys.Instance.SndSelect(msg);
                    break;
                case CMD.SndLoadPrg:
                    RoomSys.Instance.SndLoadPrg(msg);
                    break;
                case CMD.ReqBattleStart:
                    RoomSys.Instance.ReqBattleStart(msg);
                    break;
                case CMD.SndOpKey:
                    RoomSys.Instance.SndOpKey(msg);
                    break;
                case CMD.SndChat:
                    RoomSys.Instance.SndChat(msg);
                    break;
                case CMD.ReqBattleEnd:
                    RoomSys.Instance.ReqBattleEnd(msg);
                    break;
                case CMD.ReqPing:
                    SyncPingCMD(msg);
                    break;
                case CMD.None:
                default:
                    break;
            }
        }
        /// <summary>
        /// 仅仅用来恢复心跳包消息，用于延迟检测
        /// </summary>
        /// <param name="pack"></param>
        private void SyncPingCMD(MsgPack pack)
        {
            ReqPing req = pack._msg.reqPing;
            MOBAMsg msg = new MOBAMsg
            {
                cmd = CMD.RspPing,
                rspPing = new RspPing
                {
                    pingID = req.pingID
                }
            };
            pack._session.SendMsg(msg);
        }

    }
}


