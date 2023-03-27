using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using PassOn.EngineExtensions;

namespace PassOn.Tests.ILGenerationTests
{
    [TestFixture]
    internal class MapperFactoriesTests
    {
        class ClassWith {
            public class Num
            {
                public int? Number { get; set; }
            }

            public class NumAndStr
            {
                public int? Number { get; set; }
                public string? String { get; set; }
            }

            public class List<T>
            {
                public System.Collections.Generic.List<T> Value { get; set; }
            }
        }
        public class Wrapper<T>
        {         
            public T? MappedValue { get; set; }
            public T? AnotherValue { get; set; }
        }

        private Func<Source, PassOnEngine, int, Target> CreateFunc<Source, Target>(Action<DynamicMethod, ILGenerator> addBody)
        {
            var dynMethod = new DynamicMethod(
                "__testFunction__",
                typeof(Target),
                new Type[] { typeof(Source), typeof(PassOnEngine), typeof(int) },
                Assembly.GetExecutingAssembly().ManifestModule,
                true);

            var il =
                dynMethod.GetILGenerator();

            addBody(dynMethod, il);

            il.Emit(OpCodes.Ret);

            var delType = typeof(Func<,,,>)
                .MakeGenericType(typeof(Source), typeof(PassOnEngine), typeof(int), typeof(Target));

            var func = dynMethod.CreateDelegate(delType);

            return (Func<Source, PassOnEngine, int, Target>)func;
        }

        private LocalBuilder GetInitializedResultLocal(ILGenerator il) {
            var resultLocal =
                    il.DeclareLocal(typeof(ClassWith.NumAndStr));

            var ctr = typeof(Wrapper<ClassWith.Num>).GetConstructor(Type.EmptyTypes);

            if (ctr == null)
            {
                throw new Exception("Type dont have an empty ctr!");
            }

            il.Emit(OpCodes.Newobj, ctr);
            il.Emit(OpCodes.Stloc, resultLocal);

            return resultLocal;
        }

        [Test]
        public void EmitReferenceTypeCopyTest() {
            var mapNumProps = CreateFunc<Wrapper<ClassWith.Num>, Wrapper<ClassWith.NumAndStr>>((fn, il) => {
                
                var resultLocal = 
                    GetInitializedResultLocal(il);

                var inputProp = typeof(Wrapper<ClassWith.Num>).GetProperty("MappedValue");
                var outputProp = typeof(Wrapper<ClassWith.NumAndStr>).GetProperty("MappedValue");

                MapperFactories.EmitReferenceTypeCopy(il, fn, resultLocal, inputProp, outputProp);

                il.Emit(OpCodes.Ldloc, resultLocal);
            });

            var expected = new Wrapper<ClassWith.Num> {
                MappedValue = new ClassWith.Num
                { 
                    Number = new Random().Next(),
                },
                AnotherValue = new ClassWith.Num
                {
                    Number = new Random().Next(),
                }
            };

            var engine = new PassOnEngine();

            var result = mapNumProps(
                expected, engine, 0
            );

            Assert.That(result?.MappedValue?.Number, Is.EqualTo(expected.MappedValue.Number));
            Assert.That(result?.AnotherValue, Is.Null);
        }

        [Test]
        public void EmitStackOverflowCheckTest()
        {
            var mapNumProps = CreateFunc<object, int>((fn, il) => {

                MapperFactories.EmitStackOverflowCheck(il, fn);
                // to return 1;
                il.Emit(OpCodes.Ldc_I4_1);
            });

            var engine = new PassOnEngine();

            Assert.Throws<StackOverflowException>(() => mapNumProps(
                null, null, MapperFactories.MAX_RECURSION + 1
            ));
        }

