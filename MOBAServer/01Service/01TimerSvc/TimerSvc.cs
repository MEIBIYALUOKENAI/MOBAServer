/*-------------------------------------------------------------------------
* 命名空间名称/文件名:    MOBAServer._01Service._01TimerSvc/TimerSvc 
* 功 能：       计时管理服务
* 类 名：       TimerSvc
* 创建时间：  2024/8/10 13:07:53
* 创建人:        Meibiyaluokenai
*-------------------------------------------------------------------------*/

using PETimer;
using PEUtils;

namespace MOBAServer
{
    /// <summary>
    /// 服务器这边只使用到了毫秒级定时器TickTimer
    /// </summary>
    public class TimerSvc:Singleton<TimerSvc>
    {
        TickTimer timer = new TickTimer(0, false);

        public override void Init()
        {
            base.Init();
            PELog.Log("TimerSvc Init Done");
        }

        public override void Update()
        {
            base.Update();

            timer.UpdateTask();
        }

        public int AddTask(uint delay, Action<int> taskCB, Action<int> cancelCB = null, int count = 1)
        {
            return timer.AddTask(delay, taskCB, cancelCB, count);
        }

        public bool DeleteTask(int tid)
        {
            return timer.DeleteTask(tid);
        }
    }
}


