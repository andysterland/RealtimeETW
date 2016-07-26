using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Diagnostics.Eventing.EventLib.Interop;

namespace Microsoft.Diagnostics.Eventing.EventLib
{
    /// <summary>
    /// Represent an manifest-based provider
    /// </summary>
    public class EventProvider
    {
        public EventProvider(Guid guid, string name)
        {
            ProviderGuid = guid;
            Name = name;
        }

        public Guid ProviderGuid
        {
            internal set;
            get;
        }

        public string Name
        {
            internal set;
            get;
        }

        /// <summary>
        /// Returns all the manifest-based providers
        /// </summary>
        /// <returns></returns>
        public static IList<EventProvider> GetAllProviders()
        {
            List<EventProvider> providerList = new List<EventProvider>();
            IntPtr buffer = IntPtr.Zero;

            try
            {
                // get the required buffer size
                uint bufferSize = 1;
                buffer = Marshal.AllocHGlobal(1);
                uint result = NativeMethods.TdhEnumerateProviders(buffer, ref bufferSize);

                do
                {
                    // call it again with correct size
                    Marshal.FreeHGlobal(buffer);
                    buffer = Marshal.AllocHGlobal((int)bufferSize);
                    result = NativeMethods.TdhEnumerateProviders(buffer, ref bufferSize);
                }
                while (result == NativeMethods.ERROR_INSUFFICIENT_BUFFER);

                PROVIDER_ENUMERATION_INFO pei = new PROVIDER_ENUMERATION_INFO();
                int numpro = Marshal.ReadInt32(buffer);
                pei.NumberOfProviders = (uint)numpro;

                for (int i = 0; i < pei.NumberOfProviders; i++)
                {
                    // 8 is the offset of first two fields of PROVIDER_ENUMERATION_INFO
                    // since TRACE_PROVIDER_INFO is in an array, we use PtrToStructure to marshal it back to TRACE_PROVIDER_INFO
                    TRACE_PROVIDER_INFO tpi = (TRACE_PROVIDER_INFO)Marshal.PtrToStructure((IntPtr)(buffer.ToInt64() + 8 + Marshal.SizeOf(typeof(TRACE_PROVIDER_INFO)) * i), typeof(TRACE_PROVIDER_INFO));
                    if (tpi.SchemaSource == 0)  // 0 means manifest based
                    {
                        IntPtr nameoffset = (IntPtr)(buffer.ToInt64() + tpi.ProviderNameOffset);
                        string s = Marshal.PtrToStringUni(nameoffset);
                        providerList.Add(new EventProvider(tpi.ProviderGuid, s));
                    }
                }

            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
            return providerList;
        }

    }
}
