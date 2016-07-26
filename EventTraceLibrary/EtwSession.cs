using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Diagnostics.Eventing.EventLib.Interop;

namespace Microsoft.Diagnostics.Eventing.EventLib
{
    /// <summary>
    /// Represents an ETW session
    /// </summary>
    public class EtwSession
    {
        private ulong _handle;
        private EtwSessionSetting setting;

        public EtwSession(EtwSessionSetting setting)
        {
            setting.Validate();
            this.setting = setting;
            // get the handle if the session exists
            GetSessionHandle();
        }

        public bool IsValid
        {
            get
            {
                GetSessionHandle();
                return _handle != 0;
            }
        }

        /// <summary>
        /// Create an ETW session 
        /// </summary>
        public void StartTrace()
        {
            if (_handle == 0)
            {
                EVENT_TRACE_PROPERTIES properties = CommonEvenTraceProperties();
                properties.WNode.Flags = WNodeFlags.TracedGuid;
                properties.WNode.ClientContext = 1; //QPC clock resolution
                properties.BufferSize = (uint)setting.BufferSize;
                properties.MinimumBuffers = (uint)setting.MinBuffers;
                properties.MaximumBuffers = (uint)setting.MaxBuffers;
                if (setting.RealTimeSession)
                {
                    // real time mode
                    properties.LogFileMode = EventTraceFileMode.RealTime;
                }
                else if (setting.UseNewFileMode)
                {
                    // NewFile mode
                    properties.LogFileMode = EventTraceFileMode.NewFile;
                    properties.MaximumFileSize = (uint)this.setting.MaxFileSize;
                    properties.FlushTimer = (uint)this.setting.FlushTimer;
                }
                else
                {
                    // rolling file hourly. 
                    properties.LogFileMode = EventTraceFileMode.FileSequential;
                    properties.MaximumFileSize = 0;
                    properties.FlushTimer = (uint)this.setting.FlushTimer;
                }

                // From MSDN:
                // Uses paged memory. This setting is recommended so that events do not use up the nonpaged memory.
                // Kernel-mode providers cannot log events to sessions that specify this logging mode.
                if (setting.UsePagedMemory)
                {
                    properties.LogFileMode |= EventTraceFileMode.UsePagedMemory;
                }

                uint processResult = NativeMethods.StartTrace(out _handle, setting.Name, ref properties);
                if (processResult != NativeMethods.ERROR_SUCCESS &&
                    processResult != NativeMethods.ERROR_ALREADY_EXISTS)
                {
                    throw new Win32Exception((int)processResult);
                }
            }
            // if _handle != 0, the session has already been created
        }

        /// <summary>
        /// Stops and delete the session
        /// </summary>
        public void StopTrace()
        {
            if (_handle != 0)
            {
                EVENT_TRACE_PROPERTIES properties = CommonEvenTraceProperties();

                uint processResult = NativeMethods.StopTrace(_handle, setting.Name, ref properties);

                if (processResult != NativeMethods.ERROR_SUCCESS &&
                    processResult != NativeMethods.ERROR_CANCELLED &&
                    processResult != NativeMethods.ERROR_WMI_INSTANCE_NOT_FOUND) // It is expected that the ETW session might not exist
                {
                    throw new Win32Exception((int)processResult);
                }
                _handle = 0;
            }
        }

        public void FlushTrace()
        {
            if (IsValid)
            {
                EVENT_TRACE_PROPERTIES properties = CommonEvenTraceProperties();
                uint status = NativeMethods.ControlTrace(0, this.setting.Name, ref properties, (ulong)EventTraceControl.FLUSH);

                if (status != NativeMethods.ERROR_SUCCESS)
                {
                    throw new Win32Exception((int)status);
                }

            }
        }

        /// <summary>
        /// Enable the provider for this session
        /// </summary>
        /// <param name="providerId"></param>
        public void EnableProvider(Guid providerId)
        {
            EnableProvider(providerId, 0xff, 0, 0);
        }

