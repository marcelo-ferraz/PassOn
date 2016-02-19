using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassOn.Tests.Model
{
    public class BaseClass
    {
        public class SubClass
        {
            public int Value { get; set; }
        }

        public int Int { get; set; }
        public string String { get; set; }
        [Clone(Inspection.Shallow)]
        public List<int> Numbers { get; set; }

        public List<SubClass> List { get; set; }
        public List<SubClass> List2Array { get; set; }
        public SubClass[] Array { get; set; }
        public SubClass[] Array2List { get; set; }
    }
}
