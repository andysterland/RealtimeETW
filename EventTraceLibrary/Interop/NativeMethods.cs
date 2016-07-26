
using System;
using System.Runtime.InteropServices;

namespace Microsoft.Diagnostics.Eventing.EventLib.Interop
{
    /// An internal class that contains all of the marshaling and structure definitions for native API calls.
    internal static class NativeMethods
    {

        [DllImport("advapi32.dll", ExactSpelling = true, EntryPoint = "OpenTraceW", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern ulong OpenTrace(ref EventTraceLogfile logfile);

        [DllImport("advapi32.dll", ExactSpelling = true, EntryPoint = "ProcessTrace")]
        internal static extern int ProcessTrace(ulong[] HandleArray,
                                                 uint HandleCount,
                                                 IntPtr StartTime,
                                                 IntPtr EndTime);

        [DllImport("advapi32.dll", ExactSpelling = true, EntryPoint = "CloseTrace")]
        internal static extern int CloseTrace(ulong traceHandle);

        [DllImport("tdh.dll", ExactSpelling = true, EntryPoint = "TdhGetEventInformation")]
        internal static extern int TdhGetEventInformation(
            ref EventRecord Event,
            uint TdhContextCount,
            IntPtr TdhContext,
            [Out] IntPtr eventInfoPtr,
            ref int BufferSize);

        internal const uint FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
        internal const uint FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x00002000;
        internal const uint FORMAT_MESSAGE_FROM_STRING = 0x00000400;

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern uint FormatMessage(uint dwFlags, IntPtr lpSource,
           uint dwMessageId, uint dwLanguageId, ref IntPtr lpBuffer,
           uint nSize, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)] string[] args);







        // Win32 Error Codes
        internal const int ERROR_SUCCESS = 0;
        internal const int ERROR_CANCELLED = 1223;
        internal const int ERROR_WMI_INSTANCE_NOT_FOUND = 4201;
        internal const int ERROR_INSUFFICIENT_BUFFER = 122;
        internal const int ERROR_NOT_FOUND = 1168;
        internal const int ERROR_ALREADY_EXISTS = 183;

        #region Trace Controller API

        //	ULONG StartTrace(PTRACEHANDLE SessionHandle, LPCTSTR SessionName, PEVENT_TRACE_PROPERTIES Properties);
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        internal static extern uint StartTrace([Out] out ulong traceHandle, [In] string sessionName, [In] ref EVENT_TRACE_PROPERTIES properties);

        //	ULONG StopTrace(TRACEHANDLE SessionHandle, LPCTSTR SessionName, PEVENT_TRACE_PROPERTIES Properties);
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        internal static extern uint StopTrace([In]ulong traceHandle, [In] string sessionName, [In, Out] ref EVENT_TRACE_PROPERTIES properties);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        internal static extern uint EnableTraceEx([In] ref Guid providerID, [In] ref Guid sourceId, [In] ulong traceHandle, [In] uint enable, [In] byte level, [In] ulong matchAnyKeyword, [In] ulong matchAllKeyword, [In] uint enableProperty, [In] ref EVENT_FILTER_DESCRIPTOR enableFilterDesc);

        // ULONG FlushTrace(TRACEHANDLE SessionHandle, LPCTSTR SessionName, PEVENT_TRACE_PROPERTIES Properties);
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        internal static extern uint FlushTrace([In]ulong traceHandle, [In] string sessionName, [In, Out] ref EVENT_TRACE_PROPERTIES properties);

        //ULONG ControlTrace(__in TRACEHANDLE SessionHandle, __in LPCTSTR SessionName, __inout  PEVENT_TRACE_PROPERTIES Properties, __in     ULONG ControlCode);
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        internal static extern uint ControlTrace([In]ulong traceHandle, [In] string sessionName, [In, Out] ref EVENT_TRACE_PROPERTIES properties, ulong ControlCode);

        //ULONG __stdcall TdhEnumerateProviders(__out    PPROVIDER_ENUMERATION_INFO pBuffer, __inout  ULONG *pBufferSize);
        [DllImport("tdh.dll", CharSet = CharSet.Unicode)]
        internal static extern uint TdhEnumerateProviders([Out] IntPtr buffer, [In, Out] ref uint bufferSize);

