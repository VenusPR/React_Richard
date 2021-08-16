﻿// Copyright (c) 2021 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows;
using DynamicData.Binding;

namespace ReactiveUI.Tests
{
    /// <summary>
    /// A property bind view model.
    /// </summary>
    /// <seealso cref="ReactiveUI.ReactiveObject" />
    public class PropertyBindViewModel : ReactiveObject
    {
        private string? _property1;
        private PropertyBindModel? _model;
        private int _property2;
        private float _justASingle;
        private double _justADouble;
        private decimal _justADecimal;
        private double? _nullableDouble;
        private byte _justAByte;
        private short _justAInt16;
        private int _justAInt32;
        private long _justAInt64;
        private bool _justABoolean;
        private Visibility _justAVisibility;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyBindViewModel"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        public PropertyBindViewModel(PropertyBindModel? model = null)
        {
            Model = model ?? new PropertyBindModel { AThing = 42, AnotherThing = "Baz" };
            SomeCollectionOfStrings = new ObservableCollectionExtended<string>(new[] { "Foo", "Bar" });
        }

        /// <summary>
        /// Gets or sets the property1.
        /// </summary>
        public string? Property1
        {
            get => _property1;
            set => this.RaiseAndSetIfChanged(ref _property1, value);
        }

        /// <summary>
        /// Gets or sets the property2.
        /// </summary>
        public int Property2
        {
            get => _property2;
            set => this.RaiseAndSetIfChanged(ref _property2, value);
        }

        /// <summary>
        /// Gets or sets the just a double.
        /// </summary>
        public double JustADouble
        {
            get => _justADouble;
            set => this.RaiseAndSetIfChanged(ref _justADouble, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether [just a boolean].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [just a boolean]; otherwise, <c>false</c>.
        /// </value>
        public bool JustABoolean
        {
            get => _justABoolean;
            set => this.RaiseAndSetIfChanged(ref _justABoolean, value);
        }

        /// <summary>
        /// Gets or sets the just a decimal.
        /// </summary>
        public decimal JustADecimal
        {
            get => _justADecimal;
            set => this.RaiseAndSetIfChanged(ref _justADecimal, value);
        }

        /// <summary>
        /// Gets or sets the just a int32.
        /// </summary>
        public byte JustAByte
        {
            get => _justAByte;
            set => this.RaiseAndSetIfChanged(ref _justAByte, value);
        }

        /// <summary>
        /// Gets or sets the just a int32.
        /// </summary>
        public short JustAInt16
        {
            get => _justAInt16;
            set => this.RaiseAndSetIfChanged(ref _justAInt16, value);
        }

        /// <summary>
        /// Gets or sets the just a int32.
        /// </summary>
        public int JustAInt32
        {
            get => _justAInt32;
            set => this.RaiseAndSetIfChanged(ref _justAInt32, value);
        }

        /// <summary>
        /// Gets or sets the just a int32.
        /// </summary>
        public long JustAInt64
        {
            get => _justAInt64;
            set => this.RaiseAndSetIfChanged(ref _justAInt64, value);
        }

        /// <summary>
        /// Gets or sets the nullable double.
        /// </summary>
        public double? NullableDouble
        {
            get => _nullableDouble;
            set => this.RaiseAndSetIfChanged(ref _nullableDouble, value);
        }

        /// <summary>
        /// Gets or sets the just a visibility.
        /// </summary>
        /// <value>
        /// The just a visibility.
        /// </value>
        public Visibility JustAVisibility
        {
            get => _justAVisibility;
            set => this.RaiseAndSetIfChanged(ref _justAVisibility, value);
        }

        /// <summary>
        /// Gets or sets the just a single.
        /// </summary>
        /// <value>
        /// The just a single.
        /// </value>
        public float JustASingle
        {
            get => _justASingle;
            set => this.RaiseAndSetIfChanged(ref _justASingle, value);
        }

        /// <summary>
        /// Gets some collection of strings.
        /// </summary>
        /// <value>
        /// Some collection of strings.
        /// </value>
        public ObservableCollectionExtended<string> SomeCollectionOfStrings { get; }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        public PropertyBindModel? Model
        {
            get => _model;
            set => this.RaiseAndSetIfChanged(ref _model, value);
        }
    }
}
