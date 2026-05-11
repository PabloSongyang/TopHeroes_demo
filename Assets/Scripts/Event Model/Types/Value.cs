using System;
using UnityEngine;

namespace UltimateDH
{
    /// <summary>
    /// 在值更改时进行回调的值属性
    /// </summary>
    public class Value<T>
    {
        public delegate T Filter(T lastValue, T newValue);

        private Action<T> m_Set;
        private Filter m_Filter;
        private T m_CurrentValue;
        private T m_LastValue;


        public Value(T initialValue)
        {
            m_CurrentValue = initialValue;
            m_LastValue = m_CurrentValue;
        }

        /// <summary>
        /// 判断是传入值与当前值是否相等
        /// </summary>
        public bool Is(T value)
        {
            return m_CurrentValue != null && m_CurrentValue.Equals(value);
        }

        /// <summary>
        /// 
        /// </summary>
        public void AddChangeListener(Action<T> callback)
        {
            m_Set += callback;
        }

        /// <summary>
        /// 
        /// </summary>
        public void RemoveChangeListener(Action<T> callback)
        {
            m_Set -= callback;
        }


        /// <summary>
        /// 
        /// </summary>
        public void ClearChangeListener()
        {
            while (m_Set != null)
                m_Set -= this.m_Set;
        }

        /// <summary>
        ///在常规回调之前将调用一个过滤器，判断是否满足当前条件，一般都是限制条件（如玩家健康状况等）。
        /// </summary>
        public void SetFilter(Filter filter)
        {
            m_Filter = filter;
        }

        /// <summary>
        /// 获取当前值
        /// </summary>
        public T Get()
        {
            return m_CurrentValue;
        }

        /// <summary>
        /// 获取上次的值
        /// </summary>
        public T GetLastValue()
        {
            return m_LastValue;
        }

        /// <summary>
        /// 刷新值
        /// </summary>
        public void Set(T value)
        {
            m_LastValue = m_CurrentValue;
            m_CurrentValue = value;

            if (m_Filter != null)
                m_CurrentValue = m_Filter(m_LastValue, m_CurrentValue);

            if (m_Set != null && (m_LastValue == null || !m_LastValue.Equals(m_CurrentValue)))
                m_Set(m_CurrentValue);
        }

        /// <summary>
        /// 强制刷新
        /// </summary>
        public void SetAndForceUpdate(T value)
        {
            m_LastValue = m_CurrentValue;
            m_CurrentValue = value;

            if (m_Filter != null)
                m_CurrentValue = m_Filter(m_LastValue, m_CurrentValue);

            if (m_Set != null)
                m_Set(m_CurrentValue);
        }

        /// <summary>
        /// 改变值但不刷新状态
        /// </summary>
        public void SetAndDontUpdate(T value)
        {
            m_LastValue = m_CurrentValue;
            m_CurrentValue = value;

            if (m_Filter != null)
                m_CurrentValue = m_Filter(m_LastValue, m_CurrentValue);
        }
    }
}