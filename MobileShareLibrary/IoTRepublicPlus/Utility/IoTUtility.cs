using System;
using System.Collections.Generic;
using System.Text;

namespace IoTRepublicPlus
{
    public class IoTUtility
    {
        private string currentVersion;
        private char segmentSymbol;
        private int headerColumns = 5;

        public enum GetPackageError
        {
            NoError,
            UnknownError,
            lengthIsNotLongEnough,
            StartWordError,
            HeaderNotCompleted,
            PackageNotCompleted,
            ChecksumError,
        };

        public IoTUtility(string protocalVersion, char segmentSymbol)
        {
            this.currentVersion = protocalVersion;
            this.segmentSymbol = segmentSymbol;
        }

        public IoTPackage GetCompletedPackage(List<char> buffer,ref GetPackageError error)
        {
            IoTPackage package = null;
            bool isValid = this.isValidPackage(buffer,ref error);
            
            if(isValid)
            {
                package = this.decodePackage(buffer);                
                package.IsCompletedPackage = true;


                //copy the exists sended data to IoTPackage
                int totalLength = package.HeaderLength + package.DataLength + 2;//2 is for check sum
                package.DataVectorForSending = new byte[totalLength];

                for(int i=0;i< totalLength;i++)
                {
                    package.DataVectorForSending[i] =(byte) buffer[i];
                }

                //modify the buffer
                buffer.RemoveRange(0, totalLength);
                //buffer->erase(buffer->begin(), buffer->begin() + totalLength);
            }

            GC.Collect();

            return package;
        }
        /*
        public IoTPackage GetCompletedPackage(List<byte> buffer, ref GetPackageError error)
        {
            IoTPackage package = null;
            
            bool isValid = this.isValidPackage(buffer, ref error);

            if (isValid)
            {
                package = this.decodePackage(buffer);
                package.IsCompletedPackage = true;


                //copy the exists sended data to IoTPackage
                int totalLength = package.HeaderLength + package.DataLength + 2;//2 is for check sum
                package.DataVectorForSending = new byte[totalLength];

                for (int i = 0; i < totalLength; i++)
                {
                    package.DataVectorForSending[i] = (byte)buffer[i];
                }

                //modify the buffer
                buffer.RemoveRange(0, totalLength);
                //buffer->erase(buffer->begin(), buffer->begin() + totalLength);
            }

            GC.Collect();

            return package;
        }
        */
        public bool isValidPackage(List<char> buffer,ref GetPackageError error)
        {
            IoTPackage package = null;
            int totalLength = 0;
            uint sum = 0, sendedSum = 0;
            bool isValid =false;

            bool lengthIsLongEnough = false;
            bool startWordIsCorrect = false;
            bool headerIsCompleted = false;
            bool packageIsCompleted = false;
            bool checkSumNoError = false;

            //check if the length is long enough
            if (buffer.Count >= this.currentVersion.Length)
            {
                lengthIsLongEnough = true;
            }
            else
            {
                //cout << "header length is still too short" << endl;
                error = GetPackageError.lengthIsNotLongEnough;
            }


            if (lengthIsLongEnough)
            {
                //Check if the start word is correct
                string versionWord = new string(buffer.GetRange(0, this.currentVersion.Length).ToArray());                
                
                if (versionWord== this.currentVersion)
                {
                    startWordIsCorrect = true;
                }
                else
                {
                    Console.WriteLine("Start word is wrong:"+ versionWord);
                    
                    error = GetPackageError.StartWordError;
                }
                
            }
            
            if (lengthIsLongEnough && startWordIsCorrect)
            {
                //Check if the header is completed
                int totalClumnCount = 0;
                for (int i = 0; i < (int)buffer.Count; i++)
                {
                    if (buffer[i] == '\0')
                    {
                        totalClumnCount++;
                    }

                    if (totalClumnCount >= this.headerColumns)
                        break;
                }

                if (totalClumnCount >= this.headerColumns)
                {
                    headerIsCompleted = true;
                }
                else
                {
                    //cout << "Header is not completed" << endl;
                    error = GetPackageError.HeaderNotCompleted;
                }
            }
            

            
            if (lengthIsLongEnough && startWordIsCorrect && headerIsCompleted)
            {
                //check if the packaage is completed
                package = this.decodeOnlyHeader(buffer);
                if ((int)buffer.Count >= (package.HeaderLength + package.DataLength + 2))
                {
                    //cout << "The package is completed" << endl;
                    packageIsCompleted = true;
                }
                else
                {
                    //cout << "package is not completed" << endl;
                    error = GetPackageError.PackageNotCompleted;
                }
            }
            
            if (lengthIsLongEnough && startWordIsCorrect && headerIsCompleted && packageIsCompleted)
            {
               //check sum
                totalLength = package.HeaderLength + package.DataLength;
                for (int i = 0; i < totalLength; i++)
                {
                    sum += (byte)buffer[i];
                }

                while ((sum >> 16) > 0)
                {
                    sum = (sum & 0xFFFF) + (sum >> 16);
                }

                sendedSum = 0;
                sendedSum |= (byte)buffer[totalLength];
                sendedSum = (sendedSum << 8);
                sendedSum |= (byte)buffer[totalLength + 1];

                if (sum == sendedSum)
                {
                    checkSumNoError = true;

                }
                else
                {
                    Console.WriteLine("Check sum error!");
                    //if check sum error, need to clear the buffer
                    error = GetPackageError.ChecksumError;
                }
            }            


            if (lengthIsLongEnough && startWordIsCorrect &&
                headerIsCompleted && packageIsCompleted &&
                checkSumNoError)
            {
                //cout << "Received a vaild package!" << endl;;
                isValid = true;
                error = GetPackageError.NoError;

            }
            
            return isValid;
        }

