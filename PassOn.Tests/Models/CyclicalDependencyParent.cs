using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassOn.Tests.Models
{
    internal class CyclicalDependencyParent
    {
        public int Id { get; set; }
        public CyclicalDependencyChild Child { get; set; }
    }
    internal class CyclicalDependencyChild
    {
        public int Id { get; set; }
        public CyclicalDependencyParent Parent { get; set; }
    }
}
