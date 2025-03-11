using UnityEngine;
using System.Collections.Generic;

namespace GameLogic.Country.View.AI
{
    /// <summary>
    /// HTN基础状态
    /// </summary>
    public class HTNState
    {
        public Vector3 Position { get; set; }           // 当前位置
        public Vector3 TargetPosition { get; set; }     // 目标位置
        public bool IsMoving { get; set; }              // 是否正在移动
        public bool IsActioning { get; set; }           // 是否正在执行动作
        public bool IsComplete { get; set; }            // 是否完成
        public float MoveSpeed { get; set; } = 1.0f;    // 移动速度
    }

    /// <summary>
    /// HTN任务基类
    /// </summary>
    public abstract class HTNTask
    {
        /// <summary>
        /// 是否能执行
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public abstract bool CanExecute(HTNState state);

        /// <summary>
        /// 执行这个任务
        /// </summary>
        /// <param name="state"></param>
        public abstract void Execute(HTNState state);
       
        /// <summary>
        /// 是否完成，如果存在任务，每帧都会检测这个是否能完成
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public abstract bool IsComplete(HTNState state);

        /// <summary>
        /// 退出这个任务
        /// </summary>
        /// <param name="state"></param>
        public virtual void OnExit(HTNState state) { }
    }

    /// <summary>
    /// HTN规划器基类, 因为事件存在规划，调整状态机到规划器
    /// 通过任务切换State，状态切换后Object系统里会根据状态进行逻辑处理
    /// </summary>
    public class HTNPlanner
    {
        protected Queue<HTNTask> taskQueue = new Queue<HTNTask>();
        protected HTNTask currentTask;
        protected HTNState state;

        public HTNPlanner(HTNState state)
        {
            this.state = state;
        }

        public virtual void Update()
        {
            if (currentTask == null || currentTask.IsComplete(state))
            {
                if (currentTask != null)
                {
                    currentTask.OnExit(state);
                }

                if (taskQueue.Count > 0)
                {
                    currentTask = taskQueue.Dequeue();
                    if (currentTask.CanExecute(state))
                    {
                        currentTask.Execute(state);
                    }
                }
            }
        }

        /// <summary>
        /// 添加一个任务
        /// </summary>
        /// <param name="task"></param>
        public void AddTask(HTNTask task)
        {
            taskQueue.Enqueue(task);
        }

        /// <summary>
        /// 清除所有任务
        /// </summary>
        public void ClearTasks()
        {
            taskQueue.Clear();
            if (currentTask != null)
            {
                currentTask.OnExit(state);
                currentTask = null;
            }
        }
    }
}