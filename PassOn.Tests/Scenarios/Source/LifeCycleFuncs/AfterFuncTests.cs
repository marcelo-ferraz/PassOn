using PassOn.Engine;
using PassOn.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassOn.Tests.Scenarios.Source.LifeCycleFuncs
{
    [TestFixture]
    internal class AfterFuncTests
    {

        class Target
        {
            public Guid Id { get; set; }

            public string? Text { get; set; }
        }

        class Source
        {
            public List<object?> Values { get; private set; } = new List<object?>();

            public Guid Id { get; set; }
            public string? Text { get; set; }
        }

        class ProperArgs : Source
        {
            [AfterMapping]
            public Target Before(Source src, Target tgt)
            {
                Values.Add(src);
                Values.Add(tgt);

                return tgt;
            }
        }

        class NoArgs : Source
        {
            [AfterMapping]
            public Target Before()
            {
                Values.Add(null);
                Values.Add(null);

                return new Target();
            }
        }

        class SourceOnlyArg : Source
        {
            [AfterMapping]
            public Target Before(Source src)
            {
                Values.Add(src);
                Values.Add(null);

                return new Target();
            }
        }

        class OnlyArgObject : Source
        {
            [AfterMapping]
            public Target Before([Source] object src)
            {
                Values.Add(src);
                Values.Add(null);

                return new Target();
            }
        }

        class OnlyArgObjectNoAttribute : Source
        {
            [AfterMapping]
            public Target Before(object src)
            {
                Values.Add(src);
                Values.Add(null);

                return new Target();
            }
        }

        class TargetOnlyArg : Source
        {
            [AfterMapping]
            public Target Before(Target tgt)
            {
                Values.Add(null);
                Values.Add(tgt);

                return tgt;
            }
        }

        class TargetOnlyArgObject : Source
        {
            [AfterMapping]
            public Target Before([Target] object tgt)
            {
                Values.Add(null);
                Values.Add(tgt);

                return (Target) tgt;
            }
        }

        class TargetOnlyArgObjectNoAttribute : Source
        {
            [AfterMapping]
            public Target Before(object tgt)
            {
                Values.Add(null);
                Values.Add(tgt);

                // tgt is null here, as there is no way to know 
                // maybe I should throw an exception?
                return new Target();
            }
        }

        class BothArgsAreObject : Source
        {
            [AfterMapping]
            public Target Before([Source] object src, [Target] object tgt)
            {
                Values.Add(src);
                Values.Add(tgt);

                return (Target) tgt;
            }
        }

        class SourceIsObjectTargetIsProper : Source
        {
            [AfterMapping]
            public Target Before([Source] object src, Target tgt)
            {
                Values.Add(src);
                Values.Add(tgt);

                return tgt;
            }
        }

        class SourceIsProperTargetIsObject : Source
        {
            [AfterMapping]
            public Target Before(Source src, [Target] object tgt)
            {
                Values.Add(src);
                Values.Add(tgt);

                return (Target) tgt;
            }
        }

        class ArgOrderInverted : Source
        {
            [AfterMapping]
            public Target Before(Target arg1, Source arg2)
            {
                Values.Add(arg1);
                Values.Add(arg2);

                return arg1;
            }
        }

        class BothArgumentsAreTargets : Source
        {
            [AfterMapping]
            public Target Before(Target tgt, Target src)
            { 
                /* this one will throw an exception */
                return tgt;
            }
        }

        class BothArgumentsAreSources : Source
        {
            [AfterMapping]
            public Target Before(Source tgt, Source src)
            { 
                /* this one will throw an exception */
                return new Target();
            }
        }

        class BothHaveSourceAttributes : Source
        {
            [AfterMapping]
            public Target Before([Source] Target tgt, [Source] Source src)
            { 
                /* this one will throw an exception */
                return tgt;
            }
        }

        class BothHaveTargetAttributes : Source
        {
            [AfterMapping]
            public Target Before([Target] Target tgt, [Target] Source src)
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

        private (S source, Func<S, Target> mapper) Arrange<S>()
        where S : Source, new()
        {
            var source = new S
            {
                Id = initialId,
                Text = initialText,
            };

            var mapper = (Func<S, Target>)passOnEngine
                .GetOrCreateMapper<S, Target>();

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
            Assert.That(source.Values[0]?.GetType(), Is.EqualTo(typeof(Target)));
            Assert.IsTrue(source.Values[1]?.GetType().IsAssignableTo(typeof(Source)));
        }

        [Test]
        public void SourceIsProperTargetIsObjectTest()
        {
            var (source, mapper) = Arrange<SourceIsProperTargetIsObject>();

            var result = mapper(source);

            Assert.That(result.Id, Is.EqualTo(initialId));
            Assert.That(result.Text, Is.EqualTo(initialText));
            Assert.IsTrue(source.Values[0]?.GetType().IsAssignableTo(typeof(Source)));
            Assert.That(source.Values[1]?.GetType(), Is.EqualTo(typeof(Target)));
        }

        [Test] 
        public void ProperArgsTest() { 
            var (source, mapper) = Arrange<ProperArgs>(); 
            var result = mapper(source); 
            
            Assert.That(result.Id, Is.EqualTo(initialId)); 
            Assert.That(result.Text, Is.EqualTo(initialText));
            Assert.IsTrue(source.Values[0]?.GetType().IsAssignableTo(typeof(Source))); 
            Assert.That(source.Values[1]?.GetType(), Is.EqualTo(typeof(Target))); 
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
            Assert.IsTrue(source.Values[0]?.GetType().IsAssignableTo(typeof(Source)));
            Assert.IsNull(source.Values[1]);
        }

        [Test] 
        public void OnlyArgObjectTest() { 
            var (source, mapper) = Arrange<OnlyArgObject>(); 
            var result = mapper(source); 
            
            Assert.That(result.Id, Is.EqualTo(Guid.Empty)); 
            Assert.IsNull(result.Text);
            Assert.IsTrue(source.Values[0]?.GetType().IsAssignableTo(typeof(Source))); 
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
            Assert.That(source.Values[1]?.GetType(), Is.EqualTo(typeof(Target))); 
        }

        [Test] 
        public void TargetOnlyArgObjectTest() { 
            var (source, mapper) = Arrange<TargetOnlyArgObject>(); 
            var result = mapper(source); 
            
            Assert.That(result.Id, Is.EqualTo(initialId)); 
            Assert.That(result.Text, Is.EqualTo(initialText));
            Assert.IsNull(source.Values[0]);
            Assert.That(source.Values[1]?.GetType(), Is.EqualTo(typeof(Target))); 
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
            Assert.IsTrue(source.Values[0]?.GetType().IsAssignableTo(typeof(Source))); 
            Assert.That(source.Values[1]?.GetType(), Is.EqualTo(typeof(Target))); 
        }

        [Test] 
        public void SourceIsObjectTargetIsProperTest() { 
            var (source, mapper) = Arrange<SourceIsObjectTargetIsProper>(); 
            var result = mapper(source); 
            
            Assert.That(result.Id, Is.EqualTo(initialId)); 
            Assert.That(result.Text, Is.EqualTo(initialText));
            Assert.IsTrue(source.Values[0]?.GetType().IsAssignableTo(typeof(Source))); 
            Assert.That(source.Values[1]?.GetType(), Is.EqualTo(typeof(Target))); 
        }
    }
}
