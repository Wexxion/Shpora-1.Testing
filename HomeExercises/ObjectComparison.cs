using FluentAssertions;
using NUnit.Framework;

namespace HomeExercises
{
    public class ObjectComparison
    {
        [Test]
        [Description("Проверка текущего царя")]
        [Category("ToRefactor")]
        public void CheckCurrentTsar()
        {
            var actualTsar = TsarRegistry.GetCurrentTsar();

            var expectedTsar = new Person("Ivan IV The Terrible", 54, 170, 70,
                new Person("Vasili III of Russia", 28, 170, 60, null));

            actualTsar.ShouldBeEquivalentTo(expectedTsar,
                options => options
                    .Excluding(obj => obj.SelectedMemberInfo.DeclaringType == typeof(Person)
                                      && obj.SelectedMemberInfo.Name == "Id"));
        }

        [Test]
        [Description("Альтернативное решение. Какие у него недостатки?")]
        public void CheckCurrentTsar_WithCustomEquality()
        {
            var actualTsar = TsarRegistry.GetCurrentTsar();
            var expectedTsar = new Person("Ivan IV The Terrible", 54, 170, 70,
                new Person("Vasili III of Russia", 28, 170, 60, null));

            // Какие недостатки у такого подхода? 
            Assert.True(AreEqual(actualTsar, expectedTsar));

            // Ну во-первых, хочется заметить что было бы круто, если метод AreEqual 
            // запихнуть в сам класс Person, при этом сделав override Equals, ну и для
            // полного соответствия канонам переопределить GetHashCode(). Ведь в будущем
            // мы можем использовать наш класс Person к примеру в словарях, тогда то и 
            // пригодятся нам методы Equals() и GetHashCode(). 

            // Было бы вообще круто если мы сделали наш класс Person Value-типом (тот что из DDD)
            // Тогда нам бы вообще не пришлось реализовывать Equals, GetHashCode и ToString, ведь
            // в нашем потенциальном проекте может быть не только 1 Value-тип. Таким образом мы 
            // можем избавиться от затратного по времени определения методов Equals, GetHashCode и ToString.

            // Допустим мы добавили новое поле в класс Person, fluent сделает все верно, учтет новые поля,
            // а вот в альтернативном решение придется добавить строчки для обработки этих полей.
            // Так же, в альтернативном решении не будет говориться где произошла ошибка, будет протсо
            // выведено: было false а ожидалось true. Fluent выведет подробную информацию, что поможет быстро 
            // найти ошибку.
        }

        private bool AreEqual(Person actual, Person expected)
        {
            if (actual == expected) return true;
            if (actual == null || expected == null) return false;
            return
                actual.Name == expected.Name
                && actual.Age == expected.Age
                && actual.Height == expected.Height
                && actual.Weight == expected.Weight
                && AreEqual(actual.Parent, expected.Parent);
        }
    }

    public class TsarRegistry
    {
        public static Person GetCurrentTsar()
        {
            return new Person(
                "Ivan IV The Terrible", 54, 170, 70,
                new Person("Vasili III of Russia", 28, 170, 60, null));
        }
    }

    public class Person
    {
        public static int IdCounter;
        public int Age, Height, Weight;
        public int Id;
        public string Name;
        public Person Parent;

        public Person(string name, int age, int height, int weight, Person parent)
        {
            Id = IdCounter++;
            Name = name;
            Age = age;
            Height = height;
            Weight = weight;
            Parent = parent;
        }
    }
}