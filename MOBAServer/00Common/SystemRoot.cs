/*-------------------------------------------------------------------------
* 命名空间名称/文件名:    MOBAServer
* 功 能：       
* 类 名：       SystemRoot
* 创建时间：  2024/8/10 11:38:05
* 创建人:        Meibiyaluokenai
*-------------------------------------------------------------------------*/

namespace MOBAServer
{
    public abstract class SystemRoot<T>:Singleton<T> where T : new()
    {
        protected NetSvc netSvc =null;  
        protected TimerSvc timerSvc = null;  
        protected CacheSvc cacheSvc = null;

        public override void Init() {
            base.Init();
            netSvc = NetSvc.Instance;
            timerSvc = TimerSvc.Instance;
            cacheSvc = CacheSvc.Instance;
            
        }

        public override void Update() {
            base.Update();
        }
    }
}


