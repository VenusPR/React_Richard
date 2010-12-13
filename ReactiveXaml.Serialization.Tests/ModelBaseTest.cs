using System;
using System.Collections.Generic;
using System.Concurrency;
using System.Linq;
using System.Text;
using Microsoft.Pex.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReactiveXaml.Tests;
using Microsoft.Pex.Framework.Generated;

namespace ReactiveXaml.Serialization.Tests
{
    public class ModelTestFixture : ModelBase
    {
        string _TestString;
        public string TestString {
            get { return _TestString; }
            set { this.RaiseAndSetIfChanged(x => x.TestString, value); }
        }
    }

    [PexClass, TestClass]
    public partial class ModelBaseTest
    {
        [PexMethod]
        public void ItemsChangedShouldFire(string[] setters)
        {
            PexAssume.IsNotNull(setters);
            PexAssume.AreElementsNotNull(setters);

            var sched = new TestScheduler();
            var output_changed = new List<object>();
            var output_changing = new List<object>();
            var fixture = sched.With(_ => new ModelTestFixture());

            fixture.ItemChanging.Subscribe(output_changing.Add);
            fixture.ItemChanged.Subscribe(output_changed.Add);

            setters.Run(x => fixture.TestString = x);

            sched.Run();

            PexAssert.AreEqual(setters.Distinct().Count(), output_changed.Count);
            PexAssert.AreEqual(setters.Distinct().Count(), output_changing.Count);
        }

        [PexMethod]
        public void GuidsShouldBeUniqueForContent(string[] setters)
        {
            PexAssume.IsNotNull(setters);
            PexAssume.AreElementsNotNull(setters);

            var sched = new TestScheduler();
            var fixture = sched.With(_ => new ModelTestFixture());
            var output = new Dictionary<string, Guid>();

            setters.ToObservable(sched).Subscribe(x => {
                fixture.TestString = x;
                output[x] = fixture.ContentHash;
            });

            sched.Run();

            PexAssert.AreDistinctValues(output.Values.ToArray());
        }
    }
}
