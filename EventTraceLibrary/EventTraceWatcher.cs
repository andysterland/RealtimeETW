//------------------------------------------------------------------------------
// http://code.msdn.microsoft.com/EventTraceWatcher
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Linq;
using Microsoft.Diagnostics.Eventing.EventLib.Interop;

namespace Microsoft.Diagnostics.Eventing.EventLib
{

    public sealed class EventTraceWatcher : IDisposable
    {
        private readonly string _loggerName;
        private volatile bool _enabled;
        private EventTraceLogfile _logFile;
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private ulong _traceHandle;
        private IAsyncResult _asyncResult;
        private ProcessTraceDelegate _processEventsDelgate;

        private delegate void ProcessTraceDelegate(ulong traceHandle);

        public EventTraceWatcher(string loggerName)
        {
            _loggerName = loggerName;
        }

        ~EventTraceWatcher()
        {
            Cleanup();
        }

        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                _lock.EnterReadLock();
                if (_enabled == value)
                {
                    _lock.ExitReadLock();
                    return;
                }
                _lock.ExitReadLock();

                _lock.EnterWriteLock();
                try
                {
                    if (value)
                    {
                        StartTracing();
                    }
                    else
                    {
                        StopTracing();
                    }
                    _enabled = value;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }

        public event EventHandler<EventArrivedEventArgs> EventArrived;

        private void Cleanup()
        {
            this.Enabled = false;
            _lock.Dispose();
        }

        private static byte[] CreateComposedKey(Guid providerId, byte opcode)
        {
            const int GuidSizePlusOpcodeSize = 17;
            byte[] key = new byte[GuidSizePlusOpcodeSize];
            // Copy guid
            Buffer.BlockCopy(providerId.ToByteArray(), 0, key, 0, GuidSizePlusOpcodeSize - 1);
            // Copy opcode
            key[GuidSizePlusOpcodeSize - 1] = opcode;
            return key;
        }

        private EventArrivedEventArgs CreateEventArgsFromEventRecord(EventRecord eventRecord)
        {
            using (TraceEventInfoWrapper traceEventInfo = new TraceEventInfoWrapper(eventRecord))
            {
                Guid providerId = eventRecord.EventHeader.ProviderId;
                try
                {
                    // Get the properties using the current event information (schema).
                    IList<PropertyItem> properties = traceEventInfo.GetProperties(eventRecord);
                    string fmtmsg = traceEventInfo.GetMessage();
                    string message = traceEventInfo.FormatMessage(fmtmsg, properties.Select(p => p.Value.ToString()).ToArray());

                    EventArrivedEventArgs args = new EventArrivedEventArgs(providerId, traceEventInfo.EventName, properties);
                    args.Message = message;
                    args.LogDateTime = DateTime.FromFileTime(eventRecord.EventHeader.TimeStamp);
                    args.Task = traceEventInfo.GetTask();
                    args.Level = eventRecord.EventHeader.EventDescriptor.Level;
                    args.LevelName = traceEventInfo.GetLevelName();
                    args.ProcessId = eventRecord.EventHeader.ProcessId;
                    args.ThreadId = eventRecord.EventHeader.ThreadId;

                    return args;
                }
                catch (Exception e)
                {
                    // there are some error when creating a event arg, so use the exception as the message
                    EventArrivedEventArgs args = new EventArrivedEventArgs(e);
                    args.Task = string.Empty;
                    return args;
                }
            }
        }

        public void Dispose()
        {
            Cleanup();
            GC.SuppressFinalize(this);
        }

        private void EventRecordCallback([In] ref EventRecord eventRecord)
        {
            if (EventArrived != null)
            {
                EventArrivedEventArgs e = CreateEventArgsFromEventRecord(eventRecord);
                if (e != null)
                {
                    EventArrived(this, e);
                }
            }
        }

        private void ProcessTraceInBackground(ulong traceHandle)
        {
            Exception asyncException = null;

            try
            {
                ulong[] array = { traceHandle };
                int error = NativeMethods.ProcessTrace(array, 1, IntPtr.Zero, IntPtr.Zero);
                if (error != 0)
                {
                    throw new System.ComponentModel.Win32Exception(error);
                }
            }
            catch (Exception exception)
            {
                asyncException = exception;
            }

            // Send exception to subscribers.
            if (asyncException != null && this.EventArrived != null)
            {
                this.EventArrived(this, new EventArrivedEventArgs(asyncException));
            }
        }

        private void StartTracing()
        {
            const uint RealTime = 0x00000100;
            const uint EventRecord = 0x10000000;

            _logFile = new EventTraceLogfile();
            _logFile.LoggerName = _loggerName;
            _logFile.EventRecordCallback = EventRecordCallback;

            _logFile.ProcessTraceMode = EventRecord | RealTime;
            _traceHandle = NativeMethods.OpenTrace(ref _logFile);

            int error = Marshal.GetLastWin32Error();
            if (error != 0)
            {
                throw new System.ComponentModel.Win32Exception(error);
            }

            _processEventsDelgate = new ProcessTraceDelegate(ProcessTraceInBackground);
            _asyncResult = _processEventsDelgate.BeginInvoke(_traceHandle, null, _processEventsDelgate);
        }

        private void StopTracing()
        {
            NativeMethods.CloseTrace(_traceHandle);
            _traceHandle = 0;
            _processEventsDelgate.EndInvoke(_asyncResult);
        }

    }
}