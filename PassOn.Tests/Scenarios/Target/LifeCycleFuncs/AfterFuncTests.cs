using PassOn.Engine;
using PassOn.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassOn.Tests.Scenarios.Target.LifeCycleFuncs
{
    [TestFixture]
    internal class AfterFuncTests
    {

        class Source
        {
            public Guid Id { get; set; }

            public string? Text { get; set; }
        }

        class Target
        {
            public List<object?> Values { get; private set; } = new List<object?>();

            public Guid Id { get; set; }
            public string? Text { get; set; }
        }

        class ProperArgs : Target
        {
            [AfterMapping]
            public Source Before(Target src, Source tgt)
            {
                Values.Add(src);
                Values.Add(tgt);

                return tgt;
            }
        }

        class NoArgs : Target
        {
            [AfterMapping]
            public Source Before()
            {
                Values.Add(null);
                Values.Add(null);

                return new Source();
            }
        }

        class SourceOnlyArg : Target
        {
            [AfterMapping]
            public Source Before(Target src)
            {
                Values.Add(src);
                Values.Add(null);

                return new Source();
            }
        }

        class OnlyArgObject : Target
        {
            [AfterMapping]
            public Source Before([Source] object src)
            {
                Values.Add(src);
                Values.Add(null);

                return new Source();
            }
        }

        class OnlyArgObjectNoAttribute : Target
        {
            [AfterMapping]
            public Source Before(object src)
            {
                Values.Add(src);
                Values.Add(null);

                return new Source();
            }
        }

        class TargetOnlyArg : Target
        {
            [AfterMapping]
            public Source Before(Source tgt)
            {
                Values.Add(null);
                Values.Add(tgt);

                return tgt;
            }
        }

        class TargetOnlyArgObject : Target
        {
            [AfterMapping]
            public Source Before([Target] object tgt)
            {
                Values.Add(null);
                Values.Add(tgt);

                return (Source) tgt;
            }
        }

        class TargetOnlyArgObjectNoAttribute : Target
        {
            [AfterMapping]
            public Source Before(object tgt)
            {
                Values.Add(null);
                Values.Add(tgt);

                return new Source();
            }
        }

        class BothArgsAreObject : Target
        {
            [AfterMapping]
            public Source Before([Source] object src, [Target] object tgt)
            {
                Values.Add(src);
                Values.Add(tgt);

                return (Source) tgt;
            }
        }

        class SourceIsObjectTargetIsProper : Target
        {
            [AfterMapping]
            public Source Before([Source] object src, Source tgt)
            {
                Values.Add(src);
                Values.Add(tgt);

                return tgt;
            }
        }

        class SourceIsProperTargetIsObject : Target
        {
            [AfterMapping]
            public Source Before(Target src, [Target] object tgt)
            {
                Values.Add(src);
                Values.Add(tgt);

                return (Source) tgt;
            }
        }

        class ArgOrderInverted : Target
        {
            [AfterMapping]
            public Source Before(Source arg1, Target arg2)
            {
                Values.Add(arg1);
                Values.Add(arg2);

                return arg1;
            }
        }

        class BothArgumentsAreTargets : Target
        {
            [AfterMapping]
            public Source Before(Source tgt, Source src)
            { 
                /* this one will throw an exception */
                return tgt;
            }
        }

        class BothArgumentsAreSources : Target
        {
            [AfterMapping]
            public Source Before(Target tgt, Target src)
            { 
                /* this one will throw an exception */
                return new Source();
            }
        }

        class BothHaveSourceAttributes : Target
        {
            [AfterMapping]
            public Source Before([Source] Source tgt, [Source] Target src)
            { 
                /* this one will throw an exception */
                return tgt;
            }
        }

        class BothHaveTargetAttributes : Target
        {
            [AfterMapping]
            public Source Before([Target] Source tgt, [Target] Target src)
            {
                /* this one will throw an exception */
                return tgt;
            }
        }

        Guid initialId;
        string initialText;
        MapperEngine passOnEngine;

        [SetUp]
        public void Setup()
        {

            initialId = Guid.NewGuid();
            initialText = Utilities.RandomString();
            passOnEngine = new MapperEngine();
        }

        private (S source, Func<S, Source> mapper) Arrange<S>()
        where S : Target, new()
        {
            var source = new S
            {
                Id = initialId,
                Text = initialText,
            };

            var mapper = (Func<S, Source>)passOnEngine
                .GetOrCreateMapper<S, Source>();

            return (source, mapper);
        }

        [Test]
        public void BothHaveTargetAttributesTest()
        {
            Assert.Throws<BothArgumentsAreTargetsException>(
                () => Arrange<BothHaveTargetAttributes>()
            );
        }

        [Test]
        public void BothHaveSourceAttributesTest()
        {
            Assert.Throws<BothArgumentsAreSourcesException>(
                () => Arrange<BothHaveSourceAttributes>()
            );
        }

        [Test]
        public void BothArgumentsAreTargetsTest()
        {
            Assert.Throws<BothArgumentsAreTargetsException>(
                () => Arrange<BothArgumentsAreTargets>()
            );
        }

        [Test]
        public void BothArgumentsAreSourcesTest()
        {
            Assert.Throws<BothArgumentsAreSourcesException>(
                () => Arrange<BothArgumentsAreSources>()
            );
        }

        [Test]
        public void ArgOrderInvertedTest()
        {
            var (source, mapper) = Arrange<ArgOrderInverted>();

            var result = mapper(source);

            Assert.That(result.Id, Is.EqualTo(initialId));
            Assert.That(result.Text, Is.EqualTo(initialText));
            Assert.That(source.Values[0]?.GetType(), Is.EqualTo(typeof(Source)));
            Assert.IsTrue(source.Values[1]?.GetType().IsAssignableTo(typeof(Target)));
        }

        [Test]
        public void SourceIsProperTargetIsObjectTest()
        {
            var (source, mapper) = Arrange<SourceIsProperTargetIsObject>();

            var result = mapper(source);

            Assert.That(result.Id, Is.EqualTo(initialId));
            Assert.That(result.Text, Is.EqualTo(initialText));
            Assert.IsTrue(source.Values[0]?.GetType().IsAssignableTo(typeof(Target)));
            Assert.That(source.Values[1]?.GetType(), Is.EqualTo(typeof(Source)));
        }

        [Test] 
        public void ProperArgsTest() { 
            var (source, mapper) = Arrange<ProperArgs>(); 
            var result = mapper(source); 
            
            Assert.That(result.Id, Is.EqualTo(initialId)); 
            Assert.That(result.Text, Is.EqualTo(initialText));
            Assert.IsTrue(source.Values[0]?.GetType().IsAssignableTo(typeof(Target))); 
            Assert.That(source.Values[1]?.GetType(), Is.EqualTo(typeof(Source))); 
        }

        [Test] 
        public void NoArgsTest() { 
            var (source, mapper) = Arrange<NoArgs>(); 
            var result = mapper(source); 
            
            Assert.That(result.Id, Is.EqualTo(Guid.Empty)); 
            Assert.IsNull(result.Text);
            Assert.IsNull(source.Values[0]);
            Assert.IsNull(source.Values[1]); 
        }

        [Test] 
        public void SourceOnlyArgTest() { 
            var (source, mapper) = Arrange<SourceOnlyArg>(); 
            var result = mapper(source); 
            
            Assert.That(result.Id, Is.EqualTo(Guid.Empty));
            Assert.IsNull(result.Text);
            Assert.IsTrue(source.Values[0]?.GetType().IsAssignableTo(typeof(Target)));
            Assert.IsNull(source.Values[1]);
        }

        [Test] 
        public void OnlyArgObjectTest() { 
            var (source, mapper) = Arrange<OnlyArgObject>(); 
            var result = mapper(source); 
            
            Assert.That(result.Id, Is.EqualTo(Guid.Empty)); 
            Assert.IsNull(result.Text);
            Assert.IsTrue(source.Values[0]?.GetType().IsAssignableTo(typeof(Target))); 
            Assert.IsNull(source.Values[1]); 
        }

        [Test] 
        public void OnlyArgObjectNoAttributeTest() { 
            Assert.Throws<AmbiguousArgumentMatchException>(
                () => Arrange<OnlyArgObjectNoAttribute>()
            );
        }

        [Test] 
        public void TargetOnlyArgTest() { 
            var (source, mapper) = Arrange<TargetOnlyArg>(); 
            var result = mapper(source); 
            
            Assert.That(result.Id, Is.EqualTo(initialId)); 
            Assert.That(result.Text, Is.EqualTo(initialText));
            Assert.IsNull(source.Values[0]); 
            Assert.That(source.Values[1]?.GetType(), Is.EqualTo(typeof(Source))); 
        }

        [Test] 
        public void TargetOnlyArgObjectTest() { 
            var (source, mapper) = Arrange<TargetOnlyArgObject>(); 
            var result = mapper(source); 
            
            Assert.That(result.Id, Is.EqualTo(initialId)); 
            Assert.That(result.Text, Is.EqualTo(initialText));
            Assert.IsNull(source.Values[0]);
            Assert.That(source.Values[1]?.GetType(), Is.EqualTo(typeof(Source))); 
        }

        [Test] 
        public void TargetOnlyArgObjectNoAttributeTest() { 
            Assert.Throws<AmbiguousArgumentMatchException>(
                () => Arrange<TargetOnlyArgObjectNoAttribute>()
            );
        }

        [Test] 
        public void BothArgsAreObjectTest() { 
            var (source, mapper) = Arrange<BothArgsAreObject>(); 
            var result = mapper(source); 
            
            Assert.That(result.Id, Is.EqualTo(initialId)); 
            Assert.That(result.Text, Is.EqualTo(initialText));
            Assert.IsTrue(source.Values[0]?.GetType().IsAssignableTo(typeof(Target))); 
            Assert.That(source.Values[1]?.GetType(), Is.EqualTo(typeof(Source))); 
        }

        [Test] 
        public void SourceIsObjectTargetIsProperTest() { 
            var (source, mapper) = Arrange<SourceIsObjectTargetIsProper>(); 
            var result = mapper(source); 
            
            Assert.That(result.Id, Is.EqualTo(initialId)); 
            Assert.That(result.Text, Is.EqualTo(initialText));
            Assert.IsTrue(source.Values[0]?.GetType().IsAssignableTo(typeof(Target))); 
            Assert.That(source.Values[1]?.GetType(), Is.EqualTo(typeof(Source))); 
        }
    }
}
