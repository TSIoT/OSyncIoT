using System;
using System.Collections.Generic;
using System.Text;

using System.IO; //file IO controller
//using IOTProtocol.JsonUtility;
using IoTRepublicPlus.Utility;
using Newtonsoft.Json.Linq;

namespace MobileShareLibrary
{
    public static class OSyncUtility
    {
        //private static JsonUtility jsonUtility = new JsonUtility();
        //private static JsFileParser jsFileParser=new JsFileParser();
        private static string selfDescriptionPath = "SelfDeviceDescription.conf";
        private static string selfDescription = "";


        public static string GetDeviceID()
        {
            string uniqueID = System.Guid.NewGuid().ToString();
            return uniqueID;
        }

        public static string GetSelfFileDescription()
        {
            selfDescriptionCheck();
            return selfDescription;
        }
        
        public static void SetDescriptionValue(ref string jsText, string attribute, string value)
        {
            JObject obj = JsonUtility.LoadJsonData(jsText);
            JsonUtility.SetValueInFirstObject(obj, attribute, value);
            jsText = obj.ToString(Newtonsoft.Json.Formatting.None);
        }
        
      
        /**
         * check if the self description is exists, if not, cread one
         **/
        private static void selfDescriptionCheck()
        {
            string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filePath = Path.Combine(docPath, selfDescriptionPath);
            string description = "";
            //Console.WriteLine("File path:" + filePath);

            if (File.Exists(filePath))
            {
              //Console.WriteLine("Found file");
                description = File.ReadAllText(filePath);
            }
            else
            {
                Console.WriteLine("Cannot found file, create a new one");
                description = "{\"IOTDEV\":{\"DeviceName\":\"MobileUI\",\"FunctionGroup\":\"UI\",\"DeviceID\":\"empty\",\"Component\":[]}}";                
                string deviceId = OSyncUtility.GetDeviceID();
                SetDescriptionValue(ref description, "DeviceID", deviceId);

                File.WriteAllText(filePath, description);
            }

            selfDescription = description;
            //Console.WriteLine(selfDescription);
        }


    }
}
