﻿using ReactiveXaml;
using Xunit;
using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ReactiveXaml.Tests
{
    public class ValidatedTestFixture : ReactiveValidatedObject
    {
        string _IsNotNullString;
        [Required]
        public string IsNotNullString {
            get { return _IsNotNullString; }
            set { this.RaiseAndSetIfChanged(x => x.IsNotNullString, value); }
        }
        
        string _IsOnlyOneWord;
        [Required]
        [RegularExpression(@"^[a-zA-Z]+$")]
        public string IsOnlyOneWord {
            get { return _IsOnlyOneWord; }
            set { this.RaiseAndSetIfChanged(x => x.IsOnlyOneWord, value); }
        }

        string _UsesExprRaiseSet;
        public string UsesExprRaiseSet {
            get { return _UsesExprRaiseSet; }
            set { _UsesExprRaiseSet = this.RaiseAndSetIfChanged(x => x.UsesExprRaiseSet, value); }
        }
    }

    public class ReactiveValidatedObjectTest
    {
        [Fact]
        public void IsObjectValidTest()
        {
            var output = new List<bool>();
            var fixture = new ValidatedTestFixture();
            //fixture.IsValidObservable.Subscribe(output.Add);

            Assert.IsFalse(fixture.IsObjectValid());

            fixture.IsNotNullString = "foo";
            Assert.IsFalse(fixture.IsObjectValid());

            fixture.IsOnlyOneWord = "Foo Bar";
            Assert.IsFalse(fixture.IsObjectValid());

            fixture.IsOnlyOneWord = "Foo";
            Assert.IsTrue(fixture.IsObjectValid());

            fixture.IsOnlyOneWord = "";
            Assert.IsFalse(fixture.IsObjectValid());

            /*
            new[] { false, false, false, true, false }.Zip(output, (expected, actual) => new { expected, actual })
                .Do(Console.WriteLine)
                .Run(x => Assert.Equal(x.expected, x.actual));
             */
        }
    }
}
