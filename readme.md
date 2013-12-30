A ligthweight JSON serializer/deserializer in C#
================================================

It always outputs valid JSON but also:

  * allows comments
  * allows single quotes or double quotes
  * allows unquoted keys

unquoted keys and single quotes are convenient when embeding json in c# code.

  * DateTime is serialized as ISO 8601.
  * NotJsonSeralizable attribute to ignore properties
  * simple IJsonSerializable and IJsonDeserializable to customize serialization


Usage
-----

	Json.Deserialize("{ i: 3 }");

	Json.Deserialize<Person>("{ name: 'john' }");

	Json.Serialize(value);


Examples
--------

	# this is a comment
	# --------------------------
	{ 
		# this is another comment
		i: 3,
		# this is another comment
		'name': 'john'
	}

	{ 
		"car": { "seats": 4 } 
	}

	{ 
		'car': { 'seats': 4 } 
	}

	{ 
		car: { seats: 4 } 
	}


	Json.Serialize(new DateTime(2014, 1, 1, 12, 0, 0))

returns:

	"2014-01-01T12:00:00"



