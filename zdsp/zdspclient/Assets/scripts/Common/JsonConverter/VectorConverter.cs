using UnityEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

namespace Newtonsoft.Json
{
	public class Vector3Converter : JsonConverter
	{
		public override bool CanConvert (Type objectType)
		{
			return objectType == typeof(Vector3);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JObject jObject = JObject.Load(reader);
			return new Vector3((float)jObject ["x"], (float)jObject ["y"], (float)jObject ["z"]);
		}
		
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			Vector3 v = (Vector3)value;
			writer.WriteStartObject();
			writer.WritePropertyName("x");
			serializer.Serialize(writer, v.x);
			writer.WritePropertyName("y");
			serializer.Serialize(writer, v.y);
			writer.WritePropertyName("z");
			serializer.Serialize(writer, v.z);
			writer.WriteEndObject();
		}
	}
}

