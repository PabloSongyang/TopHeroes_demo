using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateDH
{
	/// <summary>
	/// 
	/// </summary>
	public class Activity
	{
		public bool Active { get; private set; }

		private TryerDelegate m_StartTryers;
		private TryerDelegate m_StopTryers;
		private Action m_OnStart;
		private Action m_OnStop;


        /// <summary>
        ///这将注册一个批准或不批准启动此活动的方法。
        /// </summary>
        public void AddStartTryer(TryerDelegate tryer)
		{
			m_StartTryers += tryer;
		}

        /// <summary>
        /// 这将注册一个批准或不批准停止此活动的方法。
        /// </summary>
        public void AddStopTryer(TryerDelegate tryer)
		{
			m_StopTryers += tryer;
		}

        /// <summary>
        /// 事件开始时调用。
        /// </summary>
        public void AddStartListener(Action listener)
		{
			m_OnStart += listener;
		}

        /// <summary>
        /// 事件停止时调用。
        /// </summary>
        public void AddStopListener(Action listener)
		{
			m_OnStop += listener;
		}

		/// <summary>
		/// 强制开始
		/// </summary>
		public void ForceStart()
		{
			if(Active)
				return;

			Active = true;
			if(m_OnStart != null)
				m_OnStart();
		}

		/// <summary>
		/// 尝试开始
		/// </summary>
		public bool TryStart()
		{
			if(Active)
				return false;

			if(m_StartTryers != null)
			{
				bool activityStarted = CallStartApprovers();

				if(activityStarted)
					Active = true;
				
				if(activityStarted && m_OnStart != null)
					m_OnStart();
				
				return activityStarted;
			}
			else
				Debug.LogWarning("没有添加任何启动条件,所以活动不会启动");

			return false;
		}
			
		/// <summary>
		/// 尝试停止
		/// </summary>
		public bool TryStop()
		{
			if(!Active)
				return false;

			if(m_StopTryers != null)
			{
				if(CallStopApprovers())
				{
					Active = false;

					if(m_OnStop != null)
						m_OnStop();
					
					return true;
				}
			}

			return false;
		}

        /// <summary>
        /// 强制停止。
        /// </summary>
        public void ForceStop()
		{
			if(!Active)
				return;
			
			Active = false;

			if(m_OnStop != null)
				m_OnStop();
		}

		private bool CallStartApprovers()
		{
			var invocationList = m_StartTryers.GetInvocationList();
			for(int i = 0;i < invocationList.Length;i ++)
			{
				if(!(bool)invocationList[i].DynamicInvoke())
					return false;
			}

			return true;
		}

		private bool CallStopApprovers()
		{
			var invocationList = m_StopTryers.GetInvocationList();
			for(int i = 0;i < invocationList.Length;i ++)
			{
				if(!(bool)invocationList[i].DynamicInvoke())
					return false;
			}

			return true;
		}
	}
}