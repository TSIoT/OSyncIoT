using System;
using System.Collections.Generic;
using System.Text;

namespace IoTRepublicPlus
{
    public class IoTPackage
    {
        public const char SegmentSymbol='\0';

        public bool IsCompletedPackage { get; set; }
        public string Ver { get; set; }
        public int HeaderLength { get; set; }
        public int DataLength { get; set; }
        public string SorIp { get; set; }
        public string DesIp { get; set; }
        public uint Checksum { get; set; }

        public List<char> DataVector = null;        
        public byte[] DataVectorForSending = null;

        private List<char> SendingDataList = null;

        public IoTPackage()
        {
            this.IsCompletedPackage = false;
            this.Ver = "";
            this.HeaderLength = 0;
            this.DataLength = 0;
            this.SorIp = "";
            this.DesIp = "";
            this.Checksum = 0;
        }

        public IoTPackage(string ver, string soIp, string desIp)
        {
            int headerLength = 0;
            StringBuilder tempStream = new StringBuilder();
            //stringstream tempStream;

            this.IsCompletedPackage = false;
            this.Ver = ver;
            this.SorIp = soIp;
            this.DesIp = desIp;

            headerLength += this.Ver.Length + 1; //1 is for segment symbol
            headerLength += this.SorIp.Length + 1; //1 is for segment symbol
            headerLength += this.DesIp.Length + 1; //1 is for segment symbol

            this.DataLength = 0;           

            //tempStream << this.DataLength;
            tempStream.Append(this.DataLength.ToString());
            //cout << "DataLength:" << tempStream.str() << endl;
            headerLength += tempStream.Length + 1;//1 is for segment symbol

            if (headerLength <= 8)
            {
                headerLength += 1;
            }
            else if (headerLength <= 97)
            {
                headerLength += 2;
            }
            else
            {
                headerLength += 3;
            }
            headerLength++; //for segment symbol

            this.HeaderLength = headerLength;
           
            this.PacketToSendingVector();
        }

        public IoTPackage(string ver, string soIp, string desIp, List<char> data)
        {
            int headerLength = 0;
            StringBuilder tempStream=new StringBuilder();
            //stringstream tempStream;

            this.IsCompletedPackage = false;
            this.Ver = ver;
            this.SorIp = soIp;
            this.DesIp = desIp;

            headerLength += this.Ver.Length + 1; //1 is for segment symbol
            headerLength += this.SorIp.Length + 1; //1 is for segment symbol
            headerLength += this.DesIp.Length + 1; //1 is for segment symbol

            if (data == null)
            {
                this.DataLength = 0;
            }
            else
            {
                this.DataLength = data.Count;
            }

            //tempStream << this.DataLength;
            tempStream.Append(this.DataLength.ToString());
            //cout << "DataLength:" << tempStream.str() << endl;
            headerLength += tempStream.Length + 1;//1 is for segment symbol

            if (headerLength <= 8)
            {
                headerLength += 1;
            }
            else if (headerLength <= 97)
            {
                headerLength += 2;
            }
            else
            {
                headerLength += 3;
            }
            headerLength++; //for segment symbol

            this.HeaderLength = headerLength;

            if (data != null)
            {
                this.DataLength = data.Count;
                this.DataVector = new List<char>(data);
            }

            this.PacketToSendingVector();
        }

        public IoTPackage(string ver, string soIp, string desIp, string data)
        {
            int headerLength = 0;
            StringBuilder tempStream = new StringBuilder();
            //stringstream tempStream;

            this.IsCompletedPackage = false;
            this.Ver = ver;
            this.SorIp = soIp;
            this.DesIp = desIp;

            headerLength += this.Ver.Length + 1; //1 is for segment symbol
            headerLength += this.SorIp.Length + 1; //1 is for segment symbol
            headerLength += this.DesIp.Length + 1; //1 is for segment symbol

            if (data == null)
            {
                this.DataLength = 0;
            }
            else
            {
                this.DataLength = data.Length;
            }

            //tempStream << this.DataLength;
            tempStream.Append(this.DataLength.ToString());
            //cout << "DataLength:" << tempStream.str() << endl;
            headerLength += tempStream.Length + 1;//1 is for segment symbol

            if (headerLength <= 8)
            {
                headerLength += 1;
            }
            else if (headerLength <= 97)
            {
                headerLength += 2;
            }
            else
            {
                headerLength += 3;
            }
            headerLength++; //for segment symbol

            this.HeaderLength = headerLength;

            if (data != null)
            {
                this.DataLength = data.Length;
                this.DataVector = new List<char>(data);
            }

            this.PacketToSendingVector();
        }

