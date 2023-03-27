using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassOn.Tests.Models
{
    public class Inheritance {
        public class IntWrapper
        {
            public int Value { get; set; }
        }

        public class ComplexBase
        {
            public int Int { get; set; }
            public string? String { get; set; }
            public List<int>? Numbers { get; set; }
            public List<IntWrapper>? List { get; set; }

            public IntWrapper[]? Array { get; set; }
            [MapStrategy(Strategy.Shallow)]

            public List<IntWrapper>?  SecondArray { get; set; }
            public IntWrapper[]? SecondList { get; set; }
        }

        public class Simple : ComplexBase
        {
            [MapStrategy(Aliases = new[] { "Data" })]
            public DateTime Date { get; set; }
        }
    }    
}
