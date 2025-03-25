/*-------------------------------------------------------------------------
* 命名空间名称/文件名:    KPCNet/KCPNet 
* 功 能：       网络服务管理
* 类 名：       KCPNet
* 创建时间：  2024/8/8 14:06:30
* 创建人:        Meibiyaluokenai
*-------------------------------------------------------------------------*/


using SULog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
namespace SUNet
{
    public class KCPNet<T,K> where T: KCPSession<K>,new ()
        where K : KCPMsg,new ()
    {
        UdpClient _udp;
        IPEndPoint _remotePoint;

        //用于取消异步的循环
        private CancellationTokenSource _CTS;
        private CancellationToken _CT;

        public KCPNet()
        {
            _CTS = new CancellationTokenSource();
            _CT = _CTS.Token;   
        }
        #region Server
        public Dictionary<uint, T> _sessionDic = null;//存储多个客户端   sid
        public void StartAsServer(string ip, int port)
        {

            _udp = new UdpClient(new IPEndPoint(IPAddress.Parse(ip), port));//服务端UDP ip和端口固定
            _remotePoint = new IPEndPoint(IPAddress.Parse(ip), port);
            _sessionDic = new Dictionary<uint, T>();

            //用于处理，突然关闭客户端 服务端无限发消息产生的bug
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _udp.Client.IOControl((IOControlCode)(-1744830452), new byte[] { 0, 0, 0, 0 }, null);
            }
            
            SULogger.LogColor(LogColorEnum.Green, "Server Start.....");
            Task.Run(ServerRecive, _CT);//异步操作 指定的工作排成队列，在Tread Pool上运行

        }
        async void ServerRecive()
        {
            UdpReceiveResult result;

            while (true)
            {
                try
                {
                    if (_CT.IsCancellationRequested)
                    {
                        SULogger.LogWarn("服务端端接受消息循环被取消");
                        break;
                    }
                    result = await _udp.ReceiveAsync();//等待异步接受完成
                    uint sid = BitConverter.ToUInt32(result.Buffer, 0);
                    //说明该客户端是第一次建立连接
                    if (sid == 0)
                    {
                        //服务端发送分配sid给客户端的消息，前面4个字节也是零，告诉客户端这是我给你分配的唯一sid
                        sid = CreateOnlyOneSid();
                        byte[] sid_bytes = BitConverter.GetBytes(sid);
                        byte[] conv_bytes = new byte[8];
                        Array.Copy(sid_bytes, 0, conv_bytes, 4, 4);
                        SendUDPMsg(conv_bytes, result.RemoteEndPoint);  
                    }
                    else
                    {
                        //这里说明，是分配给客户端sid后该客户端第一次发送消息
                        if(!_sessionDic.TryGetValue(sid,out T session))
                        {
                            //服务端正式分配一个session与客户端对应
                            session = new T();
                            session.InitSession(sid, SendUDPMsg, result.RemoteEndPoint);
                            session.OnSessionClose = OnServerSessionCloseCB;

                            //这里要加锁的原因是在另一个线程也会对该容器进行修改
                            //OnServerSessionCloseCB这个线程
                            lock (_sessionDic)
                            {
                                _sessionDic.Add(sid, session);
                            }
                            
                        }
                        else
                        {
                            session = _sessionDic[sid];//字典已经存在与该客户端对应的session
                        }
                        session.ReceiveData(result.Buffer);
                    }
                    
                }
                catch (Exception e)
                {
                    SULogger.LogWarn($"服务端异步接受消息处理异常+{e.ToString()}");
                }
            }
        }
        private void OnServerSessionCloseCB(uint sid)
        {
            lock(_sessionDic)
            {
                if(_sessionDic.TryGetValue(sid,out T session))
                {
                    _sessionDic.Remove(sid);
                    SULogger.LogWarn($"客户端{sid}断开连接");
                }
                else
                {
                    SULogger.LogError("Session 不在字典里 但是存在？？？");
                }
            }
        }
        private void CloseServer()
        {
            foreach(var item in _sessionDic)
            {
                item.Value.CloseSession();
            }
            _sessionDic = null;

            if(_udp != null)
            {
                _udp.Close();
                _udp= null;
                _CTS.Cancel();
            }
        }
        private uint sid = 0;
        //获取唯一的sid
        private uint CreateOnlyOneSid()
        {
            while(true)
            {
                ++sid;
                if(sid == uint.MaxValue)
                {
                    sid = 1;
                }
                if (!_sessionDic.ContainsKey(sid))
                {
                    break;
                }
            }
            return sid;
        }
        public void BroadCastMsg(K msg)
        {
            byte[] b = KCPTool.Compress(KCPTool.Serialize(msg));// 序列化、压缩
            foreach (var item in _sessionDic)
            {
                item.Value.SendMsg(b,true);
            }

        }
        #endregion

