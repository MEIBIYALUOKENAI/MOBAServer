/*-------------------------------------------------------------------------
* 命名空间名称/文件名:    KPCNet/KCPSession 
* 功 能：       KCP会话
* 类 名：       KCPSession
* 创建时间：  2024/8/8 11:52:04
* 创建人:        Meibiyaluokenai
*-------------------------------------------------------------------------*/

using SULog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets.Kcp;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SUNet
{
    [Serializable]
    public abstract class KCPMsg { }
    //当前连接状态
    public enum SessionState
    {
        None,
        Connected,
        DisConnected,
    }
    /// <summary>
    /// 需要网络连接，数据收发，就必须有一个session ，客户端1v1  而服务端需要1vn
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class KCPSession<T> where T : KCPMsg,new()
    {
        protected uint _sid;
        Action<byte[], IPEndPoint> _udpSender;//kcp处理消息回调给udp向远端发送消息
        protected IPEndPoint _remotePoint;
        protected SessionState _sessionState;

        public Action<uint> OnSessionClose;//当kcp关闭时调用的回调


        public KCPHandle _kcpHandle;//用于处理kcp输入输出时的回调
        public Kcp _kcp;

        private CancellationTokenSource _CTS;
        private CancellationToken _CT;

        public void InitSession(uint sid, Action<byte[], IPEndPoint> udpSender,IPEndPoint remotePoint)
        {
            //用于取消线程的令牌
            _CTS= new CancellationTokenSource();
            _CT = _CTS.Token;


            _sid = sid;//记录sid
            _udpSender = udpSender;//kcp处理完的消息回调给udp发送消息
            _remotePoint = remotePoint;
            _sessionState = SessionState.Connected;//状态设置为已连接

            _kcpHandle = new KCPHandle();//用于处理kcp 输入输出消息的回调

            //kcp初始化
            _kcp = new Kcp(_sid, _kcpHandle);//kcp需要唯一sid，这样对消息处理就有唯一sid
            _kcp.NoDelay(1, 10, 2, 1);
            _kcp.WndSize(64, 64);
            _kcp.SetMtu(512);

            //绑定Out回调，
            _kcpHandle.Out = (Memory<byte> buffer) =>
            {
                byte[] bytes = buffer.ToArray();//执行Out时触发回调，转成字节数组使用udp发送消息
                _udpSender(bytes, _remotePoint);
            };

            //kcp接收消息回调，触发Recv回调会对消息进行解压缩，反序列化
            //得到具体的消息 Msg然后交给net做对应处理
            _kcpHandle.Recv = (byte[] buffer) =>
            {
                byte[] a = KCPTool.DeCompress(buffer);
                T k = KCPTool.Deserialize<T>(a);
                if(k!= null)
                {
                    //具体的消息处理
                    OnReceiveMsg(k);
                }
            };
            OnConnected();
            Task.Run(Update,_CT);

        }

        //实时更新kcp处理完数据的交互
        //这个线程用于实时更新kcp处理数据
        async void Update()
        {
            try
            {
                while (true)
                {
                    DateTime now = DateTime.UtcNow;
                    OnUpdate(now);
                    if (_CT.IsCancellationRequested)
                    {
                        SULogger.LogWarn("与KCP连接接受处理后的数据操作 断开连接");
                        break;
                    }
                    else
                    {
                        _kcp.Update(now);
                        int len;
                        //这里说明kcp处理完一条完整的数据了
                        while ((len = _kcp.PeekSize()) > 0)
                        {
                            byte[] buffer = new byte[len];
                            if (_kcp.Recv(buffer) >= 0)//将处理完的数据给buffer
                            {
                                //去除控制消息 对kcp处理后的数据
                                //完整有序的消息，这时候才能处理

                                //此时已经去除了kcp的控制消息，只需要对该数据依次进行解压缩和反序列化
                                _kcpHandle.Recive(buffer);
                            }
                        }
                        await Task.Delay(10);
                    }
                    
                }
            }catch (Exception ex)
            {
                SULogger.LogWarn($"会话更新异常{ex}");
            }
        }

        public void SendMsg(T msg)
        {
            if(IsConnected())
            {
                byte[] bytes= KCPTool.Serialize(msg);
                SendMsg(bytes);
            }
            else
            {
                SULogger.LogWarn("未建立连接，你在发什么呀");
            }
        }

        /// <summary>
        /// AsSpan提供了一种更高效的内存访问方式，允许 KCP 协议直接操作内存中的数据，而不需要进行额外的内存复制。
        /// </summary>
        /// <param name="bytes"></param>
        public void SendMsg(byte[] bytes)
        {
            if (IsConnected())
            {
                byte[] b=KCPTool.Compress(bytes);
                _kcp.Send(b.AsSpan());//kcp在底层会为 序列化后、解压后的数据 加上控制协议，保证传输可靠
            }
            else
            {
                SULogger.LogWarn("未建立连接，你在发什么呀");
            }
        }
        public void SendMsg(byte[] bytes,bool isKey)//序列化压缩后的数据发送
        {
            if (IsConnected())
            {
                //这是封装消息的最后一步，kcp对
                _kcp.Send(bytes.AsSpan());//kcp在底层会为 序列化后、解压后的数据 加上控制协议，保证传输可靠
            }
            else
            {
                SULogger.LogWarn("未建立连接，你在发什么呀");
            }
        }

        public void CloseSession()
        {
            _CTS.Cancel();
            OnDisConnected();

            OnSessionClose?.Invoke(_sid);
            OnSessionClose = null;

            _sessionState = SessionState.DisConnected;
            _remotePoint = null;
            _kcpHandle = null;
            _kcp = null;
            _kcpHandle=null;
            _sid = 0;
            _CTS = null;
        }
        public uint GetSessionID()
        {
            return _sid;
        }

        protected abstract void OnDisConnected();
        protected abstract void OnConnected();
        protected abstract void OnUpdate(DateTime now);
        protected abstract void OnReceiveMsg(T msg);//只需要对该函数进行重写，就能调用对应的session处理消息

        //接收数据
        public void ReceiveData(byte[] buffer)
        {
            _kcp.Input(buffer.AsSpan());//给kcp内部进行处理
        }
        public bool IsConnected() { return _sessionState == SessionState.Connected; }
    }
}


