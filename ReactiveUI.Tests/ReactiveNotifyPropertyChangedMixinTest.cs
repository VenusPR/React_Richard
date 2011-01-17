﻿using System;
using System.Collections.Generic;
using System.Concurrency;
using System.Linq;
using System.Text;
using ReactiveUI.Testing;
using Xunit;

namespace ReactiveUI.Tests
{
    public class HostTestFixture : ReactiveObject
    {
        public TestFixture _Child;
        public TestFixture Child {
            get { return _Child; }
            set { this.RaiseAndSetIfChanged(x => x.Child, value); }
        }
    }

    public class ReactiveNotifyPropertyChangedMixinTest
    {
        [Fact]
        public void OFPSimplePropertyTest()
        {
            (new TestScheduler()).With(sched => {
                var fixture = new TestFixture();
                var changes = fixture.ObservableForProperty(x => x.IsOnlyOneWord).CreateCollection();

                fixture.IsOnlyOneWord = "Foo";
                sched.Run();
                Assert.Equal(1, changes.Count);

                fixture.IsOnlyOneWord = "Bar";
                sched.Run();
                Assert.Equal(2, changes.Count);

                fixture.IsOnlyOneWord = "Baz";
                sched.Run();
                Assert.Equal(3, changes.Count);

                fixture.IsOnlyOneWord = "Baz";
                sched.Run();
                Assert.Equal(3, changes.Count);

                Assert.True(changes.All(x => x.Sender == fixture));
                Assert.True(changes.All(x => x.PropertyName == "IsOnlyOneWord"));
                changes.Select(x => x.Value).AssertAreEqual(new[] {"Foo", "Bar", "Baz"});
            });
        }

        [Fact]
        public void OFPSimpleChildPropertyTest()
        {
            (new TestScheduler()).With(sched => {
                var fixture = new HostTestFixture();
                var changes = fixture.ObservableForProperty(x => x.Child.IsOnlyOneWord).CreateCollection();

                fixture.Child.IsOnlyOneWord = "Foo";
                sched.Run();
                Assert.Equal(1, changes.Count);

                fixture.Child.IsOnlyOneWord = "Bar";
                sched.Run();
                Assert.Equal(2, changes.Count);

                fixture.Child.IsOnlyOneWord = "Baz";
                sched.Run();
                Assert.Equal(3, changes.Count);

                fixture.Child.IsOnlyOneWord = "Baz";
                sched.Run();
                Assert.Equal(3, changes.Count);

                Assert.True(changes.All(x => x.Sender == fixture));
                Assert.True(changes.All(x => x.PropertyName == "IsOnlyOneWord"));
                changes.Select(x => x.Value).AssertAreEqual(new[] {"Foo", "Bar", "Baz"});
            });
        }

        [Fact]
        public void OFPReplacingTheHostShouldResubscribeTheObservable()
        {
             (new TestScheduler()).With(sched => {
                var fixture = new HostTestFixture();
                var changes = fixture.ObservableForProperty(x => x.Child.IsOnlyOneWord).CreateCollection();

                fixture.Child.IsOnlyOneWord = "Foo";
                sched.Run();
                Assert.Equal(1, changes.Count);

                fixture.Child.IsOnlyOneWord = "Bar";
                sched.Run();
                Assert.Equal(2, changes.Count);

                fixture.Child = new TestFixture();
                sched.Run();

                fixture.Child.IsOnlyOneWord = "Baz";
                sched.Run();
                Assert.Equal(3, changes.Count);

                fixture.Child.IsOnlyOneWord = "Baz";
                sched.Run();
                Assert.Equal(3, changes.Count);

                Assert.True(changes.All(x => x.Sender == fixture));
                Assert.True(changes.All(x => x.PropertyName == "IsOnlyOneWord"));
                changes.Select(x => x.Value).AssertAreEqual(new[] {"Foo", "Bar", "Baz"});
            });           
        }
    }
}