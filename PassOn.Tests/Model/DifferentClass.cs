using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassOn.Tests.Model
{
    public class DifferentClass
    {
        public static string ToStr(DateTime o)
        {
            return o.ToString("dd/MM/yyyy");
        }

        public static Func<DateTime, string> ToStr2 
        {
            get 
            {
                return o => o.ToString("dd/MM/yyyy");
            }
        }

        public class DifferentSubClass
        {
            public int Value { get; set; }
        }
                
        public string String { get; set; }

        //[Clone(Inspection.Shallow, DifferentClass.ToStr2)]
        //[Clone(Inspection.Custom, (Func<DateTime, string>) DifferentClass.ToStr)] 
        public DateTime Data { get; set; }

        public List<int> Numbers { get; set; }

        public List<DifferentSubClass> List { get; set; }
                
        public DifferentSubClass[] List2Array { get; set; }
        public DifferentSubClass[] Array { get; set; }
        public List<DifferentSubClass> Array2List { get; set; }
    }
}