        public void EnableProvider(Guid providerId, byte level, ulong anyKeyword, ulong allKeyword)
        {
            EVENT_FILTER_DESCRIPTOR filter = new EVENT_FILTER_DESCRIPTOR();
            Guid emptyGuid = Guid.Empty;
            uint status = NativeMethods.EnableTraceEx(
                                ref providerId,       // Provider GUID
                                ref emptyGuid,                // Not passing a Session ID to the provider's callback
                                _handle,       // Session handle from StartTrace
                                1,				 // TRUE enables the provider
                                level,				 // log all levels since we defined our levels per component
                                anyKeyword,					 // To include all events that a provider provides, set MatchAnyKeyword to zero 
                                allKeyword,                   // No MatchAllKeyword value
                                0,                   // Do not include SID or terminal session identifier in event 
                                ref filter                 // No provider-defined data to pass
                                );
        }

        public void DisableProvider(Guid providerId)
        {
            EVENT_FILTER_DESCRIPTOR filter = new EVENT_FILTER_DESCRIPTOR();
            Guid emptyGuid = Guid.Empty;
            uint status = NativeMethods.EnableTraceEx(
                                ref providerId,       // Provider GUID
                                ref emptyGuid,                // Not passing a Session ID to the provider's callback
                                _handle,       // Session handle from StartTrace
                                0,				 // FALSE to disable the provider
                                0xff,				 // log all levels since we defined our levels per component
                                0,					 // To include all events that a provider provides, set MatchAnyKeyword to zero 
                                0,                   // No MatchAllKeyword value
                                0,                   // Do not include SID or terminal session identifier in event 
                                ref filter                 // No provider-defined data to pass
                                );

            if (status != NativeMethods.ERROR_NOT_FOUND && 
                status != NativeMethods.ERROR_SUCCESS)
            {
                throw new Win32Exception((int)status);
            }
        }

        private void GetSessionHandle()
        {
            EVENT_TRACE_PROPERTIES properties = CommonEvenTraceProperties();
            uint status = NativeMethods.ControlTrace(0, this.setting.Name, ref properties, (ulong)EventTraceControl.QUERY);
            if (status == NativeMethods.ERROR_SUCCESS)
            {
                _handle = properties.WNode.ContextVersion.HistoricalContext;
            }
            else
            {
                _handle = 0;
            }
        }

        public bool IsEnabledForProvider(Guid providerGuid)
        {
            IntPtr inBuffer = IntPtr.Zero;
            IntPtr outBuffer = IntPtr.Zero;
            try
            {
                inBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Guid)));
                Marshal.StructureToPtr(providerGuid, inBuffer, true);

                uint len;
                uint status = NativeMethods.EnumerateTraceGuidsEx(TRACE_QUERY_INFO_CLASS.TraceGuidQueryInfo,
                    inBuffer, (uint)Marshal.SizeOf(typeof(Guid)), IntPtr.Zero, 0, out len);

                outBuffer = Marshal.AllocHGlobal((int)len);
                uint len2;
                status = NativeMethods.EnumerateTraceGuidsEx(TRACE_QUERY_INFO_CLASS.TraceGuidQueryInfo,
                    inBuffer, (uint)Marshal.SizeOf(typeof(Guid)), outBuffer, len, out len2);

