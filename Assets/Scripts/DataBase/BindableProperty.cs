using System;
using UnityEngine.Events;
namespace PabloFramework
{
    /// <summary>
    /// Object基类的Equals方法存在两个明显的问题。一是缺乏类型安全性，二是对于值类型而言需要装箱
    /// IEquatable提供了略微更好的性能
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BindableProperty<T> where T : IEquatable<T>
    {
        public delegate void ValueChangedHandler(T oldValue, T newValue);
        public ValueChangedHandler OnValueChangedHandler;

        private T m_Value = default;

        public T Value
        {
            get
            {
                return m_Value;
            }
            set
            {
                if (!value.Equals(this.m_Value))
                {
                    this.m_Value = value;
                    OnValueChanged?.Invoke(value);
                }

                if (!object.Equals(this.m_Value, value))
                {
                    T old = this.m_Value;
                    this.m_Value = value;
                    ValueChanged(old, this.m_Value);
                }
            }
        }

        public Action<T> OnValueChanged;
        public UnityAction<T> OnUnityValueChanged;


        private void ValueChanged(T oldValue, T newValue)
        {
            if (this.OnValueChangedHandler != null)
            {
                this.OnValueChangedHandler(oldValue, newValue);
            }
        }

        public override string ToString()
        {
            return (this.Value != null ? this.Value.ToString() : "null");
        }
    }
}