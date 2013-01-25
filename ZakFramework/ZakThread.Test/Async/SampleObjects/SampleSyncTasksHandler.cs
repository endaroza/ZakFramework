﻿using System.Collections.Generic;
using System.Threading;
using ZakCore.Utils.Logging;
using ZakThread.Async;
using System;
using System.Diagnostics;

namespace ZakThread.Test.Async.SampleObjects
{
	public class SampleSyncTasksHandler : BaseSyncHandlerThread
	{
		private readonly int _waitTimeMs;
		private long _callsCount;

		public long CallsCount { get { return Interlocked.Read(ref _callsCount); } }

		public SampleSyncTasksHandler(string threadName, int waitTimeMs, int batchSize = 0, int batchTimeoutMs = 0) :
			base(NullLogger.Create(), threadName, true)
		{
			BatchSize = batchSize;
			BatchTimeoutMs = batchTimeoutMs;
			MaxMesssagesPerCycle = 500;
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

		public override void RunThread(int timeoutMs = 1000)
		{
			base.RunThread(timeoutMs);
			Debug.WriteLine("ThreadStarted");
		}

		public override void HandleBatchCompleted(IEnumerable<RequestObjectMessage> batchExecuted)
		{
			foreach (var item in batchExecuted)
			{
				Interlocked.Increment(ref _callsCount);
			}
		}
	}
}
