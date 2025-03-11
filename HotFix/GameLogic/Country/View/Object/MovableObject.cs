using GameLogic.Country.View.AI;
using TEngine;
using UnityEngine;

namespace GameLogic.Country.View.Object
{
    /// <summary>
    /// 可移动的场景对象， 不可以直接创建，需要子类实现具体的资源加载逻辑
    /// </summary>
    public class MovableObject : SceneObject
    {
        protected HTNPlanner htnPlanner;
        protected HTNState htnState;
        
        // 添加位置设置追踪
        protected string lastPositionSetBy = "初始化";

        public virtual string ModelPath => $"MovableObject_{SceneObjectInfo.MapObjectEntity.Model}";

        public override void Initialize()
        {
            CreateViewContainers();
            LoadResources();
            InitializeHTN();
            InitializeHolder();
            UpdatePosition();
            ShowObjectView();
            base.Initialize();
        }
        

        protected virtual void CreateViewContainers()
        {
            ObjectView = new GameObject("ObjectView");
            ObjectView.transform.SetParent(transform, false);
        }

        protected virtual void LoadResources()
        {
            // 子类实现具体的资源加载逻辑
        }

        protected virtual void InitializeHTN()
        {
            htnState = new HTNState
            {
                Position = SceneObjectInfo.Position,
                IsMoving = false,
                IsActioning = false,
                IsComplete = false
            };

            htnPlanner = new HTNPlanner(htnState);
        }


        protected override void OnDynamicUpdate()
        {
            if (htnPlanner != null)
            {
                htnState.Position = SceneObjectInfo.Position;
                htnPlanner.Update();
                HolderRef.CompAnimation?.UpdateAnimationState(); // 如果存在动画， 则需要更新动画状态
                UpdatePosition();
            }
        }

        public void AddTask(HTNTask task)
        {
            htnPlanner.AddTask(task);
        }

        /// <summary>
        /// 检查任务队列是否为空
        /// </summary>
        /// <returns>如果当前没有任务且队列为空，则返回true</returns>
        public bool IsTaskQueueEmpty()
        {
            // 这是一个辅助方法，实际实现可能需要修改HTNPlanner以提供队列状态
            // 这里我们假设如果没有正在执行的任务且没有移动或动作，则队列为空
            return htnPlanner != null && 
                   !htnState.IsMoving && 
                   !htnState.IsActioning;
        }

        public virtual void MoveTo(Vector3 target, float speed=1.0f)
        {
            htnPlanner.AddTask(new MoveTask(this, target, speed));
        }

        public virtual void PerformAction(float duration)
        {
            htnPlanner.AddTask(new ActionTask(this, duration));
        }

        /// <summary>
        /// 设置对象位置，并同步更新相关状态
        /// </summary>
        /// <param name="position">新位置</param>
        /// <param name="caller">调用者标识，用于调试</param>
        protected virtual void SetPosition(Vector3 position, string caller = "未知")
        {
            if (position.x < transform.position.x)
            {
                Log.Warning($"设置错误 {caller}");
            }
            transform.position = position;
            SceneObjectInfo.Position = position;
            lastPositionSetBy = caller;
            
            // 可以在这里添加日志或断点来跟踪位置设置
            Debug.Log($"{gameObject.name} 位置被设置为 {position} 由 {caller}");
        }

        /// <summary>
        /// 获取最后设置位置的调用者
        /// </summary>
        public string GetLastPositionSetBy()
        {
            return lastPositionSetBy;
        }

        /// <summary>
        /// 更新移动位置
        /// </summary>
        public override void UpdatePosition()
        {
            if (htnState != null && htnState.IsMoving)
            {
                // 计算移动方向
                Vector3 direction = (htnState.TargetPosition - transform.position).normalized;
                
                // 使用HTN状态中的移动速度
                float speed = htnState.MoveSpeed;
                
                // 计算本帧移动距离
                float distance = speed * Time.deltaTime;
                
                // 计算到目标的距离
                float remainingDistance = Vector3.Distance(transform.position, htnState.TargetPosition);
                
                // 如果剩余距离小于本帧移动距离，直接到达目标
                if (remainingDistance <= distance)
                {
                    // 使用封装的位置设置方法
                    SetPosition(htnState.TargetPosition, "UpdatePosition-到达目标");
                }
                else
                {
                    // 使用封装的位置设置方法
                    Vector3 newPosition = transform.position + direction * distance;
                    SetPosition(newPosition, "UpdatePosition-移动中");
                }
            }
        }

        protected override void Dispose()
        {
            htnPlanner?.ClearTasks();
            base.Dispose();
        }
    }
}
