//------------------------------------------------------------------------------
// http://code.msdn.microsoft.com/EventTraceWatcher
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Diagnostics.Eventing.EventLib.Interop
{

    internal sealed class TraceEventInfoWrapper : IDisposable
    {

        /// <summary>
        /// Base address of the native TraceEventInfo structure.
        /// </summary>
        private IntPtr _address;

        /// <summary>
        /// Managed representation of the native TraceEventInfo structure.
        /// </summary>
        private TraceEventInfo _traceEventInfo;

        //
        // True if the event has a schema with well defined properties.
        //
        private bool _hasProperties;

        /// <summary>
        /// Marshalled array of EventPropertyInfo objects.
        /// </summary>
        private EventPropertyInfo[] _eventPropertyInfoArray;

        internal TraceEventInfoWrapper(EventRecord eventRecord)
        {
            Initialize(eventRecord);
        }

        ~TraceEventInfoWrapper()
        {
            ReleaseMemory();
        }

        internal string EventName
        {
            private set;
            get;
        }

        internal string GetMessage()
        {
            if (_traceEventInfo.EventMessageOffset == 0)
            {
                return string.Empty;
            }
            else
            {
                string msg = Marshal.PtrToStringUni(new IntPtr(_address.ToInt64() + _traceEventInfo.EventMessageOffset));
                return msg;
            }
        }

        internal string GetTask()
        {
            if (_traceEventInfo.TaskNameOffset == 0)
            {
                return string.Empty;
            }
            else
            {
                string msg = Marshal.PtrToStringUni(new IntPtr(_address.ToInt64() + _traceEventInfo.TaskNameOffset));
                return msg;
            }
        }

        internal string GetLevelName()
        {
            if (_traceEventInfo.LevelNameOffset == 0)
            {
                return string.Empty;
            }
            else
            {
                string msg = Marshal.PtrToStringUni(new IntPtr(_address.ToInt64() + _traceEventInfo.LevelNameOffset));
                return msg;
            }
        }

        public void Dispose()
        {
            ReleaseMemory();
            GC.SuppressFinalize(this);
        }

        internal IList<PropertyItem> GetProperties(EventRecord eventRecord)
        {
            // We only support top level properties and simple types
            IList<PropertyItem> properties = new List<PropertyItem>();

            if (this._hasProperties)
            {

                int offset = 0;

                for (int i = 0; i < _traceEventInfo.TopLevelPropertyCount; i++)
                {
                    EventPropertyInfo info = _eventPropertyInfoArray[i];

                    // Read the current property name
                    string propertyName = Marshal.PtrToStringUni(new IntPtr(_address.ToInt64() + info.NameOffset));

                    object value;
                    string mapName;
                    int length;

                    if (eventRecord.UserData == IntPtr.Zero)
                    {
                        mapName = string.Empty;
                        value = string.Empty;
                        length = info.LengthPropertyIndex;
                    }
                    else
                    {
                        IntPtr dataPtr = new IntPtr(eventRecord.UserData.ToInt64() + offset);
                        value = ReadPropertyValue(info, dataPtr, out mapName, out length);
                    }

                    // If we have a map name, return both map name and map value as a pair.
                    if (!string.IsNullOrEmpty(mapName))
                    {
                        value = new KeyValuePair<string, object>(mapName, value);
                    }

                    offset += length;
                    properties.Add(new PropertyItem(propertyName, value));
                }

                if (offset < eventRecord.UserDataLength)
                {
                    //
                    // There is some extra information not mapped.
                    //
                    IntPtr dataPtr = new IntPtr(eventRecord.UserData.ToInt64() + offset);
                    int length = eventRecord.UserDataLength - offset;
                    byte[] array = new byte[length];

                    for (int index = 0; index < length; index++)
                    {
                        array[index] = Marshal.ReadByte(dataPtr, index);
                    }

                    properties.Add(new PropertyItem("__ExtraPayload", array));
                }
            }
            else
            {
                // NOTE: It is just a guess that this is an Unicode string
                string str = Marshal.PtrToStringUni(eventRecord.UserData);

                properties.Add(new PropertyItem("EventData", str));
            }

            return properties;
        }

        private void Initialize(EventRecord eventRecord)
        {
            int size = 0;
            const uint BufferTooSmall = 122;
            const uint ElementNotFound = 1168;

            this._traceEventInfo = new TraceEventInfo();
            int error = NativeMethods.TdhGetEventInformation(ref eventRecord, 0, IntPtr.Zero, IntPtr.Zero, ref size);
            if (error == ElementNotFound)
            {
                // Nothing else to do here.
                this._hasProperties = false;
                return;
            }
            this._hasProperties = true;

            if (error != BufferTooSmall)
            {
                throw new System.ComponentModel.Win32Exception(error);
            }

            // Get the event information (schema)
            this._address = Marshal.AllocHGlobal(size);
            error = NativeMethods.TdhGetEventInformation(ref eventRecord, 0, IntPtr.Zero, _address, ref size);
            if (error != 0)
            {
                throw new System.ComponentModel.Win32Exception(error);
            }

            // Marshal the first part of the trace event information.
            Marshal.PtrToStructure(this._address, _traceEventInfo);

            // Marshal the second part of the trace event information, the array of property info.
            int actualSize = Marshal.SizeOf(this._traceEventInfo);
            if (size != actualSize)
            {
                int structSize = Marshal.SizeOf(typeof(EventPropertyInfo));
                int itemsLeft = (size - actualSize) / structSize;

                this._eventPropertyInfoArray = new EventPropertyInfo[itemsLeft];
                long baseAddress = _address.ToInt64() + actualSize;
                for (int i = 0; i < itemsLeft; i++)
                {
                    IntPtr structPtr = new IntPtr(baseAddress + (i * structSize));
                    EventPropertyInfo info = new EventPropertyInfo();
                    Marshal.PtrToStructure(structPtr, info);
                    this._eventPropertyInfoArray[i] = info;
                }
            }

            // Get the opcode name
            if (_traceEventInfo.OpcodeNameOffset > 0)
            {
                this.EventName = Marshal.PtrToStringUni(new IntPtr(_address.ToInt64() + _traceEventInfo.OpcodeNameOffset));
            }
        }

        private object ReadPropertyValue(EventPropertyInfo info, IntPtr dataPtr, out string mapName, out int length)
        {

            length = info.LengthPropertyIndex;

            if (info.NonStructTypeValue.MapNameOffset != 0)
            {
                mapName = Marshal.PtrToStringUni(new IntPtr(this._address.ToInt64() + info.NonStructTypeValue.MapNameOffset));
            }
            else
            {
                mapName = string.Empty;
            }

            switch (info.NonStructTypeValue.InType)
            {
                case TdhInType.Null:
                    break;
                case TdhInType.UnicodeString:
                    {
                        string str = Marshal.PtrToStringUni(dataPtr);
                        length = (str.Length + 1) * sizeof(char);
                        return str;
                    }
                case TdhInType.AnsiString:
                    {
                        string str = Marshal.PtrToStringAnsi(dataPtr);
                        length = (str.Length + 1);
                        return str;
                    }
                case TdhInType.Int8:
                    return (sbyte)Marshal.ReadByte(dataPtr);
                case TdhInType.UInt8:
                    return Marshal.ReadByte(dataPtr);
                case TdhInType.Int16:
                    return Marshal.ReadInt16(dataPtr);
                case TdhInType.UInt16:
                    return (uint)Marshal.ReadInt16(dataPtr);
                case TdhInType.Int32:
                    return Marshal.ReadInt32(dataPtr);
                case TdhInType.UInt32:
                    return (uint)Marshal.ReadInt32(dataPtr);
                case TdhInType.Int64:
                    return Marshal.ReadInt64(dataPtr);
                case TdhInType.UInt64:
                    return (ulong)Marshal.ReadInt64(dataPtr);
                case TdhInType.Float:
                    return BitConverter.ToSingle(BitConverter.GetBytes(Marshal.ReadInt32(dataPtr)), 0);
                case TdhInType.Double:
                    return BitConverter.ToDouble(BitConverter.GetBytes(Marshal.ReadInt64(dataPtr)), 0);
                case TdhInType.Boolean:
                    return (bool)(Marshal.ReadInt32(dataPtr) != 0);
                case TdhInType.Binary:
                    break;
                case TdhInType.Guid:
                    return new Guid(
                           Marshal.ReadInt32(dataPtr),
                           Marshal.ReadInt16(dataPtr, 4),
                           Marshal.ReadInt16(dataPtr, 6),
                           Marshal.ReadByte(dataPtr, 8),
                           Marshal.ReadByte(dataPtr, 9),
                           Marshal.ReadByte(dataPtr, 10),
                           Marshal.ReadByte(dataPtr, 11),
                           Marshal.ReadByte(dataPtr, 12),
                           Marshal.ReadByte(dataPtr, 13),
                           Marshal.ReadByte(dataPtr, 14),
                           Marshal.ReadByte(dataPtr, 15)
                           );
                case TdhInType.Pointer:
                    return Marshal.ReadIntPtr(dataPtr);
                case TdhInType.FileTime:
                    return Marshal.ReadInt64(dataPtr);
                case TdhInType.SystemTime:
                    {
                        SYSTEMTIME st = new SYSTEMTIME();
                        Marshal.PtrToStructure(dataPtr, st);
                        return st;
                    }
                case TdhInType.SID:
                    //TODO: use ConvertSidToStringSid
                    return "SID";
                case TdhInType.HexInt32:
                    break;
                case TdhInType.HexInt64:
                    break;
                case TdhInType.CountedString:
                    break;
                case TdhInType.CountedAnsiString:
                    break;
                case TdhInType.ReversedCountedString:
                    break;
                case TdhInType.ReversedCountedAnsiString:
                    break;
                case TdhInType.NonNullTerminatedString:
                    break;
                case TdhInType.NonNullTerminatedAnsiString:
                    break;
                case TdhInType.UnicodeChar:
                    break;
                case TdhInType.AnsiChar:
                    break;
                case TdhInType.SizeT:
                    break;
                case TdhInType.HexDump:
                    break;
                case TdhInType.WbemSID:
                    break;
                default:
                    break;
            }

            Debugger.Break();
            return null;
        }

        private void ReleaseMemory()
        {
            if (this._address != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(this._address);
                this._address = IntPtr.Zero;
            }
        }


        internal string FormatMessage(string message, params string[] args)
        {
            if (string.IsNullOrEmpty(message))
            {
                // if for some reason there is no format string but we have parameters, just output parameters as a comma separated string
                StringBuilder sb = new StringBuilder();
                if (args != null)
                {
                    sb.Append("No format message. Parameters: ");
                    sb.AppendLine(string.Join(", ", args));                    
                }
                return sb.ToString();
            }
            else
            {
                IntPtr msgPtr = Marshal.StringToHGlobalUni(message);
                IntPtr lpMsgBuf = IntPtr.Zero;
                try
                {
                    uint dwChars = NativeMethods.FormatMessage(
                         NativeMethods.FORMAT_MESSAGE_ALLOCATE_BUFFER | NativeMethods.FORMAT_MESSAGE_FROM_STRING | NativeMethods.FORMAT_MESSAGE_ARGUMENT_ARRAY,
                         msgPtr,
                         0,  // ignored since dwFlags include FORMAT_MESSAGE_FROM_STRING
                         0,  // ignored since dwFlags include FORMAT_MESSAGE_FROM_STRING
                         ref lpMsgBuf,
                         0,
                         args);

                    string sRet = Marshal.PtrToStringUni(lpMsgBuf);
                    return sRet;
                }
                finally
                {
                    Marshal.FreeHGlobal(lpMsgBuf);
                    Marshal.FreeHGlobal(msgPtr);
                }
            }
        }

    }
}