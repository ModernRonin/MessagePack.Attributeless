using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace MessagePack.Attributeless.Tests
{
    [TestFixture]
    public class ConstraintExtensionsTests
    {
        class TypeWithPublicDefaultConstructor { }

        class TypeWithProtectedDefaultConstructor
        {
            protected TypeWithProtectedDefaultConstructor(int x) { }
        }

        class TypeWithoutDefaultConstructor
        {
            public TypeWithoutDefaultConstructor(int x) { }
        }

        interface IBaseInterface { }

        interface IChildInterface : IBaseInterface { }

        abstract class AnAbstractBaseType : IChildInterface { }

        class BaseType : AnAbstractBaseType { }

        class ChildOfBaseType : BaseType { }

        class UnrelatedType { }

        static IEnumerable<TestCaseData> DerivationCases
        {
            get
            {
                yield return make(typeof(UnrelatedType), typeof(ChildOfBaseType), false);
                yield return make(typeof(UnrelatedType), typeof(BaseType), false);
                yield return make(typeof(UnrelatedType), typeof(AnAbstractBaseType), false);
                yield return make(typeof(UnrelatedType), typeof(IChildInterface), false);
                yield return make(typeof(UnrelatedType), typeof(IBaseInterface), false);

                yield return make(typeof(ChildOfBaseType), typeof(ChildOfBaseType), false);
                yield return make(typeof(ChildOfBaseType), typeof(BaseType), true);
                yield return make(typeof(ChildOfBaseType), typeof(AnAbstractBaseType), true);
                yield return make(typeof(ChildOfBaseType), typeof(IChildInterface), true);
                yield return make(typeof(ChildOfBaseType), typeof(IBaseInterface), true);

                yield return make(typeof(BaseType), typeof(ChildOfBaseType), false);
                yield return make(typeof(BaseType), typeof(BaseType), false);
                yield return make(typeof(BaseType), typeof(AnAbstractBaseType), true);
                yield return make(typeof(BaseType), typeof(IChildInterface), true);
                yield return make(typeof(BaseType), typeof(IBaseInterface), true);

                yield return make(typeof(AnAbstractBaseType), typeof(ChildOfBaseType), false);
                yield return make(typeof(AnAbstractBaseType), typeof(BaseType), false);
                yield return make(typeof(AnAbstractBaseType), typeof(AnAbstractBaseType), false);
                yield return make(typeof(AnAbstractBaseType), typeof(IChildInterface), true);
                yield return make(typeof(AnAbstractBaseType), typeof(IBaseInterface), true);

                yield return make(typeof(IChildInterface), typeof(ChildOfBaseType), false);
                yield return make(typeof(IChildInterface), typeof(BaseType), false);
                yield return make(typeof(IChildInterface), typeof(AnAbstractBaseType), false);
                yield return make(typeof(IChildInterface), typeof(IChildInterface), false);
                yield return make(typeof(IChildInterface), typeof(IBaseInterface), true);

                yield return make(typeof(IBaseInterface), typeof(ChildOfBaseType), false);
                yield return make(typeof(IBaseInterface), typeof(BaseType), false);
                yield return make(typeof(IBaseInterface), typeof(AnAbstractBaseType), false);
                yield return make(typeof(IBaseInterface), typeof(IChildInterface), false);
                yield return make(typeof(IBaseInterface), typeof(IBaseInterface), false);

                TestCaseData make(Type subType, Type baseType, bool expected)
                {
                    var verb = expected ? "derives from" : "does not derive from";
                    return new TestCaseData(subType, baseType, expected).SetName(
                        $"{subType.Name} {verb} {baseType.Name}");
                }
            }
        }

        static void DoesNotThrow(Action action) => action.Should().NotThrow();
        static void Throws(Action action) => action.Should().Throw<ArgumentException>();

        [TestCaseSource(nameof(DerivationCases))]
        public void MustBeDerivedFrom(Type subType, Type baseType, bool isDerivedFrom)
        {
            void action() => subType.MustBeDerivedFrom(baseType);

            if (isDerivedFrom) DoesNotThrow(action);
            else Throws(action);
        }

        [Test]
        public void TypeMustBeDefaultConstructable_is_false_if_type_has_no_default_constructor() =>
            Throws(typeof(TypeWithoutDefaultConstructor).MustBeDefaultConstructable);

        [Test]
        public void TypeMustBeDefaultConstructable_is_false_if_type_has_non_public_default_constructor() =>
            Throws(typeof(TypeWithProtectedDefaultConstructor).MustBeDefaultConstructable);

        [Test]
        public void TypeMustBeDefaultConstructable_is_true_if_type_has_public_default_constructor() =>
            DoesNotThrow(typeof(TypeWithPublicDefaultConstructor).MustBeDefaultConstructable);
    }
}