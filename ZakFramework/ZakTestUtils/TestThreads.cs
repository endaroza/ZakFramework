﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using ZakCore.Utils.Collections;

namespace ZakTestUtils
{
	public class TestThreads
	{
		public class TestThreadsEventArgs : EventArgs
		{
			public int From { get; internal set; }
			public int To { get; internal set; }
			public int CurrentCycle { get; internal set; }
			public object Param { get; internal set; }
			public object Result { get; set; }
		}

		private readonly int _maxDegreeOfParallelism;
		private readonly bool _waitForTermination;
		private readonly EventHandler<TestThreadsEventArgs> _toExecute;
		public TestThreads(bool waitForTermination, EventHandler<TestThreadsEventArgs> toExecute, int maxDegreeOfParallelism = -1)
		{
			_waitForTermination = waitForTermination;
			_toExecute = toExecute;
			_maxDegreeOfParallelism = maxDegreeOfParallelism == -1 ? Environment.ProcessorCount : maxDegreeOfParallelism;
			_threads = new List<Thread>();
		}

		public List<Thread> _threads;
		public LockFreeQueue<Exception> Exceptions { get; private set; }
		public LockFreeQueue<object> Results { get; private set; }
		private long _runningThreads;
		private ManualResetEventSlim _eventStart;

		public CounterContainer CyclesCounter { get; set; }

		public long RunParallel(int count, object param = null,bool infinite=false)
		{
			var sw = new Stopwatch();

			CyclesCounter = new CounterContainer();
			Exceptions = new LockFreeQueue<Exception>();
			Results = new LockFreeQueue<object>();
			if (count % _maxDegreeOfParallelism != 0) throw new Exception();
			var steps = count / _maxDegreeOfParallelism;
			_runningThreads = _maxDegreeOfParallelism;
			_eventStart = new ManualResetEventSlim(false);

			for (int i = 0; i < _maxDegreeOfParallelism; i++)
			{
				var from = steps * i;
				var to = steps * (i + 1);
				var thread = new Thread(RunTask);
				thread.Start(new Tuple<int, int, object,bool>(from, to, param, infinite));
				_threads.Add(thread);
			}
			sw.Start();
			_eventStart.Set();
			if (_waitForTermination)
			{
				while (CyclesCounter.Counter < count)
				{
					Thread.Sleep(100);
				}
			}
			else
			{
				return 0;
			}
			sw.Stop();
			return sw.ElapsedMilliseconds;
		}

		public bool IsFinished { get { return Interlocked.Read(ref _runningThreads) <= 0; } }

		private void RunTask(object param)
		{
			var tuple = (Tuple<int, int, object, bool>)param;
			var from = tuple.Item1;
			var to = tuple.Item2;
			var par = tuple.Item3;
			var infinte = tuple.Item4;
			_eventStart.Wait(5000);
			if (infinte)
			{
				int i = 0;
				var sw = new Stopwatch();
				sw.Start();
				while (true)
				{
					var ea = new TestThreadsEventArgs
					{
						CurrentCycle = i,
						From = from,
						To = to,
						Param = par
					};
					_toExecute(this, ea);
					i++;
					if(sw.ElapsedMilliseconds<2) Thread.Sleep(2);
					else Thread.Sleep(1);
				}
			}
			for (int i = from; i < to; i++)
			{
				try
				{
					var ea = new TestThreadsEventArgs
					{
						CurrentCycle = i,
						From = from,
						To = to,
						Param = par
					};
					_toExecute(this, ea);
					Results.Enqueue(ea.Result);
					Thread.Sleep(1);
				}
				catch (Exception ex)
				{
					Exceptions.Enqueue(ex);
				}
				CyclesCounter.Increment();
			}
			Interlocked.Decrement(ref _runningThreads);
		}

		public void Terminate()
		{
			foreach (var thread in _threads)
			{
				thread.Abort();
			}
			_threads = new List<Thread>();
			_runningThreads = 0;
		}
	}
}
