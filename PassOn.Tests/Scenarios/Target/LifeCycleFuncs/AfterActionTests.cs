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
    internal class AfterActionTests
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
            public void After(Target src, Source tgt)
            {
                Values.Add(src);
                Values.Add(tgt);
            }
        }

        class NoArgs : Target
        {
            [AfterMapping]
            public void After()
            {
                Values.Add(null);
                Values.Add(null);
            }
        }

        class SourceOnlyArg : Target
        {
            [AfterMapping]
            public void After(Target src)
            {

                Values.Add(src);
                Values.Add(null);
            }
        }

        class OnlyArgObject : Target
        {
            [AfterMapping]
            public void After([Source] object src)
            {
                Values.Add(src);
                Values.Add(null);
            }
        }

        class OnlyArgObjectNoAttribute : Target
        {
            [AfterMapping]
            public void After(object src)
            {
                Values.Add(src);
                Values.Add(null);
            }
        }

        class TargetOnlyArg : Target
        {
            [AfterMapping]
            public void After(Source tgt)
            {
                Values.Add(null);
                Values.Add(tgt);
            }
        }

        class TargetOnlyArgObject : Target
        {
            [AfterMapping]
            public void After([Target] object tgt)
            {
                Values.Add(null);
                Values.Add(tgt);
            }
        }

        class TargetOnlyArgObjectNoAttribute : Target
        {
            [AfterMapping]
            public void After(object tgt)
            {
                Values.Add(null);
                Values.Add(tgt);
            }
        }

        class BothArgsAreObject : Target
        {
            [AfterMapping]
            public void After([Source] object src, [Target] object tgt)
            {
                Values.Add(src);
                Values.Add(tgt);
            }
        }

        class SourceIsObjectTargetIsProper : Target
        {
            [AfterMapping]
            public void After([Source] object src, Source tgt)
            {
                Values.Add(src);
                Values.Add(tgt);
            }
        }

        class SourceIsProperTargetIsObject : Target
        {
            [AfterMapping]
            public void After(Target src, [Target] object tgt)
            {
                Values.Add(src);
                Values.Add(tgt);
            }
        }

        class ArgOrderInverted : Target
        {
            [AfterMapping]
            public void After(Source arg1, Target arg2)
            {
                Values.Add(arg1);
                Values.Add(arg2);
            }
        }

        class BothArgumentsAreTargets : Target
        {
            [AfterMapping]
            public void After(Source tgt, Source src)
            { /* this one will throw an exception */ }
        }

        class BothArgumentsAreSources : Target
        {
            [AfterMapping]
            public void After(Target tgt, Target src)
            { /* this one will throw an exception */ }
        }

        class BothHaveSourceAttributes : Target
        {
            [AfterMapping]
            public void After([Source] Source tgt, [Source] Target src)
            { /* this one will throw an exception */ }
        }

        class BothHaveTargetAttributes : Target
        {
            [AfterMapping]
            public void After([Target] Source tgt, [Target] Target src)
            { /* this one will throw an exception */ }
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
            
            Assert.That(result.Id, Is.EqualTo(initialId)); 
            Assert.That(result.Text, Is.EqualTo(initialText));
            Assert.IsNull(source.Values[0]);
            Assert.IsNull(source.Values[1]); 
        }

        [Test] 
        public void SourceOnlyArgTest() { 
            var (source, mapper) = Arrange<SourceOnlyArg>(); 
            var result = mapper(source); 
            
            Assert.That(result.Id, Is.EqualTo(initialId)); 
            Assert.That(result.Text, Is.EqualTo(initialText));
            Assert.IsTrue(source.Values[0]?.GetType().IsAssignableTo(typeof(Target)));
            Assert.IsNull(source.Values[1]);
        }

        [Test] 
        public void OnlyArgObjectTest() { 
            var (source, mapper) = Arrange<OnlyArgObject>(); 
            var result = mapper(source); 
            
            Assert.That(result.Id, Is.EqualTo(initialId)); 
            Assert.That(result.Text, Is.EqualTo(initialText));
            Assert.IsTrue(source.Values[0]?.GetType().IsAssignableTo(typeof(Target))); 
            Assert.IsNull(source.Values[1]); 
        }

        [Test]
        public void OnlyArgObjectNoAttributeTest()
        {
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
