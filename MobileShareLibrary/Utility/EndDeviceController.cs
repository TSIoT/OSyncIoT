using System;
using System.Collections.Generic;
using System.Text;

//using IOTProtocol.Utility;
using IoTRepublicPlus;

namespace MobileShareLibrary.Utility
{
    public class EndDeviceController
    {
        private List<IoTDevice> allDeviceRoutingInfo;

        public EndDeviceController()
        {
            this.allDeviceRoutingInfo = new List<IoTDevice>();
        }

        public void SetDeviceInfo(IoTPackage package)
        {
            char[] rawData = package.DataVector.ToArray();
            int dataLength = package.DataVector.Count;
            this.allDeviceRoutingInfo.Clear();

            //Place the raw data into device info
            StringBuilder strBuilder = new StringBuilder(1024);
            int objCount = 0;

            IoTDevice dInfo = new IoTDevice();

            for (int i = 0; i < dataLength; i++)
            {
                if (rawData[i] == '\0')
                {
                    if (objCount % 2 == 0)
                    {
                        dInfo.IoTIP = strBuilder.ToString();
                    }
                    else if (objCount % 2 == 1)
                    {
                        dInfo.SetJSDescription(strBuilder.ToString());
                        //dInfo.Description = strBuilder.ToString().ToCharArray();
                        this.allDeviceRoutingInfo.Add(dInfo);
                        dInfo = new IoTDevice();
                    }

                    strBuilder.Remove(0, strBuilder.Length);
                    objCount++;
                }
                else
                {
                    strBuilder.Append(rawData[i]);
                }
            }
             
           
        }

        public List<IoTDevice> GetAllDeviceInfo()
        {
            return this.allDeviceRoutingInfo;
        }
    }
}
