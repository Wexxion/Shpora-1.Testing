using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Challenge
{
	[TestFixture]
	public class WordsStatistics_Tests
	{
		public static string Authors = "Лифанов Гладышева"; // "Egorov Shagalina"

		public virtual IWordsStatistics CreateStatistics()
		{
			// меняется на разные реализации при запуске exe
			return new WordsStatistics();
		}

		private IWordsStatistics statistics;

		[SetUp]
		public void SetUp()
		{
			statistics = CreateStatistics();
		}

	    [Test]
	    public void Space()
	    {
	        statistics.AddWord(" ");
	        statistics.GetStatistics().Should().BeEmpty();
        }

	    [Test]
	    public void Both()
	    {
	        statistics.AddWord("          a");
	        var a = statistics.GetStatistics().Should().Equal(Tuple.Create(1, "          "));
	    }


	    [Test]
	    public void Again()
	    {
	        statistics.AddWord("a");
	        statistics.GetStatistics().Should().Equal(Tuple.Create(1, "a"));
	        statistics.AddWord("a");
	        statistics.GetStatistics().Should().Equal(Tuple.Create(2, "a"));
        }

	    [Test, Timeout(190)]
	    public void Crash123()
	    {
	        for (var i = 0; i < 12500; i++)
	            statistics.AddWord($"a{i}");
	        statistics.GetStatistics().Should().HaveCount(12500);
	    }

	    [Test]
	    public void CrashSTA()
	    {
	        statistics.AddWord("a");
	        var newStat = CreateStatistics();
	        statistics.GetStatistics().Should().HaveCount(1);
	    }
	    [Test, Timeout(2500)]
	    public void BigTest1()
	    {
	        for (var i = 0; i < 900000; i++)
	            statistics.AddWord($"{i}{i}{i}{i}{i}{i}");
	    }

        [Test]
	    public void OrderCount()
	    {
	        statistics.AddWord("kek");
	        statistics.AddWord("lol");
	        statistics.AddWord("lol");
	        statistics.GetStatistics().ShouldAllBeEquivalentTo(new[]{
	            Tuple.Create(2, "lol"), Tuple.Create(1, "kek")}, format => format.WithStrictOrdering());
        }

	    [Test]
	    public void Exception()
	    {
	        Action act = () => statistics.AddWord(null);
	        act.ShouldThrow<ArgumentNullException>();
	    }


	    [Test]
	    public void Order()
	    {
	        statistics.AddWord("Lol");
	        statistics.AddWord("Kek");
	        statistics.GetStatistics().ShouldAllBeEquivalentTo(new []{
	            Tuple.Create(1, "kek"), Tuple.Create(1, "lol")}, format => format.WithStrictOrdering());
        }

		[Test]
		public void GetStatistics_IsEmpty_AfterCreation()
		{
			statistics.GetStatistics().Should().BeEmpty();
		}

		[Test]
		public void GetStatistics_ContainsItem_AfterAddition()
		{
			statistics.AddWord("abc");
			statistics.GetStatistics().Should().Equal(Tuple.Create(1, "abc"));
		}

		[Test]
		public void GetStatistics_ContainsManyItems_AfterAdditionOfDifferentWords()
		{
			statistics.AddWord("abc");
			statistics.AddWord("def");
			statistics.GetStatistics().Should().HaveCount(2);
		}


		// Документация по FluentAssertions с примерами : https://github.com/fluentassertions/fluentassertions/wiki
	}
}