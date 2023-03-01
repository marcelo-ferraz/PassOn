using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassOn.Tests.Models
{
    public class InheritedClass : BaseClass
    {
        [MapStrategy(Aliases = new[] { "Data" })]
        public DateTime Date { get; set; }

    }
}
