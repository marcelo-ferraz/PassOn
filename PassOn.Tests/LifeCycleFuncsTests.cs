//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace PassOn.Tests
//{
//    internal class LifeCycleFuncsTests
//    {
//        class Source
//        {
//            public Guid Id { get; set; }
//            public string? Text { get; set; }
//        }

//        class SourceWithBeforeAction : Source
//        {
//            [BeforeMapping]
//            public void Before(Source src, Target tgt)
//            {
//                System.Diagnostics.Debugger.Break();
//            }
//        }

//        class SourceWithBeforeActionNoArgs : Source
//        {
//            [BeforeMapping]
//            public void Before()
//            {
//                System.Diagnostics.Debugger.Break();
//            }
//        }

//        class SourceWithBeforeActionSourceOnlyArg : Source
//        {
//            [BeforeMapping]
//            public void Before(Source src)
//            {
//                System.Diagnostics.Debugger.Break();
//            }
//        }
        
//        class SourceWithBeforeActionSourceOnlyArgObject : Source
//        {
//            [BeforeMapping]
//            public void Before([Source] object src)
//            {
//                System.Diagnostics.Debugger.Break();
//            }
//        }

//        class SourceWithBeforeActionTargetOnlyArg : Source
//        {
//            [BeforeMapping]
//            public void Before(Target tgt)
//            {
//                System.Diagnostics.Debugger.Break();
//            }
//        }


//        class SourceWithBeforeActionTargetOnlyArgObject : Source
//        {
//            [BeforeMapping]
//            public void Before([Target] object tgt)
//            {
//                System.Diagnostics.Debugger.Break();
//            }
//        }




//        class SourceWithAfterFunc : Source
//        {
//            [AfterMapping]
//            public Target After(Source src, Target tgt)
//            {
//                System.Diagnostics.Debugger.Break();
//            }
//        }


//        class SourceWithAfterFunc : Source
//        {
//            [AfterMapping]
//            public void After(Source src, Target tgt)
//            {
//                System.Diagnostics.Debugger.Break();
//            }
//        }

//        class TargetWithBeforeFunc : Target
//        {
//            [BeforeMapping]
//            public void Before(Source src, object tgt)
//            {
//                System.Diagnostics.Debugger.Break();
//            }
//        }

//        class TargetWithAfterFunc : Target
//        {
//            [AfterMapping]
//            public object After(Source src, Target tgt)
//            {
//                System.Diagnostics.Debugger.Break();
//                return tgt;
//            }
//        }
//        class TargetWithBeforeFunc : Target
//        {
//            [BeforeMapping]
//            public void Before(Source src, object tgt)
//            {
//                System.Diagnostics.Debugger.Break();
//            }
//        }

//        class TargetWithAfterFunc : Target
//        {
//            [AfterMapping]
//            public object After(Source src, Target tgt)
//            {
//                System.Diagnostics.Debugger.Break();
//                return tgt;
//            }
//        }

//        class Target
//        {
//            public Guid Id { get; set; }

//            public string? Text { get; set; }
//        }
//    }
//}
