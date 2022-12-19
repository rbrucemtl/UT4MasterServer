﻿using MongoDB.Bson.Serialization;

namespace UT4MasterServer
{
	public static partial class Program
	{
		public class EpicIDSerializationProvider : IBsonSerializationProvider
		{
			public IBsonSerializer GetSerializer(Type type)
			{
				if (type == typeof(EpicID))
					return new EpicIDSerializer();
				return null;
			}
		}

#if true
#endif
	}
}