        //ULONG WINAPI EnumerateTraceGuidsEx(
        //  __in   TRACE_QUERY_INFO_CLASS TraceQueryInfoClass,
        //  __in   PVOID InBuffer,
        //  __in   ULONG InBufferSize,
        //  __out  PVOID OutBuffer,
        //  __in   ULONG OutBufferSize,
        //  __out  PULONG ReturnLength
        //);
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        internal static extern uint EnumerateTraceGuidsEx([In] TRACE_QUERY_INFO_CLASS traceQueryInfoClass, [In] IntPtr inBuffer, [In] uint inBufferSize,
            [Out] IntPtr outBuffer, [In] uint outBufferSize, [Out] out uint returnLength);

        #endregion

        #region Trace Processing API


        //	ULONG ProcessTrace(
        //	  PTRACEHANDLE HandleArray, 
        //	  ULONG HandleCount,
        //	  LPFILETIME StartTime,
        //	  LPFILETIME EndTime
        //	);
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        internal static extern uint ProcessTrace(
            [In] ulong[] traceHandleArray,
            [In] int handleArrayLength,
            [In] ref long startFileTime,
            [In] ref long endFileTime);

        #endregion
    }

    #region Trace Enums and Constants

    internal enum EventTraceControl : ulong
    {
        QUERY = 0,
        STOP = 1,
        UPDATE = 2,
        FLUSH = 3
    }

    internal enum EventTraceFileMode : uint
    {
        NewFile = 0x00000008,  // switches to a new log file when the file reaches the maximum size
        FileSequential = 0x00000001, // Writes events to a log file sequentially
        RealTime = 0x00000100,   // log in real time
        UsePagedMemory = 0x01000000 // Uses paged memory. This setting is recommended so that events do not use up the nonpaged memory.
    }

    internal enum TRACE_QUERY_INFO_CLASS
    {
        TraceGuidQueryList,
        TraceGuidQueryInfo,
        TraceGuidQueryProcess
    }

    #endregion


    #region Miscellaneous structure definitions

