using System;
using UnityEngine;

namespace UltimateDH
{
    public delegate bool TryerDelegate();

    /// <summary>
    /// 
    /// </summary>
    public class Attempt
    {
        private TryerDelegate m_Tryer;
        private Action m_Listeners;


        /// <summary>
        /// 注册尝试执行此操作的方法。
        /// 注意：只允许1次尝试！
        /// </summary>
        public void AddTryer(TryerDelegate tryer)
        {
            m_Tryer += tryer;
        }

        /// <summary>
        /// 注册尝试执行此操作的方法。
        /// 注意：只允许1次尝试！
        /// </summary>
        public void RemoveTryer(TryerDelegate tryer)
        {
            m_Tryer -= tryer;
        }

        /// <summary>
        /// 
        /// </summary>
        public void AddListener(Action listener)
        {
            m_Listeners += listener;
        }

        /// <summary>
        /// 
        /// </summary>
        public void RemoveListener(Action listener)
        {
            m_Listeners -= listener;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Try()
        {
            bool wasSuccessful = CallApprovers();
            if (wasSuccessful)
            {
                if (m_Listeners != null)
                    m_Listeners();
                return true;
            }

            return false;
        }


        private bool CallApprovers()
        {
            if (m_Tryer != null)
            {
                var invocationList = m_Tryer.GetInvocationList();
                for (int i = 0; i < invocationList.Length; i++)
                {
                    if (!(bool)invocationList[i].DynamicInvoke())
                        return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class AttemptValue<T>
    {
        private TryerDelegate m_Tryer;
        private Action m_Listeners;


        /// <summary>
        /// 注册尝试执行此操作的方法。
        /// 注意：只允许1次尝试！
        /// </summary>
        public void AddTryer(TryerDelegate tryer)
        {
            m_Tryer += tryer;
        }

        /// <summary>
        /// 注册尝试执行此操作的方法。
        /// 注意：只允许1次尝试！
        /// </summary>
        public void RemoveTryer(TryerDelegate tryer)
        {
            m_Tryer -= tryer;
        }

        /// <summary>
        /// 
        /// </summary>
        public void AddListener(Action listener)
        {
            m_Listeners += listener;
        }

        /// <summary>
        /// 
        /// </summary>
        public void RemoveListener(Action listener)
        {
            m_Listeners -= listener;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Try()
        {
            bool wasSuccessful = CallApprovers();
            if (wasSuccessful)
            {
                if (m_Listeners != null)
                    m_Listeners();
                return true;
            }

            return false;
        }

        public T Count;
        /// <summary>
        /// 
        /// </summary>
        public T TryValue()
        {
            Try();
            return Count;
        }


        private bool CallApprovers()
        {
            if (m_Tryer != null)
            {
                var invocationList = m_Tryer.GetInvocationList();
                for (int i = 0; i < invocationList.Length; i++)
                {
                    if (!(bool)invocationList[i].DynamicInvoke())
                        return false;
                }
            }

            return true;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class AttemptFunc
    {
        private TryerDelegate m_Tryer;
        private Func<bool> m_Listeners;


        /// <summary>
        /// 注册尝试执行此操作的方法。
        /// 注意：只允许1次尝试！
        /// </summary>
        public void AddTryer(TryerDelegate tryer)
        {
            m_Tryer += tryer;
        }

        /// <summary>
        /// 注册尝试执行此操作的方法。
        /// 注意：只允许1次尝试！
        /// </summary>
        public void RemoveTryer(TryerDelegate tryer)
        {
            m_Tryer -= tryer;
        }

        /// <summary>
        /// 
        /// </summary>
        public void AddListener(Func<bool> listener)
        {
            m_Listeners += listener;
        }

        /// <summary>
        /// 
        /// </summary>
        public void RemoveListener(Func<bool> listener)
        {
            m_Listeners -= listener;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Try()
        {
            bool wasSuccessful = CallApprovers();
            if (wasSuccessful)
            {
                bool IsDone = false;
                if (m_Listeners != null)
                    IsDone = m_Listeners();
                return IsDone;
            }

            return false;
        }


        private bool CallApprovers()
        {
            var invocationList = m_Tryer.GetInvocationList();
            for (int i = 0; i < invocationList.Length; i++)
            {
                if (!(bool)invocationList[i].DynamicInvoke())
                    return false;
            }

            return true;
        }
    }





    /// <summary>
    /// 
    /// </summary>
    public class Attempt<T>
    {
        public delegate bool GenericTryerDelegate(T arg);

        public delegate bool GenericTryerDelegateBase();

        GenericTryerDelegate m_Tryer;
        GenericTryerDelegateBase m_Tryerbase;
        Action<T> m_Listeners;


        /// <summary>
        /// 注册尝试执行此操作的方法。
        /// 注意：只允许1次尝试！
        /// </summary>
        public void AddTryer(GenericTryerDelegate tryer)
        {
            m_Tryer += tryer;
        }

        /// <summary>
        /// 注册尝试执行此操作的方法。
        /// 注意：只允许1次尝试！
        /// </summary>
        public void AddTryerBase(GenericTryerDelegateBase tryer)
        {
            m_Tryerbase += tryer;
        }

        /// <summary>
        /// 注册尝试执行此操作的方法。
        /// 注意：只允许1次尝试！
        /// </summary>
        public void RemoveTryer(GenericTryerDelegate tryer)
        {
            m_Tryer -= tryer;
        }

        /// <summary>
        /// 
        /// </summary>
        public void AddListener(Action<T> listener)
        {
            m_Listeners += listener;
        }

        /// <summary>
        /// 
        /// </summary>
        public void RemoveListener(Action<T> listener)
        {
            m_Listeners -= listener;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Try(T arg)
        {
            bool succeeded = CallApprovers();
            if (succeeded)
            {
                if (m_Listeners != null)
                    m_Listeners(arg);
                return true;
            }

            return false;
        }


        private bool CallApprovers()
        {
            if (m_Tryer != null)
            {
                var invocationList = m_Tryer.GetInvocationList();

                for (int i = 0; i < invocationList.Length; i++)
                {
                    if (!(bool)invocationList[i].DynamicInvoke())
                        return false;
                }
            }
            if (m_Tryerbase != null)
            {
                var invocatioBasenList = m_Tryerbase.GetInvocationList();

                for (int i = 0; i < invocatioBasenList.Length; i++)
                {
                    if (!(bool)invocatioBasenList[i].DynamicInvoke())
                        return false;
                }
            }
            return true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Attempt<T, V>
    {
        public delegate bool GenericTryerDelegate(T arg1, V arg2);

        private GenericTryerDelegate m_Tryer;
        private Action<T, V> m_Listeners;


        /// <summary>
        /// 注册尝试执行此操作的方法。
        /// 注意：只允许1次尝试！
        /// </summary>
        public void SetTryer(GenericTryerDelegate tryer)
        {
            m_Tryer = tryer;
        }

        /// <summary>
        /// 
        /// </summary>
        public void AddListener(Action<T, V> listener)
        {
            m_Listeners += listener;
        }

        /// <summary>
        /// 
        /// </summary>
        public void RemoveListener(Action<T, V> listener)
        {
            m_Listeners -= listener;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Try(T arg1, V arg2)
        {
            bool succeeded = m_Tryer != null && m_Tryer(arg1, arg2);
            if (succeeded)
            {
                if (m_Listeners != null)
                    m_Listeners(arg1, arg2);
                return true;
            }

            return false;
        }
    }
}