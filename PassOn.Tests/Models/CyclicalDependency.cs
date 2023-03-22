using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassOn.Tests.Models
{
    internal class CyclicalDependency {
        internal class Parent
        {
            public int Id { get; set; }
            public Child? Child { get; set; }
        }
        internal class Child
        {
            public int Id { get; set; }
            public Parent? Parent { get; set; }
        }
    }    
}
