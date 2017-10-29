using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;

namespace HomeExercises
{
    public class NumberValidatorTests
    {
        [TestCase(-1, 2, true, TestName = "Only Positive")]
        [TestCase(-1, 2, false, TestName = "Not Only Positive")]
        [TestCase(1, -1, true, TestName = "Scale <= 0 and positive")]
        [TestCase(1, -1, false, TestName = "Scale <= 0 and negative")]
        [TestCase(1, 2, true, TestName = "Scale >= Precision and positive")]
        [TestCase(1, 2, false, TestName = "Scale >= Precision and negative")]
        public void When_InvalidArguments_ShouldThrow_ArgumentException
            (int precision, int scale, bool onlyPositive)
        {
            Action act = () => new NumberValidator(precision, scale, onlyPositive);
            act.ShouldThrow<ArgumentException>();
        }

        private static IEnumerable TestCasesForValidArgsTest()
        {
            return new[]
            {
                new TestCaseData(3, 2, true, "").Returns(false).SetName("Empty string"), 
                new TestCaseData(3, 2, true, null).Returns(false).SetName("Null string"),
                new TestCaseData(3, 2, true, "+0.0").Returns(true).SetName("Number starts with +"), 
                new TestCaseData(3, 2, true, "0.00").Returns(true).SetName("Everything correct"), 
                new TestCaseData(4, 2, true, "0.000").Returns(false).SetName("3 digits in fracPart(2 allowed)"), 
                new TestCaseData(3, 2, true, "-0.0").Returns(false).SetName("negative num + onlyPositive"), 
                new TestCaseData(3, 2, false, "-0.0").Returns(true).SetName("negative num + negative allowed"), 
                new TestCaseData(3, 2, false, "0.-0").Returns(false).SetName("- in fracPart"), 
                new TestCaseData(3, 2, true, "l.ol").Returns(false).SetName("value is not a number"), 
            };
        }

        [Test]
        [TestCaseSource("TestCasesForValidArgsTest")]
        public bool When_ValidArguments_Should_WorkCorrectly
            (int precision, int scale, bool onlyPositive, string value)
        {
            return new NumberValidator(precision, scale, onlyPositive).IsValidNumber(value);
        }
    }

    public class NumberValidator
    {
        private readonly Regex numberRegex;
        private readonly bool onlyPositive;
        private readonly int precision;
        private readonly int scale;

        public NumberValidator(int precision, int scale = 0, bool onlyPositive = false)
        {
            this.precision = precision;
            this.scale = scale;
            this.onlyPositive = onlyPositive;
            if (precision <= 0)
                throw new ArgumentException("precision must be a positive number");
            if (scale < 0 || scale >= precision)
                throw new ArgumentException("precision must be a non-negative number less or equal than precision");
            numberRegex = new Regex(@"^([+-]?)(\d+)([.,](\d+))?$", RegexOptions.IgnoreCase);
        }

        public bool IsValidNumber(string value)
        {
            // Проверяем соответствие входного значения формату N(m,k), в соответствии с правилом, 
            // описанным в Формате описи документов, направляемых в налоговый орган в электронном виде по телекоммуникационным каналам связи:
            // Формат числового значения указывается в виде N(m.к), где m – максимальное количество знаков в числе, включая знак (для отрицательного числа), 
            // целую и дробную часть числа без разделяющей десятичной точки, k – максимальное число знаков дробной части числа. 
            // Если число знаков дробной части числа равно 0 (т.е. число целое), то формат числового значения имеет вид N(m).

            if (string.IsNullOrEmpty(value))
                return false;

            var match = numberRegex.Match(value);
            if (!match.Success)
                return false;

            // Знак и целая часть
            var intPart = match.Groups[1].Value.Length + match.Groups[2].Value.Length;
            // Дробная часть
            var fracPart = match.Groups[4].Value.Length;

            if (intPart + fracPart > precision || fracPart > scale)
                return false;

            if (onlyPositive && match.Groups[1].Value == "-")
                return false;
            return true;
        }
    }
}