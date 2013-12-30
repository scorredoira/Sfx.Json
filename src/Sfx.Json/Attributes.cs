using System;

namespace Sfx.JSON
{
	/// <summary>
	/// Allows to ignore properties
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class NotJsonSeralizableAttribute : Attribute
	{
	}    

	/// <summary>
	/// Allows to customize serialization
	/// </summary>
	public interface IJsonSerializable
	{
		string Serialize(bool minimice);
	}

	/// <summary>
	/// Allows to customize deserialization
	/// </summary>
	public interface IJsonDeserializable
	{
		object Deserialize(object json);
	}
}

