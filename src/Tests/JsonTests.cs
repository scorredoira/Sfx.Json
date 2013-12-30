using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace Sfx.JSON.Tests
{
	[TestFixture]
    static class JsonTests
    {
        [Test]
        public static void DeserializeInt()
        {
            var item = Json.Deserialize("{ i: 3 }") as Dictionary<string, object>;
            Assert.AreEqual(item["i"], 3);
        }

        [Test]
        public static void DeserializeBoolean()
        {
			var item = Json.Deserialize("{ i: true }") as Dictionary<string, object>;
			Assert.AreEqual(item["i"], true);
		}

		[Test]
		public static void DeserializeStaticType()
		{
			var person = Json.Deserialize<Person>("{ Name: 'john', Age: 18 }");
			Assert.AreEqual(person.Name, "john");
			Assert.AreEqual(person.Age, 18);
		}
		
        [Test]
        public static void DeserializeDateTime()
		{
			var now = new DateTime(2012, 4, 1, 20, 0, 0, 0);
			var json = Json.Serialize(now);
			Assert.AreEqual(Json.Deserialize<DateTime>(json), now);
		}

        [Test]
        public static void DeserializeNestedObject()
        {
			var item = Json.Deserialize("{ car: { seats: 2 } }") as Dictionary<string, object>;
            var car = item["car"] as IDictionary;
            Assert.AreEqual(car["seats"], 2);
        }

        [Test]
        public static void Deserialize2()
        {
            var item = Json.Deserialize("[2,4]") as IList;
            Assert.AreEqual(item.Count, 2);
            Assert.AreEqual(item[0], 2);
            Assert.AreEqual(item[1], 4);
        }

        [Test]
        public static void TrimBlankChars()
        {
			var item = Json.Deserialize("\r\n\t\t\t{\r\n\t car:\r\n\t {\r\n\t seats: \r\n\t2 } }") as Dictionary<string, object>;
            var car = item["car"] as IDictionary;
            Assert.AreEqual(car["seats"], 2);
        }

        [Test]
        public static void ComplexCollection()
        {
            var items = new List<object>();
            items.Add(5);
            items.Add("hello");
            items.Add(new { number = 3 });
            items.Add(new Dictionary<string, object>() { { "file", "hello.jpg" } });
            var serialized = Json.Serialize(items);

            var deserializedItems = Json.Deserialize(serialized, items.GetType()) as List<object>;
            Assert.AreEqual(deserializedItems.Count, 4);
            Assert.AreEqual(deserializedItems[0], 5);
            Assert.AreEqual(deserializedItems[1], "hello");
            Assert.AreEqual(((IDictionary)deserializedItems[2])["number"], 3);
            Assert.AreEqual(((IDictionary)deserializedItems[3])["file"], "hello.jpg");
        }
		
        [Test]
        public static void Test4()
		{
			var item = Json.Deserialize("{ i: 3, a: { b: 2 }, d: [5,6,8] }") as Dictionary<string, object>;
			Assert.AreEqual(item["i"], 3);

            var a = item["a"] as IDictionary;
			Assert.AreEqual(a["b"], 2);

            var d = item["d"] as IList;
			Assert.AreEqual(d[2], 8);
		}

        [Test]
        public static void TestSingleQuotes()
        {
			var item = Json.Deserialize("{ a: '3' }") as Dictionary<string, object>;
            Assert.AreEqual(item["a"], "3");
		}		

		[Test]
		public static void DeserializeMap()
		{
			var map = Json.Deserialize<Dictionary<string, object>>("{ name: 'john' }");
			Assert.AreEqual(map["name"], "john");
		}

		[Test]
		public static void SerializeDate()
		{
			var item = Json.Serialize(new DateTime(2014, 1, 1, 12, 0, 0));
			Assert.AreEqual("\"2014-01-01T12:00:00\"", item);
		}

		[Test]
		public static void DeserializeDate()
		{
			var item = Json.Deserialize<DateTime>("\"2014-01-01T12:00:00\"");
			Assert.AreEqual(item, new DateTime(2014, 1, 1, 12, 0, 0));
		}

		[Test]
		public static void IgnoreComments()
		{
			var json = @"
				# this is a comment
				# --------------------------
				{ 
					# this is another comment
					i: 3,
					# this is another comment
					'name': 'john'
				}
			";

			var item = Json.Deserialize<Dictionary<string,object>>(json);
			Assert.AreEqual(item["i"], 3);
			Assert.AreEqual(item["name"], "john");
		}

		class Person
		{
			public string Name { get; set; }
			public int Age { get; set; }
		}
    }
}
















