        public IoTPackage(string ver, string soIp, string desIp, char[] data)
        {
            int headerLength = 0;
            StringBuilder tempStream = new StringBuilder();
            //stringstream tempStream;

            this.IsCompletedPackage = false;
            this.Ver = ver;
            this.SorIp = soIp;
            this.DesIp = desIp;

            headerLength += this.Ver.Length + 1; //1 is for segment symbol
            headerLength += this.SorIp.Length + 1; //1 is for segment symbol
            headerLength += this.DesIp.Length + 1; //1 is for segment symbol

            if (data == null)
            {
                this.DataLength = 0;
            }
            else
            {
                this.DataLength = data.Length;
            }

            //tempStream << this.DataLength;
            tempStream.Append(this.DataLength.ToString());
            //cout << "DataLength:" << tempStream.str() << endl;
            headerLength += tempStream.Length + 1;//1 is for segment symbol

            if (headerLength <= 8)
            {
                headerLength += 1;
            }
            else if (headerLength <= 97)
            {
                headerLength += 2;
            }
            else
            {
                headerLength += 3;
            }
            headerLength++; //for segment symbol

            this.HeaderLength = headerLength;

            if (data != null)
            {
                this.DataLength = data.Length;
                this.DataVector = new List<char>(data);
            }

            this.PacketToSendingVector();
        }

        public void PacketToSendingVector()
        {
            int i = 0;
            uint sum = 0;
            //stringstream tempStream;
            StringBuilder tempStream=new StringBuilder();

            this.SendingDataList = new List<char>(this.HeaderLength + this.DataLength + 2);
            this.DataVectorForSending = new byte[this.HeaderLength + this.DataLength + 2];

            //version
            for (i = 0; i < (int)this.Ver.Length; i++)
            {              
                this.SendingDataList.Add(this.Ver[i]);
            }
            this.SendingDataList.Add(SegmentSymbol);


            //header length
            tempStream.Clear();
            tempStream.Append(this.HeaderLength.ToString());
            //cout << "Header length:" << tempStream.str() << endl;;

            for (i = 0; i < tempStream.Length; i++)
            {                
                //this.DataVectorForSending.push_back(tempStream.str().at(i));
                this.SendingDataList.Add(tempStream[i]);
            }
            this.SendingDataList.Add(SegmentSymbol);


            //data length
            tempStream.Clear(); //clear tempStream
            tempStream.Append(this.DataLength.ToString());
            //cout << "Header length:" << tempStream.str() << endl;;
            for (i = 0; i < (int)tempStream.Length; i++)
            {                
                //this.DataVectorForSending.push_back(tempStream.str().at(i));
                this.SendingDataList.Add(tempStream[i]);
            }
            this.SendingDataList.Add(SegmentSymbol);
            //this.DataForSending[offset] = SegmentSymbol;


            //source ip            
            for (i = 0; i < (int)this.SorIp.Length; i++)
            {
                //this.DataForSending[offset] = this.SorIp.at(i);
                this.SendingDataList.Add(this.SorIp[i]);
            }
            this.SendingDataList.Add(SegmentSymbol);
            //this.DataForSending[offset] = SegmentSymbol;


            //destination ip            
            for (i = 0; i < (int)this.DesIp.Length; i++)
            {
                //this.DataForSending[offset] = this.DesIp.at(i);
                //this.DataVectorForSending.push_back(this.DesIp.at(i));
                this.SendingDataList.Add(this.DesIp[i]);
            }
            this.SendingDataList.Add(SegmentSymbol);            
            //this.DataForSending[offset] = SegmentSymbol;
            //offset++;


            //data
            for (i = 0; i < this.DataLength; i++)
            {
                //this.DataForSending[offset] = this.Data[i];
                //this.DataVectorForSending.push_back(this.DataVector.at(i));
                this.SendingDataList.Add(this.DataVector[i]);
            }
            
            //checksum
            for (i = 0; i < (int)this.SendingDataList.Count; i++)
            {
                this.DataVectorForSending[i] = (byte)this.SendingDataList[i];
                sum += this.DataVectorForSending[i];
            }

		    while ((sum >> 16) > 0)
		    {
			    sum = (sum & 0xFFFF) + (sum >> 16);
		    }

            this.DataVectorForSending[this.DataVectorForSending.Length-2]= (byte)(sum >> 8);
            this.DataVectorForSending[this.DataVectorForSending.Length - 1] = (byte)sum;

		    this.Checksum = sum;
    }

    }
}
