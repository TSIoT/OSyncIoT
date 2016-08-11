using System;
using System.Collections.Generic;
using System.Text;

namespace MobileShareLibrary
{
    public class IoTComponent
    {
        public enum ComType
        {
            Unknown,
            Content,
            Button
        };

        public string ID { get; set; }
        public string Group { get; set; }
        public string Name { get; set; }
        public ComType Type { get; set; }        
        public int ReadFrequency { get; set; }
    }
}
