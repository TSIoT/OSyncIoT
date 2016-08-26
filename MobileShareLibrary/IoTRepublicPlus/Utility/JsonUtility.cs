using System;
using System.Collections.Generic;
using System.Text;

using System.Linq;
using Newtonsoft.Json.Linq;


namespace IoTRepublicPlus.Utility
{
    public class JsonUtility
    {
        public static JObject LoadJsonData(string text)
        {
            JObject jsonObj = null;
            try
            {
                jsonObj = JObject.Parse(text);
            }
            catch(Newtonsoft.Json.JsonReaderException ex)
            {
                jsonObj = null;
            }
            

            return jsonObj;
        }
                
        public static string GetFirstKeyName(JObject root)
        {
            string firstKeyName = "";
            if (root != null)
            {
                List<string> keys = root.Properties().Select(p => p.Name).ToList();
                firstKeyName = keys[0];
            }

            return firstKeyName;
        }

        public static string GetValueInFirstObject(JObject root, string keyName)
        {
            string value = "";
            string rootKeyName = GetFirstKeyName(root);
            if(rootKeyName.Length>0)
            {
                JObject firstObj = root[rootKeyName].ToObject<JObject>();
                if(firstObj.Property(keyName) !=null)
                    value=root[rootKeyName][keyName].ToString();
            }
            return value;
        }

        public static string SetValueInFirstObject(JObject root, string keyName,string value)
        {            
            string rootKeyName = GetFirstKeyName(root);
            if (rootKeyName.Length > 0)
            {
                JObject firstObj = root[rootKeyName].ToObject<JObject>();
                if (firstObj.Property(keyName) != null)
                    root[rootKeyName][keyName] = value;                    
            }
            return value;
        }

        public static string GetArrayValueInFirstObject(JObject root, string arrayName, int arrayIndex,string keyName)
        {
            string value = "";
            if(isArrayElementExists(root, arrayName, arrayIndex, keyName))
            {
                string rootKeyName = GetFirstKeyName(root);
                value= root[rootKeyName][arrayName][arrayIndex][keyName].ToString();                
            }
                                            
            return value;
        }

        public static int GetArrayLengthInFirstObject(JObject root, string arrayName)
        {
            int arrayLength = 0;

            try
            {
                string rootKeyName = GetFirstKeyName(root);
                arrayLength=root[rootKeyName][arrayName].ToObject<JArray>().Count;

            }
            catch(System.NullReferenceException)
            {
                Console.WriteLine("The array ["+ arrayName + "] is not exists");
            }

            return arrayLength;
        }

        private static bool isArrayElementExists(JObject root, string arrayName, int arrayIndex, string keyNam)
        {
            bool isExists = false;

            try
            {
                string rootKeyName = GetFirstKeyName(root);
                if (rootKeyName.Length > 0)
                {
                    string temp = root[rootKeyName][arrayName][arrayIndex][keyNam].ToString();
                    isExists = true;
                }
            }
            catch(System.NullReferenceException)
            {
                //Console.WriteLine(nullException.ToString());
                Console.WriteLine("The value of key is not exists");
                isExists = false;
            }
            catch(System.ArgumentOutOfRangeException)
            {
                //Console.WriteLine(outOfRangeException.ToString());
                Console.WriteLine("The index["+ arrayIndex + "] of array is out of range");
                isExists = false;
            }
                        
            return isExists;
        }

        private static bool IsLeaglJsonFile(string text)
        {
            JObject jsonObj = null;
            bool result = false;

            try
            {
                jsonObj = JObject.Parse(text);
                result = true;
            }
            catch (Newtonsoft.Json.JsonReaderException ex)
            {
                jsonObj = null;
                result = false;
            }


            return result;
        }
    }
}