        private IoTPackage decodePackage(List<char> buffer)
        {
            int totalLength = 0, i = 0, j = 0;

            IoTPackage package = this.decodeOnlyHeader(buffer);

            //copy data to package class
            totalLength = package.HeaderLength + package.DataLength;
            package.DataVector=new List<char>(package.DataLength);            
             
            for (i = package.HeaderLength; i < totalLength; i++, j++)
            {
                package.DataVector.Add(buffer[i]);
            }
            //Check sum
            package.Checksum = 0;
            package.Checksum |= (byte)buffer[totalLength];
            package.Checksum = package.Checksum << 8;
            package.Checksum |= (byte)buffer[totalLength + 1];
            
            return package;
        }

        private IoTPackage decodeOnlyHeader(List<char> buffer)
        {
            IoTPackage package= new IoTPackage();

            int startIndex = 0;
            string tempStr;
            //printAllChar(&buffer->at(0),buffer->size());
            //version
            package.Ver = this.popHeader(buffer, startIndex);
            startIndex += package.Ver.Length + 1; //+1 is for segement symbol

            //header length
            tempStr = this.popHeader(buffer, startIndex);
            package.HeaderLength = int.Parse(tempStr);

            startIndex += tempStr.Length + 1; //+1 is for segement symbol

            //data length
            tempStr = this.popHeader(buffer, startIndex);
            package.DataLength = int.Parse(tempStr);

            startIndex += tempStr.Length + 1; //+1 is for segement symbol

            //source IoT ip
            package.SorIp = this.popHeader(buffer, startIndex);
            startIndex += package.SorIp.Length + 1; //+1 is for segement symbol

            //destination IoT ip
            package.DesIp = this.popHeader(buffer, startIndex);
            return package;
        }

        private string popHeader(List<char> buffer, int startIndex)
        {
            StringBuilder tempBuffer=new StringBuilder();
            //stringstream tempBuffer;

            for (int i = startIndex; i < buffer.Count; i++)
            {
                if (buffer[i] != '\0')
                {
                    tempBuffer.Append(buffer[i]);
                    //tempBuffer << buffer->at(i);
                    //cout << buffer->at(i);
                }
                else
                {
                    break;
                }
            }
            //cout << endl;

            return tempBuffer.ToString();
        }
    }
}
