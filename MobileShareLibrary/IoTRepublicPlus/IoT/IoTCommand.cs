using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json.Linq;
using IoTRepublicPlus.Utility;

namespace IoTRepublicPlus
{
    public class IoTCommand
    {
        public enum Type
        {
            None =0,
            ReadRequest = 1,
            ReadResponse = 2,
            Write = 3,
            Management = 4,
        };

        public Type CmdType;
        public string ID;
        public string Value;
        public string sendedData;

        private string rootName = "IOTCMD";

        public IoTCommand(Type cmdType,string id, string value)
        {
            this.ID = id;
            this.Value = value;
            this.CmdType = cmdType;

            string templete = "{\"IOTCMD\":{\"Type\":\"None\",\"ID\":\"0\",\"Value\":\"0\"}}";
            JObject root;
            root = JsonUtility.LoadJsonData(templete);
            if (root!=null)
            {
                string typeStr = "";
                switch (this.CmdType)
                {
                    case Type.None:
                        typeStr = "None";
                        break;

                    case Type.ReadRequest:
                        typeStr = "Req";
                        break;

                    case Type.ReadResponse:
                        typeStr = "Res";
                        break;

                    case Type.Write:
                        typeStr = "Wri";
                        break;

                    case Type.Management:
                        typeStr = "Man";
                        break;

                    default:
                        break;
                }
                //cout << "Packect" << endl;
                JsonUtility.SetValueInFirstObject(root, "Type", typeStr);
                JsonUtility.SetValueInFirstObject(root, "ID", this.ID);
                JsonUtility.SetValueInFirstObject(root, "Value", this.Value);
                
                sendedData=root.ToString(Newtonsoft.Json.Formatting.None);                
            }
        }

        public IoTCommand(string content)
        {
            JObject root;
            root = JsonUtility.LoadJsonData(content);
            string jsonRootName = JsonUtility.GetFirstKeyName(root);

            if (jsonRootName == this.rootName)
            {
                this.ID = JsonUtility.GetValueInFirstObject(root, "ID");
                this.Value = JsonUtility.GetValueInFirstObject(root, "Value");
                string type = JsonUtility.GetValueInFirstObject(root, "Type");
                if (type == "Req")
                {
                    this.CmdType = Type.ReadRequest;
                }
                else if (type == "Res")
                {
                    this.CmdType = Type.ReadResponse;
                }
                else if (type == "Wri")
                {
                    this.CmdType = Type.Write;
                }
                else if (type == "Man")
                {
                    this.CmdType = Type.Management;
                }
            }
            sendedData = content;
        }
    }
}