        #region Client
        public T _clientSession;
        public void StartAsClient(string ip, int port)
        {
            
            _udp = new UdpClient(0);//随机分配一个可使用的进程 端口号
            _remotePoint = new IPEndPoint(IPAddress.Parse(ip), port);
            SULogger.LogColor(LogColorEnum.Green, "Client Start.....");
            Task.Run(ClientRecive, _CT);//异步操作 指定的工作排成队列，在Tread Pool上运行

        }
        public Task<bool> ConnectServer(int interval,int maxInterval)
        {
            SendUDPMsg(new byte[4], _remotePoint);//发送消息包请求唯一sid
            int checkTimers = 0;


            //异步做连接状态检测
            //该线程专门用来做延时检测
            //interval检测一次，总检测时长maxInterval
            Task<bool> task = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(interval);
                    checkTimers += interval;
                    //这里跳出代表连接成功
                    if(_clientSession != null && _clientSession.IsConnected())
                    {
                        return true;//结束状态检测
                    }
                    else
                    {
                        //这里跳出代表一次连接失败
                        if (checkTimers > maxInterval)
                        {
                            return false;
                        }
                    }
                }
            });
            return task;
        }
        public void CloseClient()
        {
            _clientSession.CloseSession();
        }
        async void ClientRecive()
        {
            UdpReceiveResult result;
            
            while(true)
            {
                try
                {
                    if (_CT.IsCancellationRequested)
                    {
                        SULogger.LogWarn("客户端接受消息循环被取消");
                        break;
                    }

                    result = await _udp.ReceiveAsync();//等待异步接受完成

                    //判断是否是目标服务器
                    if (Equals(_remotePoint, result.RemoteEndPoint))
                    {
                        uint sid = BitConverter.ToUInt32(result.Buffer, 0);
                        if(sid == 0)
                        {
                            //有点像收到密钥的消息
                            //收到sid (密钥)  
                            if (_clientSession != null && _clientSession.IsConnected())
                            {
                                SULogger.LogWarn("已经连接到服务端 sid已存在!!!");
                            }
                            else
                            {
                                //第一次连接
                                sid = BitConverter.ToUInt32(result.Buffer, 4);
                                SULogger.LogColor(LogColorEnum.Green, $"连接服务器，存储[sid]:{sid}做客户端唯一标识");

                                _clientSession = new T();//正式创建kcpSession
                                _clientSession.InitSession(sid,SendUDPMsg, _remotePoint);
                                _clientSession.OnSessionClose = OnClientSessionClose;
                            }
                        }
                        else
                        {
                            //TODO业务逻辑处理
                            if(_clientSession != null && _clientSession.IsConnected())
                            {
                                _clientSession.ReceiveData(result.Buffer);
                            }
                            else
                            {
                                SULogger.LogWarn(sid+"未连接但是收到消息了，无效！！！");
                            }
                        }
                    }
                    else
                    {
                        SULogger.LogWarn("不是目标服务器的消息");
                    }
                }
                catch(Exception e)
                {
                    SULogger.LogWarn($"客户端异步接受消息处理异常+{e.ToString()}");
                }
            }
        }
        private void OnClientSessionClose(uint sid)
        {
            _CTS.Cancel();
            if(_udp != null)
            {
                _udp.Close();
                _udp = null;
            }
            SULogger.LogWarn($"客户端sid{sid}断开连接");
        }
        private void SendUDPMsg(byte[] bytes, IPEndPoint remotePoint)
        {
            if(_udp != null)
            {
                _udp.SendAsync(bytes, bytes.Length, remotePoint);
            }
        }
        #endregion

    }
}


