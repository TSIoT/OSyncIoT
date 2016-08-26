using System;
using System.Collections.Generic;
using System.Text;


//using IOTProtocol.JsonUtility;
using IoTRepublicPlus.Utility;
using Newtonsoft.Json.Linq;

namespace MobileShareLibrary
{
    public class IoTDevice
    {
        public string IoTIP { get; set; }
        public string DeviceID { get; private set; }
        public string DeviceName { get; private set; }
        public string FunctionGroup { get; private set; }
        public List<IoTComponent> Component { get; private set; }

        private string descriptionText { get; set; }

        public IoTDevice()
        {
            this.Component = new List<IoTComponent>();
        }

        public void SetJSDescription(string desc)
        {
            JObject jsObj = JsonUtility.LoadJsonData(desc);
            this.DeviceID = JsonUtility.GetValueInFirstObject(jsObj, "DeviceID");
            this.DeviceName = JsonUtility.GetValueInFirstObject(jsObj, "DeviceName");
            this.FunctionGroup = JsonUtility.GetValueInFirstObject(jsObj, "FunctionGroup");
            string arrayName = "Component";
            int comCount = JsonUtility.GetArrayLengthInFirstObject(jsObj, arrayName);
            IoTComponent com;
            for (int i = 0; i < comCount; i++)
            {
                com = new IoTComponent();
                com.ID = JsonUtility.GetArrayValueInFirstObject(jsObj, arrayName, i, "ID");
                com.Name = JsonUtility.GetArrayValueInFirstObject(jsObj, arrayName, i, "Name");
                com.Group = JsonUtility.GetArrayValueInFirstObject(jsObj, arrayName, i, "Group");
                string type = JsonUtility.GetArrayValueInFirstObject(jsObj, arrayName, i, "Type");

                if (type == "Button")
                {
                    com.Type = IoTComponent.ComType.Button;
                }
                else if (type == "Content")
                {
                    com.Type = IoTComponent.ComType.Content;
                }
                else
                {
                    com.Type = IoTComponent.ComType.Unknown;
                }

                if(com.Type==IoTComponent.ComType.Content)
                {                    
                    string str_rdFrequency = JsonUtility.GetArrayValueInFirstObject(jsObj, arrayName, i, "ReadFrequency");
                    com.ReadFrequency = int.Parse(str_rdFrequency);
                }          
                this.Component.Add(com);
            }            
        }

        public string GetDescription()
        {
            return this.descriptionText;
        }

    }
}