        [Test]
        public void EmitStackOverflowCheckSafeTest()
        {
            var mapNumProps = CreateFunc<object, int>((fn, il) => {

                MapperFactories.EmitStackOverflowCheck(il, fn);
                // to return 1;
                il.Emit(OpCodes.Ldc_I4_1);
            });

            var engine = new PassOnEngine();

            var result = mapNumProps(
                null, null, MapperFactories.MAX_RECURSION - 1
            );

            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void CreateMapperSimpleWrapperTest() 
        {
            var mapper = MapperFactories.CreateMapper<
                Wrapper<ClassWith.Num>,
                Wrapper<ClassWith.NumAndStr>
            >();

            var expected = new Wrapper<ClassWith.Num>
            {
                MappedValue = new ClassWith.Num
                {
                    Number = new Random().Next(),
                }
            };

            var engine = new PassOnEngine();

            var result = mapper(
                expected, engine, 0
            );

            Assert.That(
                result?.MappedValue?.Number,
                Is.EqualTo(expected.MappedValue.Number)
            );
        }

       
        [Test]
        public void CreateMapperWrappedListTest()
        {
            var merger = MapperFactories.CreateMapper<
                Wrapper<ClassWith.List<ClassWith.Num>>,
                Wrapper<ClassWith.List<ClassWith.NumAndStr>>
            >();

            var expectedSrc = new Wrapper<ClassWith.List<ClassWith.Num>>
            {
                MappedValue =
                    new ClassWith.List<ClassWith.Num>
                    {
                        Value = new List<ClassWith.Num>()
                        {
                            new ClassWith.Num
                            {
                                Number = new Random().Next(),
                            }
                        },
                    }
            };
                     
            var engine = new PassOnEngine();

            var result = merger(
                expectedSrc, engine, 0
            );

            Assert.That(
                result?.MappedValue?.Value[0].Number,
                Is.EqualTo(expectedSrc.MappedValue.Value[0].Number)
            );

            Assert.That(
                result?.AnotherValue?.Value[0].Number,
                Is.Null
            );
        }

        [Test]
        public void CreateMapperWrappedIntListTest()
        {
            var merger = MapperFactories.CreateMapper<
                Wrapper<ClassWith.List<int>>,
                Wrapper<ClassWith.List<int>>
            >();

            var expectedSrc = new Wrapper<ClassWith.List<int>>
            {
                MappedValue =
                    new ClassWith.List<int>
                    {
                        Value = new List<int>()
                        {
                            new Random().Next(),                            
                        },
                    }
            };

            var engine = new PassOnEngine();

            var result = merger(
                expectedSrc, engine, 0
            );

            Assert.That(
                result?.MappedValue?.Value[0],
                Is.EqualTo(expectedSrc.MappedValue.Value[0])
            );

            Assert.That(
                result?.AnotherValue?.Value[0],
                Is.Null
            );
        }

        [Test]
        public void CreateMapperWrappedIntListToWrappedStringListTest()
        {
            var merger = MapperFactories.CreateMapper<
                Wrapper<ClassWith.List<int>>,
                Wrapper<ClassWith.List<string>>
            >();

            var expectedSrc = new Wrapper<ClassWith.List<int>>
            {
                MappedValue =
                    new ClassWith.List<int>
                    {
                        Value = new List<int>()
                        {
                            new Random().Next(),
                        },
                    }
            };

            var engine = new PassOnEngine();

            var result = merger(
                expectedSrc, engine, 0
            );

            Assert.That(
                result?.MappedValue?.Value[0],
                Is.EqualTo(expectedSrc.MappedValue.Value[0].ToString())
            );

            Assert.That(
                result?.AnotherValue?.Value[0],
                Is.Null
            );
        }

        [Test]
        public void CreateMapperPrimitivesTest()
        {
            var merger = MapperFactories.CreateMapper<
                ClassWith.Num,
                ClassWith.NumAndStr
            >();

            var expectedSrc = new ClassWith.Num
            { Number = new Random().Next(), };

            var engine = new PassOnEngine();

            var result = merger(
                expectedSrc, engine, 0
            );

            Assert.That(
                result?.Number,
                Is.EqualTo(expectedSrc.Number)
            );
        }

        [Test]
        public void CreateMergerSimpleWrapperTest()
        {
            var merger = MapperFactories.CreateMerger<
                Wrapper<ClassWith.Num>,
                Wrapper<ClassWith.NumAndStr>
            >();

            var expectedSrc = new Wrapper<ClassWith.Num>
            {
                MappedValue = new ClassWith.Num
                {
                    Number = new Random().Next(),
                },

            };

            var expectedTgt = new Wrapper<ClassWith.NumAndStr>
            {
                AnotherValue = new ClassWith.NumAndStr
                {
                    Number = new Random().Next(),
                }
            };

            var engine = new PassOnEngine();

            var result = merger(
                expectedSrc, expectedTgt, engine, 0
            );

            Assert.That(
                result?.MappedValue?.Number,
                Is.EqualTo(expectedSrc.MappedValue.Number)
            );
            Assert.That(
                result?.AnotherValue?.Number, 
                Is.EqualTo(expectedTgt?.AnotherValue?.Number)
            );            
        }

        [Test]
        public void CreateMergerWrappedListTest()
        {
            var merger = MapperFactories.CreateMerger<
                Wrapper<ClassWith.List<ClassWith.Num>>,
                Wrapper<ClassWith.List<ClassWith.NumAndStr>>
            >();

            var expectedSrc = new Wrapper<ClassWith.List<ClassWith.Num>>
            {
                MappedValue =
                    new ClassWith.List<ClassWith.Num>
                    {
                        Value = new List<ClassWith.Num>()
                        {
                            new ClassWith.Num
                            {
                                Number = new Random().Next(),
                            }
                        },
                    }
            };

            var expectedTgt = new Wrapper<ClassWith.List<ClassWith.NumAndStr>>
            {
                AnotherValue =
                    new ClassWith.List<ClassWith.NumAndStr>
                    {
                        Value = new List<ClassWith.NumAndStr>()
                        {
                            new ClassWith.NumAndStr
                            {
                                Number = new Random().Next(),
                            }
                        },
                    }
            };

            var engine = new PassOnEngine();

            var result = merger(
                expectedSrc, expectedTgt, engine, 0
            );

            Assert.That(
                result?.MappedValue?.Value[0].Number,
                Is.EqualTo(expectedSrc.MappedValue.Value[0].Number)
            );
            Assert.That(
                result?.AnotherValue?.Value[0].Number,
                Is.EqualTo(expectedTgt?.AnotherValue?.Value[0].Number)
            );
        }


        [Test]
        public void CreateMergerListOfWrappedListTest()
        {
            var merger = MapperFactories.CreateMerger<
                List<Wrapper<ClassWith.List<ClassWith.Num>>>,
                Wrapper<ClassWith.List<ClassWith.NumAndStr>>[]
            >();

            var expectedSrc = new List<Wrapper<ClassWith.List<ClassWith.Num>>>() {
                new Wrapper<ClassWith.List<ClassWith.Num>>
                {
                    MappedValue =
                    new ClassWith.List<ClassWith.Num>
                    {
                        Value = new List<ClassWith.Num>()
                        {
                            new ClassWith.Num
                            {
                                Number = new Random().Next(),
                            }
                        },
                    }
                }
            };

            var expectedTgt = new[] {
                new Wrapper<ClassWith.List<ClassWith.NumAndStr>>() {
                    AnotherValue =
                        new ClassWith.List<ClassWith.NumAndStr>
                        {
                            Value = new List<ClassWith.NumAndStr>()
                            {
                                new ClassWith.NumAndStr
                                {
                                    Number = new Random().Next(),
                                }
                            },
                        }
                } 
            };

            var engine = new PassOnEngine();

            var result = merger(
                expectedSrc, expectedTgt, engine, 0
            );

            Assert.That(
                result?[0].MappedValue?.Value[0].Number,
                Is.EqualTo(expectedSrc[0]?.MappedValue?.Value[0].Number)
            );
            // this second part doesnt quite work, because it maps, not merge, on that level
            //Assert.That(
            //    result?[0].AnotherValue?.Value[0].Number,
            //    Is.EqualTo(expectedTgt[0]?.AnotherValue?.Value[0].Number)
            //);
        }
    }
}
