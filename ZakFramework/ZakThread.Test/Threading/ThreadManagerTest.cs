﻿using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using ZakCore.Utils.Logging;
using ZakThread.Test.Threading.Simple;
using ZakThread.Threading;
using ZakThread.Threading.Enums;

namespace ZakThread.Test.Threading
{
	[TestFixture]
	public class ThreadManagerTest
	{
		[Test]
		public void ItShouldBBePossibleToCreateAThreadManagerAndTerminatingIt()
		{
			var threadManager = new ThreadManager(NullLogger.Create());
			threadManager.RunThread();
			Thread.Sleep(100);
			Assert.IsTrue(threadManager.Status == RunningStatus.Running);
			threadManager.Terminate();
			Thread.Sleep(1000);
			Assert.IsTrue(threadManager.Status == RunningStatus.Halted);
		}

		[Test]
		public void ItShouldBBePossibleToCreateAThreadManagerWithChildThreads()
		{
			var subThread = new List<BaseMessageThread>();
			for (int i = 0; i < 10; i++)
			{
				subThread.Add(new SimpleMessageThreadConsumer(1, "ThreadSub" + i));
			}
			var threadManager = new ThreadManager(NullLogger.Create());
			threadManager.RunThread();
			Thread.Sleep(100);
			foreach (var item in subThread)
			{
				threadManager.AddThread(item);
				//threadManager.RunThread(item.ThreadName);
			}
			Thread.Sleep(1000);
			Assert.AreEqual(RunningStatus.Running, threadManager.Status);
			foreach (var item in subThread)
			{
				Assert.AreEqual(RunningStatus.Running, item.Status);
			}
			threadManager.Terminate();
			Thread.Sleep(500);
			Assert.AreEqual(RunningStatus.Halted, threadManager.Status);
			foreach (var item in subThread)
			{
				Assert.AreEqual(RunningStatus.Halted, item.Status);
			}
		}

		[Test]
		public void ItShouldBBePossibleToCreateAThreadManagerWithChildThreadsAndDisposeIt()
		{
			var subThread = new List<BaseMessageThread>();
			for (int i = 0; i < 10; i++)
			{
				subThread.Add(new SimpleMessageThreadConsumer(1, "ThreadSub" + i));
			}
			var threadManager = new ThreadManager(NullLogger.Create());
			threadManager.RunThread();
			Thread.Sleep(100);
			foreach (var item in subThread)
			{
				threadManager.AddThread(item);
				threadManager.RunThread(item.ThreadName);
			}
			Thread.Sleep(1000);
			Assert.IsTrue(threadManager.Status == RunningStatus.Running);
			foreach (var item in subThread)
			{
				Assert.IsTrue(item.Status == RunningStatus.Running);
			}
			threadManager.Dispose();
			Thread.Sleep(500);
			Assert.IsTrue(threadManager.Status == RunningStatus.Aborted);
			foreach (var item in subThread)
			{
				Assert.IsTrue(item.Status == RunningStatus.AbortedOnCleanup || 
					item.Status == RunningStatus.Aborted || 
					item.Status == RunningStatus.Halted);
			}
		}


		[Test]
		public void ItShouldBBePossibleToRemoveChildThreads()
		{
			var subThread = new List<BaseMessageThread>();
			for (int i = 0; i < 10; i++)
			{
				subThread.Add(new SimpleMessageThreadConsumer(1, "ThreadSub" + i));
			}
			var threadManager = new ThreadManager(NullLogger.Create());
			threadManager.RunThread();
			Thread.Sleep(100);
			foreach (var item in subThread)
			{
				threadManager.AddThread(item);
				//threadManager.RunThread(item.ThreadName);
			}
			Thread.Sleep(1000);
			Assert.IsTrue(threadManager.Status == RunningStatus.Running);
			foreach (var item in subThread)
			{
				Assert.IsTrue(item.Status == RunningStatus.Running);
				threadManager.RemoveThread(item);
			}
			Thread.Sleep(500);
			foreach (var item in subThread)
			{
				Assert.IsTrue(item.Status == RunningStatus.Halted || item.Status == RunningStatus.Halting);
			}
			threadManager.Terminate();
			Thread.Sleep(500);
			Assert.IsTrue(threadManager.Status == RunningStatus.Halted);
		}


		[Test]
		public void ItShouldBBePossibleToRemoveChildThreadsByName()
		{
			var subThread = new List<BaseMessageThread>();
			for (int i = 0; i < 10; i++)
			{
				subThread.Add(new SimpleMessageThreadConsumer(1, "ThreadSub" + i));
			}
			var threadManager = new ThreadManager(NullLogger.Create());
			threadManager.RunThread();
			Thread.Sleep(100);
			foreach (var item in subThread)
			{
				threadManager.AddThread(item);
				//threadManager.RunThread(item.ThreadName);
			}
			Thread.Sleep(1000);
			Assert.AreEqual(RunningStatus.Running, threadManager.Status);
			foreach (var item in subThread)
			{
				Assert.AreEqual(RunningStatus.Running, item.Status);
				threadManager.RemoveThread(item.ThreadName);
			}
			Thread.Sleep(500);
			foreach (var item in subThread)
			{
				Assert.AreEqual(RunningStatus.Halted, item.Status);
			}
			threadManager.Terminate();
			Thread.Sleep(500);
			Assert.AreEqual(RunningStatus.Halted, threadManager.Status);
		}

		[Test]
		public void ItShouldBBePossibleToRunAManagerTerminatingItFromChild()
		{
			var subThread = new List<BaseMessageThread>();
			for (int i = 0; i < 10; i++)
			{
				subThread.Add(new SimpleMessageThreadConsumer(1, "ThreadSub" + i));
			}
			var threadManager = new ThreadManager(NullLogger.Create());
			threadManager.RunThread();
			Thread.Sleep(100);
			foreach (var item in subThread)
			{
				threadManager.AddThread(item);
				//threadManager.RunThread(item.ThreadName);
			}
			Thread.Sleep(1000);
			Assert.IsTrue(threadManager.Status == RunningStatus.Running);
			foreach (var item in subThread)
			{
				Assert.IsTrue(item.Status == RunningStatus.Running);
			}
			((SimpleMessageThreadConsumer)subThread[0]).SendTerminationMessage();
			Thread.Sleep(500);
			Assert.IsTrue(threadManager.Status == RunningStatus.Aborted);
			foreach (var item in subThread)
			{
				Assert.IsTrue(item.Status == RunningStatus.Aborted);
			}
			threadManager.Terminate();
			Thread.Sleep(500);
		}
	}
}