                if (status == NativeMethods.ERROR_SUCCESS)
                {
                    // read InstanceCount from _TRACE_GUID_INFO 
                    int instanceCount = Marshal.ReadInt32(outBuffer);
                    if (instanceCount > 0)
                    {
                        // 8 is the size of TRACE_GUID_INFO 
                        long tpiOffset = outBuffer.ToInt64() + 8;
                        TRACE_PROVIDER_INSTANCE_INFO tpi;
                        do
                        {
                            tpi = (TRACE_PROVIDER_INSTANCE_INFO)Marshal.PtrToStructure((IntPtr)tpiOffset, typeof(TRACE_PROVIDER_INSTANCE_INFO));
                            long teiStart = tpiOffset + Marshal.SizeOf(typeof(TRACE_PROVIDER_INSTANCE_INFO));
                            for (int i = 0; i < tpi.EnableCount; i++)
                            {
                                //Each instance info block contains a TRACE_ENABLE_INFO structure for each session that enabled the provider
                                long teiOffset = teiStart + i * Marshal.SizeOf(typeof(TRACE_ENABLE_INFO));
                                TRACE_ENABLE_INFO tei = (TRACE_ENABLE_INFO)Marshal.PtrToStructure((IntPtr)teiOffset, typeof(TRACE_ENABLE_INFO));
                                if (tei.LoggerId == _handle)
                                {
                                    return true;
                                }
                            }
                            // get the next TRACE_PROVIDER_INSTANCE_INFO
                            tpiOffset += tpi.NextOffset;
                        } while (tpi.NextOffset != 0);
                    }
                }
                return false;
            }
            finally
            {
                Marshal.FreeHGlobal(inBuffer);
                Marshal.FreeHGlobal(outBuffer);
            }
        }

        public void UpdateLogFileName(string newpath)
        {
            if (setting.RealTimeSession || setting.UseNewFileMode)
            {
                throw new NotSupportedException("Updating session log file name is not supported for real-time session or EVENT_TRACE_FILE_MODE_NEWFILE mode");
            }

            EVENT_TRACE_PROPERTIES properties = CommonEvenTraceProperties();

            // Set LogFileNameOffset member if you want to switch to another log file.
            properties.LogFileNameOffset = EventTracePropertiesMarshalSize.StructSize + EventTracePropertiesMarshalSize.StringSize1;
            properties.LogFileName = newpath;

            uint status = NativeMethods.ControlTrace(0, this.setting.Name, ref properties, (ulong)EventTraceControl.UPDATE);

        }

        private EVENT_TRACE_PROPERTIES CommonEvenTraceProperties()
        {
            EVENT_TRACE_PROPERTIES properties = new EVENT_TRACE_PROPERTIES();

            properties.WNode.BufferSize = EventTracePropertiesMarshalSize.StructSize + EventTracePropertiesMarshalSize.StringSize1 + EventTracePropertiesMarshalSize.StringSize2;
            properties.WNode.Flags = WNodeFlags.TracedGuid;
            properties.LoggerNameOffset = EventTracePropertiesMarshalSize.StructSize;
            if (this.setting.LogFilePath == null)
            {
                // real time session
                properties.LogFileNameOffset = 0;
            }
            else
            {
                // file mode
                // copy the log file path 
                properties.LogFileNameOffset = EventTracePropertiesMarshalSize.StructSize + EventTracePropertiesMarshalSize.StringSize1;
                properties.LogFileName = setting.LogFilePath;
            }
            return properties;
        }
    }

    /// <summary>
    /// A setting files 
    /// </summary>
    public class EtwSessionSetting
    {
        public string Name {get; set; }
        public bool RealTimeSession { get; set; }
        public string LogFilePath { get; set; }
        public int MaxFileSize { get; set; }
        public int FlushTimer { get; set; }
        public int BufferSize { get; set; }
        public int MinBuffers { get; set; }
        public int MaxBuffers { get; set; }
        public bool UsePagedMemory { get; set; }

        // indicate whether to use EVENT_TRACE_FILE_MODE_NEWFILE, i.e. ETW will automatically swtich to a new file when it reaches the max file size
        public bool UseNewFileMode { get; set; }

        public void Validate()
        {
            if (string.IsNullOrEmpty(Name))
            {
                throw new ArgumentException("sessionName", "Session name can't be null");
            }

            if (!RealTimeSession && string.IsNullOrEmpty(LogFilePath))
            {
                throw new ArgumentException("logFilePath", "log file name must be specified for non-real time session");
            }

            if (MinBuffers != 0 && MaxBuffers != 0 && MaxBuffers < MinBuffers)
            {
                throw new ArgumentException("maximumBuffers", "maximumBuffers must be greater than or equal to the value for minimumBuffers.");
            }

            if (UseNewFileMode)
            {
                if (MaxFileSize == 0)
                {
                    throw new ArgumentException("maxFileSize", "maxFileSize must be specified if in NEWFILE mode");
                }
                if (! LogFilePath.Contains("%d"))
                {
                    throw new ArgumentException("logFilePath", "Must specify '%d' in the logFilePath if using NEWFILE mode");
                }
            }
        }
    }
}
