/*-------------------------------------------------------------------------
* 命名空间名称/文件名:    KPCNet/KCPHandle 
* 功 能：       KCP处理回调类
* 类 名：       KCPHandle
* 创建时间：  2024/8/8 11:51:49
* 创建人:        Meibiyaluokenai
*-------------------------------------------------------------------------*/

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net.Sockets.Kcp;
using System.Text;

namespace SUNet
{
    public class KCPHandle : IKcpCallback
    {
        public Action<Memory<byte>> Out;

        /// <summary>
        /// 在kcp Send后触发
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="avalidLength"></param>
        public void Output(IMemoryOwner<byte> buffer, int avalidLength)
        {
            //using 确保离开代码块执行对象的dispose
            using (buffer)
            {
                Out(buffer.Memory.Slice(0, avalidLength));//发送经过 序列化、压缩、附加控制字段后的消息
            }
        }

        public Action<byte[]> Recv;
        public void Recive(byte[] buffer)
        {
            Recv(buffer);//触发回调
        }
    }
}


