﻿using System;
using System.Threading;
using ZakCore.Utils.Logging;
using ZakThread.Threading;
using ZakThread.Threading.ThreadManagerInternals;

namespace ZakThread.Test.Threading.Simple
{
	public class SimpleMessageThreadConsumer : BaseMessageThread
	{
		public Int64 HandledMessages;

		public Exception ThrowExceptionOnMessageHandling { get; set; }

		public SimpleMessageThreadConsumer(int sleepTime, string threadName, bool restartOnError = true) :
			base(NullLogger.Create(), threadName, restartOnError)
		{
			ReceiveAStopMessage = false;
			ForwardMessages = false;
			ThrowExceptionOnInitialization = null;
			ThrowExceptionOnCyclicExecution = null;
			ThrowExceptionOnCleanUp = null;
			ResetExceptionAfterThrow = false;
			IsInitialized = false;
			IsCleanedUp = false;
			IsExceptionHandled = false;
			_sleepTime = sleepTime;
		}

		public void SendTerminationMessage()
		{
			SendMessage(new InternalMessage(InternalMessageTypes.Terminate, true));
		}

		protected override bool HandleMessage(IMessage msg)
		{
			HandledMessages++;
			if (ForwardMessages)
			{
				SendMessage(msg);
			}
			if (ReceiveAStopMessage) return false;
			if (ThrowExceptionOnMessageHandling != null)
			{
				var throwing = ThrowExceptionOnMessageHandling;
				ThrowExceptionOnMessageHandling = null;
				throw throwing;
			}
			return true;
		}

		public override void RegisterMessages()
		{
		}

		private readonly int _sleepTime;

		public bool IsInitialized { get; private set; }

		public bool IsCleanedUp { get; private set; }

		public bool IsExceptionHandled { get; private set; }
		public bool ForwardMessages { set; private get; }
		public bool ReceiveAStopMessage { set; private get; }

		public bool ResetExceptionAfterThrow { get; set; }

		public Exception ThrowExceptionOnCyclicExecution { set; get; }

		public Exception ThrowExceptionOnInitialization { set; get; }

		public Exception ThrowExceptionOnCleanUp { set; get; }


		protected override bool RunSingleCycle()
		{
			if (ThrowExceptionOnCyclicExecution != null)
			{
				Exception toThrow = ThrowExceptionOnCyclicExecution;
				if (ResetExceptionAfterThrow) ThrowExceptionOnCyclicExecution = null;
				if (toThrow != null) throw toThrow;
			}

			Thread.Sleep(_sleepTime);
			return true;
		}

		protected override void Initialize()
		{
			if (ThrowExceptionOnInitialization != null) throw ThrowExceptionOnInitialization;
			IsInitialized = true;
		}

		protected override bool HandleException(Exception ex)
		{
			IsExceptionHandled = true;
			return true;
		}

		protected override void CleanUp()
		{
			if (ThrowExceptionOnCleanUp != null) throw ThrowExceptionOnCleanUp;
			IsCleanedUp = true;
		}

		internal void SetMaxMessagePerCycle(int p)
		{
			MaxMesssagesPerCycle = p;
		}
	}
}