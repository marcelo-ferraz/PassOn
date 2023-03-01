using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassOn.Tests.Models
{
    public class DifferentClass
    {
        public class DifferentSubClass
        {
            public int Value { get; set; }
        }

        public string String { get; set; }
        public DateTime Data { get; set; }

        public List<int> Numbers { get; set; }

        public List<DifferentSubClass> List { get; set; }
        public DifferentSubClass[] List2Array { get; set; }
        public DifferentSubClass[] Array { get; set; }
        public List<DifferentSubClass> Array2List { get; set; }
    }
}
