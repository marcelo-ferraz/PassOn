using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassOn.Tests.Models
{
    public class ComplexClass
    {
        public class IntWrapper
        {
            public int Value { get; set; }
        }

        public string String { get; set; }
        public DateTime Data { get; set; }

        public List<int>? Numbers { get; set; }

        public List<IntWrapper>? List { get; set; }
        
        public IntWrapper[]? Array { get; set; }

        [MapStrategy("SecondList")]
        public IntWrapper[]? SecondArray { get; set; }

        [MapStrategy("SecondArray")]
        public List<IntWrapper>? SecondList { get; set; }
    }
}
