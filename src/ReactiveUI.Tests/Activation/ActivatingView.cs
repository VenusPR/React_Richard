﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace ReactiveUI.Tests
{
    public sealed class ActivatingView : ReactiveObject, IViewFor<ActivatingViewModel>, IDisposable
    {
        public Subject<Unit> Loaded = new Subject<Unit>();
        public Subject<Unit> Unloaded = new Subject<Unit>();

        private ActivatingViewModel _viewModel;

        public ActivatingView()
        {
            this.WhenActivated(d =>
            {
                IsActiveCount++;
                d(Disposable.Create(() => IsActiveCount--));
            });
        }

        public ActivatingViewModel ViewModel
        {
            get => _viewModel;
            set => this.RaiseAndSetIfChanged(ref _viewModel, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (ActivatingViewModel)value;
        }

        public int IsActiveCount { get; set; }

        public void Dispose()
        {
            Loaded?.Dispose();
            Unloaded?.Dispose();
        }
    }
}
