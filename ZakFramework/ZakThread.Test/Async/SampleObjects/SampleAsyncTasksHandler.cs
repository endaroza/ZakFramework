﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using ZakCore.Utils.Logging;
using ZakThread.Async;

namespace ZakThread.Test.Async.SampleObjects
{
	public class SampleAsyncTasksHandler : BaseAsyncHandlerThread
	{
		private readonly int _waitTimeMs;
		private long _callsCount;

		public long CallsCount { get { return Interlocked.Read(ref _callsCount); } }

		public SampleAsyncTasksHandler(string threadName, int waitTimeMs, int batchSize = 0, int batchTimeoutMs = 0) :
			base(NullLogger.Create(), threadName, true)
		{
			BatchSize = batchSize;
			BatchTimeoutMs = batchTimeoutMs;
			_waitTimeMs = waitTimeMs;
			_callsCount = 0;
		}

		public override bool HandleTaskRequest(RequestObjectMessage container, BaseRequestObject requestObject)
		{
			var request = (RequestObject)requestObject;
			request.Return = -request.RequestId;
			if (BatchSize == 1) Interlocked.Increment(ref _callsCount);
			Thread.Sleep(_waitTimeMs);
			return true;
		}

		public override void RunThread(int timeoutMs = 0)
		{
			base.RunThread(timeoutMs);
			Debug.WriteLine("ThreadStarted");
		}

		public override void HandleBatchCompleted(IEnumerable<RequestObjectMessage> batchExecuted)
		{
			foreach (var item in batchExecuted)
			{
				if(item.Id!= Guid.Empty) Interlocked.Increment(ref _callsCount);
			}
		}

		protected override bool HandleMessage(ZakThread.Threading.IMessage msg)
		{
			return true;
		}
	}
}

