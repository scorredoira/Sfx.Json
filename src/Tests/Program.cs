using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Sfx.JSON.Tests
{
	public class Program
	{
		public static void Main(string[] args)
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
	}
}

