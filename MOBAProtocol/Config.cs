/*-------------------------------------------------------------------------
* 命名空间名称/文件名:    MOBAProtocol/Config 
* 功 能：       N/A
* 类 名：       Config
* 创建时间：  2024/8/10 14:07:58
* 创建人:        Meibiyaluokenai
*-------------------------------------------------------------------------*/


namespace MOBAProtocol
{
    public class ServerConfig
    {
        
        public const int ConfirmCountDown = 15;
        
        public const int SelectCountDown = 30;
        //本地服务器
        public const string LocalDeveInnerIP = "127.0.0.1";

        //阿里云服务器
        public const string RemoteGateIP = "8.130.113.182";
        public const string RemoteServerIP = "172.24.234.229";

        public const int UDPPort = 9999;
        public const int ServerLogicFrameIntervelMs = 66;
    }
    public class Configs
    {
        public const float ClientLogicFrameDeltaSec = 0.066f;//s
    }
}


