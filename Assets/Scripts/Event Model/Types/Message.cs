using System;

namespace UltimateDH
{
    /// <summary>
    /// 
    /// </summary>
    public class Message
    {
        private Action m_Listeners;


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
        public void Send()
        {
            if (m_Listeners != null)
                m_Listeners();
        }

        /// <summary>
		/// 
		/// </summary>
        public void ClearAllEevnt()
        {
            while (this.m_Listeners != null)
            {
                this.m_Listeners -= this.m_Listeners;
            }
        }
    }

   
    /// <summary>
    /// 
    /// </summary>
    public class Message<T>
    {
        private Action<T> m_Listeners;


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
        public void RemoveListener(Action<T> callback)
        {
            m_Listeners -= callback;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Send(T message)
        {
            if (m_Listeners != null)
                m_Listeners(message);
        }

        /// <summary>
		/// 
		/// </summary>
        public void ClearAllEevnt()
        {
            while (this.m_Listeners != null)
            {
                this.m_Listeners -= this.m_Listeners;
            }
        }

        public int Count()
        {          
            return this.m_Listeners.GetInvocationList().Length;
        }
    }
}
