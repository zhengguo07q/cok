using GameLogic.Country.View.Formation;
using System;
using System.Collections.Generic;

namespace GameLogic.Country.View.Component
{
    /// <summary>
    /// 组件基类
    /// </summary>
    public class ComponentBase
    {
        public virtual void Initialize() { }
        public virtual void Dispose() { }
    }

    /// <summary>
    /// 组件容器，用于管理和存储所有组件
    /// </summary>
    public class ComponentHolder
    {
        private readonly Dictionary<Type, ComponentBase> baseComponents = new();

        public CompNameDisplay CompNameDisplay { get; set; }

        public CompCombat CompCombat { get; set; }

        public CompCollider CompCollider { get; set; }

        public CompAnimation CompAnimation { get; set; }

        /// <summary>
        /// 引用组件到属性
        /// </summary>
        private void RefComponet(ComponentBase component) 
        {
            if (component is CompNameDisplay)
            {
                CompNameDisplay = (CompNameDisplay)component;
            }
            else if (component is CompCombat) 
            {
                CompCombat = (CompCombat)component;
            }        
            else if (component is CompCollider) 
            {
                CompCollider = (CompCollider)component;
            }
            else if (component is CompAnimation)
            {
                CompAnimation = (CompAnimation)component;
            }
        }

        // 添加组件（支持链式调用）
        public T Add<T>() where T : ComponentBase, new()
        {
            var component = new T();
            baseComponents[typeof(T)] = component;
            RefComponet(component);
            return component;
        }

        // 获取组件（泛型版本）
        public T Get<T>() where T : ComponentBase
        {
            return baseComponents.TryGetValue(typeof(T), out var component) ? (T)component : null;
        }

        // 获取组件（Type 版本）
        public ComponentBase Get(Type componentType)
        {
            return baseComponents.TryGetValue(componentType, out var component) ? component : null;
        }

        // 初始化所有组件
        public void InitializeAll()
        {
            foreach (var component in baseComponents.Values)
            {
                component.Initialize();
            }
        }

        // 释放所有组件
        public void DisposeAll()
        {
            foreach (var component in baseComponents.Values)
            {
                component.Dispose();
            }
            baseComponents.Clear();
        }
    }
}