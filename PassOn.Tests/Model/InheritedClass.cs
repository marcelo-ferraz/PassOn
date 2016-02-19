using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassOn.Tests.Model
{
    public class InheritedClass : BaseClass
    {
        [Clone(Aliases = new[] { "Data" })]
        public DateTime Date { get; set; }

    }
}
