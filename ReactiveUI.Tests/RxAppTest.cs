﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using ReactiveUI.Xaml;
using Xunit;

namespace ReactiveUI.Tests
{
    public class RxAppTest
    {
#if !MONO
        [Fact]
        public void DepPropNotifierShouldBeFound()
        {
            Assert.True(RxApp.DependencyResolver.GetServices<ICreatesObservableForProperty>()
                .Any(x => x is DependencyObjectObservableForProperty));
        }
#endif

        [Fact]
        public void SchedulerShouldBeCurrentThreadInTestRunner()
        {
            Console.WriteLine(RxApp.DeferredScheduler.GetType().FullName);
            Assert.Equal(CurrentThreadScheduler.Instance, RxApp.DeferredScheduler);
        }
    }
}
