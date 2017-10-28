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
	    public void When_InvalidArguments_ShouldThrow_ArgumentException_DepandsOn_Precision
            (int precision, int scale, bool onlyPositive)
	    {
	        Action act = () => new NumberValidator(precision, scale, onlyPositive);
	        act.ShouldThrow<ArgumentException>().WithMessage("*positive*");
        }

	    [TestCase(1, -1, true, TestName = "Scale <= 0 and positive")]
	    [TestCase(1, -1, false, TestName = "Scale <= 0 and negative")]
	    [TestCase(1, 2, true, TestName = "Scale >= Precision and positive")]
	    [TestCase(1, 2, false, TestName = "Scale >= Precision and negative")]
	    public void When_InvalidArguments_ShouldThrow_ArgumentException_DepandsOn_Scale
	        (int precision, int scale, bool onlyPositive)
	    {
	        Action act = () => new NumberValidator(precision, scale, onlyPositive);
	        act.ShouldThrow<ArgumentException>().WithMessage("*non-negative*");
	    }

	    private static IEnumerable<(int precision, int scale, bool onlyPositive, string value, bool expected)> 
            GetValidSourse() => new []
	    {
	        (3, 2, true, "", false),
	        (3, 2, true, null, false),
	        (3, 2, true, "0.0", true),
	        (3, 2, true, "+0.0", true),
	        (3, 2, true, "0.00", true),
	        (3, 2, true, "0.000", false),
	        (3, 2, true, "-0.0", false),
	        (3, 2, false, "-0.0", true),
	        (3, 2, false, "--0", false),
	        (3, 2, false, "0.-0", false),
	        (3, 2, true, "l.ol", false)
	    };

        [Test, TestCaseSource("TestCasesForValidArgsTest")]
	    public bool When_ValidArguments_Should_WorkCorrectly
	        (int precision, int scale, bool onlyPositive, string value)
        {
	        return new NumberValidator(precision, scale, onlyPositive).IsValidNumber(value);
	    }
        
	    private static IEnumerable TestCasesForValidArgsTest()
	    {
	        foreach (var testCase in GetValidSourse())
	        {
	            var (precision, scale, onlyPositive, value, expected) = testCase;
                yield return new TestCaseData(precision, scale, onlyPositive, value)
                    .Returns(expected)
                    .SetName($"return {expected} when m,k={precision},{scale}, " +
                                $"{(onlyPositive ? "positive" : "negative")} nums " +
                                $"and value = {value}");
	        }
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