using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Esports.space
{
    public static class DVCUID
    {
        static int uuidCount = 10500;
        static Dictionary<string, string> deviceUUIDDict = new Dictionary<string, string>();
        public static string DeviceIDToUUID(string deviceID)
        {
            if (!deviceUUIDDict.ContainsKey(deviceID))
            {
                deviceUUIDDict.Add(deviceID, uuidCount++.ToString());
            }
            return deviceUUIDDict[deviceID];
        }
    }
}