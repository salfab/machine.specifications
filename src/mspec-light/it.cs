using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace mspec_light
{
    // [DebuggerStepThrough]
    [TestFixture]
    public abstract class it
    {
        protected Exception Exception;

        [ActDelegate]
        public delegate void Because();

        [CleanupDelegate]
        public delegate void Cleanup();

        [SetupDelegate]
        public delegate void Establish();

        [AssertDelegate]
        public delegate void It();

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            InvokeEstablish();
            InvokeBecause();
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            InvokeCleanup();
        }

        void InvokeEstablish()
        {
            var types = new Stack<Type>();
            var type = GetType();

            do
            {
                types.Push(type);
                type = type.BaseType;
            } while (type != typeof(it));

            foreach (var t in types)
            {
                var establishFieldInfo =
                    t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                        .SingleOrDefault(x => x.FieldType.GetCustomAttribute<SetupDelegateAttribute>() != null);

                Delegate establish = null;

                if (establishFieldInfo != null) establish = establishFieldInfo.GetValue(this) as Delegate;
                if (establish != null) Exception = Catch.Exception(() => establish.DynamicInvoke(null));
            }
        }

        // [EditorBrowsable(EditorBrowsableState.Never)]
        void InvokeBecause()
        {
            var t = GetType();

            var becauseFieldInfo =
                t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                    .SingleOrDefault(x => x.FieldType.GetCustomAttribute<ActDelegateAttribute>() != null);

            Delegate because = null;

            if (becauseFieldInfo != null) because = becauseFieldInfo.GetValue(this) as Delegate;
            if (because != null) Exception = Catch.Exception(() => because.DynamicInvoke(null));
        }

        // [EditorBrowsable(EditorBrowsableState.Never)]
        private void InvokeCleanup()
        {
            try
            {
                var t = GetType();

                FieldInfo cleanupFieldInfo =
                    t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                        .SingleOrDefault(x => x.FieldType.GetCustomAttribute<CleanupDelegateAttribute>() != null);

                Delegate cleanup = null;

                if (cleanupFieldInfo != null) cleanup = cleanupFieldInfo.GetValue(this) as Delegate;
                if (cleanup != null) cleanup.DynamicInvoke(null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public IEnumerable GetObservations()
        {
            var t = GetType();

            var category = (CategoryAttribute)t.GetCustomAttributes(typeof(CategoryAttribute), true).FirstOrDefault();
            string categoryName = null;

            if (category != null)
                categoryName = category.Name;

            var itFieldInfos =
                t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                    .Where(x => x.FieldType.GetCustomAttribute<AssertDelegateAttribute>() != null);

            return itFieldInfos
                .Select(fieldInfo => new TestCaseData(fieldInfo.GetValue(this))
                .SetDescription("Jehaha")
                    .SetName(fieldInfo.Name.ToFormat())
                    .SetCategory(categoryName));
        }

        [Test, TestCaseSource("GetObservations")]
        public void should(Delegate observation)
        {
            if (Exception != null)
                throw Exception;

            observation.DynamicInvoke();
        }
    }
}
