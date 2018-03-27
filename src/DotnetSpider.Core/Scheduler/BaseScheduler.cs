﻿using DotnetSpider.Core.Redial;
using System;
using System.Collections.Generic;

namespace DotnetSpider.Core.Scheduler
{
	public abstract class BaseScheduler : Named, IScheduler, IDisposable
	{
		/// <summary>
		/// 爬虫对象
		/// </summary>
		protected ISpider Spider { get; set; }

		/// <summary>
		/// 采集成功的链接数加 1
		/// </summary>
		public abstract void IncreaseSuccessCount();

		/// <summary>
		/// 采集失败的次数加 1
		/// </summary>
		public abstract void IncreaseErrorCount();

		/// <summary>
		/// 批量导入
		/// </summary>
		/// <param name="requests">请求对象</param>
		public abstract void Import(IEnumerable<Request> requests);

		/// <summary>
		/// 是否会使用互联网
		/// </summary>
		protected abstract bool UseInternet { get; set; }

		/// <summary>
		/// 剩余链接数
		/// </summary>
		public abstract long LeftRequestsCount { get; }

		/// <summary>
		/// 总的链接数
		/// </summary>
		public virtual long TotalRequestsCount { get; }

		/// <summary>
		/// 采集成功的链接数
		/// </summary>
		public abstract long SuccessRequestsCount { get; }

		/// <summary>
		/// 采集失败的次数, 不是链接数, 如果一个链接采集多次都失败会记录多次
		/// </summary>
		public abstract long ErrorRequestsCount { get; }

		/// <summary>
		/// 是否深度优先
		/// </summary>
		public bool DepthFirst { get; set; } = true;

		/// <summary>
		/// 添加请求对象到队列
		/// </summary>
		/// <param name="request">请求对象</param>
		public void Push(Request request)
		{
			if (UseInternet)
			{
				NetworkCenter.Current.Execute("sch-push", () =>
				{
					DoPush(request);
				});
			}
			else
			{
				DoPush(request);
			}
		}

		/// <summary>
		/// 初始化队列
		/// </summary>
		/// <param name="spider">爬虫对象</param>
		public virtual void Init(ISpider spider)
		{
			if (Spider == null)
			{
				Spider = spider;
			}
			else
			{
				throw new SpiderException("Scheduler already init");
			}
		}

		/// <summary>
		/// 取得一个需要处理的请求对象
		/// </summary>
		/// <returns>请求对象</returns>
		public abstract Request Poll();

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public abstract void Dispose();

		/// <summary>
		/// 导出整个队列
		/// </summary>
		public virtual void Export()
		{
		}

		protected virtual bool ShouldReserved(Request request)
		{
			return request.CycleTriedTimes > 0 && request.CycleTriedTimes <= Spider.Site.CycleRetryTimes;
		}

		protected abstract void DoPush(Request request);
	}
}