    internal static class EventTracePropertiesMarshalSize
    {
        internal const uint StructSize = 120;
        internal const uint StringSize1 = 1024;
        internal const uint StringSize2 = 1024;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct EVENT_TRACE_PROPERTIES
    {
        internal WNODE_HEADER WNode;
        internal uint BufferSize;
        internal uint MinimumBuffers;
        internal uint MaximumBuffers;
        internal uint MaximumFileSize;
        internal EventTraceFileMode LogFileMode;
        internal uint FlushTimer;
        internal uint EnableFlags;
        internal int AgeLimit;
        internal uint NumberOfBuffers;
        internal uint FreeBuffers;
        internal uint EventsLost;
        internal uint BuffersWritten;
        internal uint LogBuffersLost;
        internal uint RealTimeBuffersLost;
        internal IntPtr LoggerThreadId;
        internal uint LogFileNameOffset;
        internal uint LoggerNameOffset;

        // The origianl EVENT_TRACE_PROPERTIES has only the above fields. However the memory for loggerName and
        // the logFileName are allocated at the end of struct. So in order to marshal this struct,
        // we add the extra fields here with a const size 1024 bytes (512 chars). 
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)EventTracePropertiesMarshalSize.StringSize1 / 2)]
        internal string LoggerName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)EventTracePropertiesMarshalSize.StringSize2 / 2)]
        internal string LogFileName;
    }

    #endregion

    #region WNODE_HEADER structure definition

    //	typedef struct _WNODE_HEADER
    //	{
    //	ULONG BufferSize;
    //	ULONG ProviderId;
    //	union
    //	{
    //		ULONG64 HistoricalContext;
    //		struct
    //		{
    //		ULONG Version;
    //		ULONG Linkage;
    //		};
    //	};
    //	union
    //	{
    //		HANDLE KernelHandle;
    //		LARGE_INTEGER TimeStamp;
    //	};
    //	GUID Guid;
    //	ULONG ClientContext;
    //	ULONG Flags;
    //	} WNODE_HEADER, *PWNODE_HEADER;
    // size = 48
    [StructLayout(LayoutKind.Sequential)]
    internal struct WNODE_HEADER
    {
        internal uint BufferSize;
        internal uint ProviderId;
        internal ContextVersionUnion ContextVersion;
        internal KernalTimestampUnion KernalTimestamp;
        internal Guid Guid;
        internal uint ClientContext;
        internal WNodeFlags Flags;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct ContextVersionUnion
    {
        [FieldOffset(0)]
        internal ulong HistoricalContext;

        [FieldOffset(0)]
        internal uint Version;
        [FieldOffset(4)]
        internal uint Linkage;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct KernalTimestampUnion
    {
        [FieldOffset(0)]
        internal uint CountLost;
        [FieldOffset(0)]
        internal IntPtr KernelHandle;
        [FieldOffset(0)]
        internal long TimeStamp;
    }

    [Flags]
    internal enum WNodeFlags : uint
    {
        UseGuidPtr = 0x00080000,  // Guid is actually a pointer
        TracedGuid = 0x00020000,  // denotes a trace
        UseMofPtr = 0x00100000  // MOF data are dereferenced
    }
    #endregion


    //typedef struct _EVENT_FILTER_DESCRIPTOR {
    //  ULONGLONG Ptr;
    //  ULONG     Size;
    //  ULONG     Type;
    //}EVENT_FILTER_DESCRIPTOR, *PEVENT_FILTER_DESCRIPTOR;
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct EVENT_FILTER_DESCRIPTOR
    {
        internal ulong Ptr;
        internal uint Size;
        internal uint Type;
    }

    //typedef struct _PROVIDER_ENUMERATION_INFO {
    //  ULONG               NumberOfProviders;
    //  ULONG               Padding;
    //  TRACE_PROVIDER_INFO TraceProviderInfoArray[ANYSIZE_ARRAY];
    //}PROVIDER_ENUMERATION_INFO;
    [StructLayout(LayoutKind.Sequential)]
    internal struct PROVIDER_ENUMERATION_INFO
    {
        internal UInt32 NumberOfProviders;
        internal UInt32 Padding;
        internal TRACE_PROVIDER_INFO[] TraceProviderInfoArray;
    }

    //typedef struct _TRACE_PROVIDER_INFO {
    //  GUID  ProviderGuid;
    //  ULONG SchemaSource;
    //  ULONG ProviderNameOffset;
    //}TRACE_PROVIDER_INFO;
    [StructLayout(LayoutKind.Sequential)]
    internal struct TRACE_PROVIDER_INFO
    {
        internal Guid ProviderGuid;
        internal UInt32 SchemaSource;
        internal UInt32 ProviderNameOffset;
    }

    //typedef struct _TRACE_PROVIDER_INSTANCE_INFO {
    //  ULONG NextOffset;
    //  ULONG EnableCount;
    //  ULONG Pid;
    //  ULONG Flags;
    //}TRACE_PROVIDER_INSTANCE_INFO, *PTRACE_PROVIDER_INSTANCE_INFO;
    [StructLayout(LayoutKind.Sequential)]
    internal struct TRACE_PROVIDER_INSTANCE_INFO
    {
        internal UInt32 NextOffset;
        internal UInt32 EnableCount;
        internal UInt32 Pid;
        internal UInt32 Flags;
    }

    //typedef struct _TRACE_ENABLE_INFO {
    //  ULONG     IsEnabled;
    //  UCHAR     Level;
    //  UCHAR     Reserved1;
    //  USHORT    LoggerId;
    //  ULONG     EnableProperty;
    //  ULONG     Reserved2;
    //  ULONGLONG MatchAnyKeyword;
    //  ULONGLONG MatchAllKeyword;
    //}TRACE_ENABLE_INFO, *PTRACE_ENABLE_INFO;
    [StructLayout(LayoutKind.Sequential)]
    internal struct TRACE_ENABLE_INFO
    {
        internal UInt32 IsEnabled;
        internal Byte Level;
        internal Byte Reserved1;
        internal UInt16 LoggerId;
        internal UInt32 EnableProperty;
        internal UInt32 Reserved2;
        internal UInt64 MatchAnyKeyword;
        internal UInt64 MatchAllKeyword;
    }



    internal delegate void EventRecordCallback([In] ref EventRecord eventRecord);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct Win32TimeZoneInfo
    {
        internal int Bias;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        internal char[] StandardName;
        internal SystemTime StandardDate;
        internal int StandardBias;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        internal char[] DaylightName;
        internal SystemTime DaylightDate;
        internal int DaylightBias;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SystemTime
    {
        internal short Year;
        internal short Month;
        internal short DayOfWeek;
        internal short Day;
        internal short Hour;
        internal short Minute;
        internal short Second;
        internal short Milliseconds;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct TraceLogfileHeader
    {
        internal uint BufferSize;
        internal uint Version;
        internal uint ProviderVersion;
        internal uint NumberOfProcessors;
        internal long EndTime;
        internal uint TimerResolution;
        internal uint MaximumFileSize;
        internal uint LogFileMode;
        internal uint BuffersWritten;
        internal Guid LogInstanceGuid;
        internal IntPtr LoggerName;
        internal IntPtr LogFileName;
        internal Win32TimeZoneInfo TimeZone;
        internal long BootTime;
        internal long PerfFreq;
        internal long StartTime;
        internal uint ReservedFlags;
        internal uint BuffersLost;
    }

    internal enum PropertyFlags
    {
        PropertyStruct = 0x1,
        PropertyParamLength = 0x2,
        PropertyParamCount = 0x4,
        PropertyWBEMXmlFragment = 0x8,
        PropertyParamFixedLength = 0x10
    }

    internal enum TdhInType : ushort
    {
        Null,
        UnicodeString,
        AnsiString,
        Int8,
        UInt8,
        Int16,
        UInt16,
        Int32,
        UInt32,
        Int64,
        UInt64,
        Float,
        Double,
        Boolean,
        Binary,
        Guid,
        Pointer,
        FileTime,
        SystemTime,
        SID,
        HexInt32,
        HexInt64,  // End of winmeta intypes
        CountedString = 300, // Start of TDH intypes for WBEM
        CountedAnsiString,
        ReversedCountedString,
        ReversedCountedAnsiString,
        NonNullTerminatedString,
        NonNullTerminatedAnsiString,
        UnicodeChar,
        AnsiChar,
        SizeT,
        HexDump,
        WbemSID
    };

    enum TdhOutType : ushort
    {
        Null,
        String,
        DateTime,
        Byte,
        UnsignedByte,
        Short,
        UnsignedShort,
        Int,
        UnsignedInt,
        Long,
        UnsignedLong,
        Float,
        Double,
        Boolean,
        Guid,
        HexBinary,
        HexInt8,
        HexInt16,
        HexInt32,
        HexInt64,
        PID,
        TID,
        PORT,
        IPV4,
        IPV6,
        SocketAddress,
        CimDateTime,
        EtwTime,
        Xml,
        ErrorCode,              // End of winmeta outtypes
        ReducedString = 300,    // Start of TDH outtypes for WBEM
        NoPrint
    };

    [StructLayout(LayoutKind.Explicit)]
    internal sealed class EventPropertyInfo
    {
        [FieldOffset(0)]
        internal PropertyFlags Flags;
        [FieldOffset(4)]
        internal UInt32 NameOffset;


        [StructLayout(LayoutKind.Sequential)]
        internal struct NonStructType
        {
            internal TdhInType InType;
            internal TdhOutType OutType;
            internal UInt32 MapNameOffset;
        }


        [StructLayout(LayoutKind.Sequential)]
        internal struct StructType
        {
            internal ushort StructStartIndex;
            internal ushort NumOfStructMembers;
            private UInt32 _Padding;
        }

        [FieldOffset(8)]
        internal NonStructType NonStructTypeValue;
        [FieldOffset(8)]
        internal StructType StructTypeValue;

        [FieldOffset(16)]
        internal ushort CountPropertyIndex;
        [FieldOffset(18)]
        internal ushort LengthPropertyIndex;
        [FieldOffset(20)]
        private UInt32 _Reserved;
    }

    internal enum TemplateFlags
    {
        TemplateEventDdata = 1,
        TemplateUserData = 2
    }

    internal enum DecodingSource
    {
        DecodingSourceXmlFile,
        DecodingSourceWbem,
        DecodingSourceWPP
    }

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class TraceEventInfo
    {
        internal Guid ProviderGuid;
        internal Guid EventGuid;
        internal EtwEventDescriptor EventDescriptor;
        internal DecodingSource DecodingSource;
        internal UInt32 ProviderNameOffset;
        internal UInt32 LevelNameOffset;
        internal UInt32 ChannelNameOffset;
        internal UInt32 KeywordsNameOffset;
        internal UInt32 TaskNameOffset;
        internal UInt32 OpcodeNameOffset;
        internal UInt32 EventMessageOffset;
        internal UInt32 ProviderMessageOffset;
        internal UInt32 BinaryXmlOffset;
        internal UInt32 BinaryXmlSize;
        internal UInt32 ActivityIDNameOffset;
        internal UInt32 RelatedActivityIDNameOffset;
        internal UInt32 PropertyCount;
        internal UInt32 TopLevelPropertyCount;
        internal TemplateFlags Flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct EventTraceHeader
    {
        internal ushort Size;
        internal ushort FieldTypeFlags;
        internal uint Version;
        internal uint ThreadId;
        internal uint ProcessId;
        internal long TimeStamp;
        internal Guid Guid;
        internal uint KernelTime;
        internal uint UserTime;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct EventTrace
    {
        internal EventTraceHeader Header;
        internal uint InstanceId;
        internal uint ParentInstanceId;
        internal Guid ParentGuid;
        internal IntPtr MofData;
        internal uint MofLength;
        internal uint ClientContext;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct EventTraceLogfile
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        internal string LogFileName;
        [MarshalAs(UnmanagedType.LPWStr)]
        internal string LoggerName;
        internal Int64 CurrentTime;
        internal UInt32 BuffersRead;
        internal UInt32 ProcessTraceMode;
        internal EventTrace CurrentEvent;
        internal TraceLogfileHeader LogfileHeader;
        internal IntPtr BufferCallback;
        internal UInt32 BufferSize;
        internal UInt32 Filled;
        internal UInt32 EventsLost;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        internal EventRecordCallback EventRecordCallback;
        internal UInt32 IsKernelTrace;
        internal IntPtr Context;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct EtwEventDescriptor
    {
        internal ushort Id;
        internal byte Version;
        internal byte Channel;
        internal byte Level;
        internal byte Opcode;
        internal ushort Task;
        internal ulong Keyword;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct EventHeader
    {

        internal ushort Size;
        internal ushort HeaderType;
        internal ushort Flags;
        internal ushort EventProperty;
        internal UInt32 ThreadId;
        internal UInt32 ProcessId;
        internal Int64 TimeStamp;
        internal Guid ProviderId;
        internal EtwEventDescriptor EventDescriptor;
        internal ulong ProcessorTime;
        internal Guid ActivityId;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct EventRecord
    {

        internal EventHeader EventHeader;
        internal EtwBufferContext BufferContext;
        internal ushort ExtendedDataCount;
        internal ushort UserDataLength;
        internal IntPtr ExtendedData;
        internal IntPtr UserData;
        internal IntPtr UserContext;


        [StructLayout(LayoutKind.Sequential)]
        internal struct EtwBufferContext
        {
            internal byte ProcessorNumber;
            internal byte Alignment;
            internal ushort LoggerId;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SYSTEMTIME
    {
        public ushort wYear;
        public ushort wMonth;
        public ushort wDayOfWeek;
        public ushort wDay;
        public ushort wHour;
        public ushort wMinute;
        public ushort wSecond;
        public ushort wMilliseconds;
    } 
 


}
