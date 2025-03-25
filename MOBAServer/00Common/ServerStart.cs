namespace MOBAServer
{
    public class ServerStart
    {
        static void Main(string[] args)
        {
            ServerRoot.Instance.Init();//服务器开启

            while (true)//主线程用来驱动所有更新 
            {
                ServerRoot.Instance.Update();
                Thread.Sleep(10);// 10ms刷新一次
            }
        }
        
    }
}
