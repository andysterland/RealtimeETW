using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Diagnostics.Eventing.EventLib;
using System.Diagnostics;

namespace OM
{
    public static class Controller
    {
        public static event EventHandler<EtwEventArgs> OnEtwEvent;
        public static event EventHandler<EtwEventCounterArgs> OnEtwEventCounter;
        private static EtwSession etwSession;
        private static EventTraceWatcher etwWatcher;
        private static string etwSessionName;
        private static Settings settings;
        private static int etwTotalEventCounter = 0;
        private static int etwFilteredEventCounter = 0;
        private static int processId = 0;

        static Controller()
        {
            //etwSessionName = "Log"+Guid.NewGuid().ToString("N").Substring(0, 8);
            etwSessionName = "LogViewerTraceSession";
        }

        public static List<KeyValuePair<int, String>> GetProcesses()
        {
            Process[] runningProcesses = Process.GetProcesses();
            List<KeyValuePair<int, String>> validProcesses = new List<KeyValuePair<int, string>>();

            foreach (Process p in runningProcesses)
            {
                String title = (p.ProcessName.ToLower() == "iexplore" && String.IsNullOrEmpty(p.MainWindowTitle)) ? "(IE tab process)" : p.MainWindowTitle;
                String name = String.Format("{0} ({1} - {2})", title, p.ProcessName, p.Id);
                validProcesses.Add(new KeyValuePair<int, String>(p.Id, name));
            }

            return validProcesses;
        }

        public static void LoadSettings(string FileName)
        {
            settings = Settings.LoadFromFile(FileName);
        }

        public static void SaveSettings(string FileName, List<EtwEventInfo> Events)
        {
            settings = Settings.LoadFromFile(FileName);
        }

        public static List<EtwEventInfo> GetSettings()
        {
            return settings.EtwEvents;
        }
        
        public static void Start()
        {
            Start(0);
        }

        public static void Start(int ProcessID)
        {
            if (    settings == null 
                ||  settings.EtwEvents == null 
                ||  settings.EtwEvents.Count == 0)
            {
                throw new Exception("No settings loaded.");
            }

            EtwSessionSetting etwSessionSettings = new EtwSessionSetting();
            etwSessionSettings.RealTimeSession = true;
            etwSessionSettings.Name = etwSessionName;
            etwSessionSettings.BufferSize = 64;

            etwSession = new EtwSession(etwSessionSettings);
            etwSession.StartTrace();

            List<Guid> guids = new List<Guid>();
            List<Guid> providerGuids = new List<Guid>();

            foreach (EtwEventInfo etwEventInfo in settings.EtwEvents)
            {
                if (!providerGuids.Contains(etwEventInfo.ProviderGuid))
                {
                    providerGuids.Add(etwEventInfo.ProviderGuid);
                    etwSession.EnableProvider(etwEventInfo.ProviderGuid);
                }
            }
            etwWatcher = new EventTraceWatcher(etwSessionName);
            etwWatcher.EventArrived += new EventHandler<EventArrivedEventArgs>(etwWatcher_EventArrived);
            etwWatcher.Enabled = true;

            etwTotalEventCounter = 0;
            etwFilteredEventCounter = 0;
            processId = ProcessID;
        }
        
        public static void Stop()
        {
            Action action = StopFunc;
            action.BeginInvoke((ar) => { action.EndInvoke(ar); }, null);
        }

        private static void StopFunc()
        {
            if (etwWatcher != null)
            {
                etwWatcher.Dispose();
            }
            if (etwSession != null)
            {
                etwSession.StopTrace();
            }

            etwSession = null;
            etwWatcher = null;
        }

        private static void etwWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            string eventName = (String.IsNullOrEmpty(e.EventName)) ? "" : e.EventName.Trim();
            string eventTask = (String.IsNullOrEmpty(e.Task)) ? "" : e.Task.Trim();
            if (     (processId == 0 || processId == e.ProcessId)
                &&   eventName.Equals("info", StringComparison.InvariantCultureIgnoreCase))
            {
                //Debug.WriteLine("Task: " + e.Task + " Delta: " + (DateTime.Now - e.LogDateTime).TotalMilliseconds);
                if (OnEtwEventCounter != null)
                {
                    etwTotalEventCounter++;
                    OnEtwEventCounter.Invoke(null, new EtwEventCounterArgs(etwTotalEventCounter, etwFilteredEventCounter, e.LogDateTime));
                }

                foreach (EtwEventInfo etw in settings.EtwEvents)
                {
                    if (eventTask.Equals(etw.Task, StringComparison.InvariantCultureIgnoreCase))
                    {
                        etwFilteredEventCounter++;
                        if (OnEtwEvent != null)
                        {
                            EtwEvent etwEvent = etw.GetEtwEvent(e.ProcessId, etw.Task, (List<PropertyItem>)e.Properties, e.LogDateTime);
                            OnEtwEvent.Invoke(null, new EtwEventArgs(etwEvent));
                            continue;
                        }
                    }
                }
            }
        }
    }
}
