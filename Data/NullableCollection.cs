using System;
using System.Collections.Generic;
using System.Linq;

namespace obsidianUpdater.Data
{
	public class NullableCollection<T> : HashSet<T>
	{
		public NullableCollection()
		{
		}
		public NullableCollection(IEnumerable<T> collection)
			: base((collection != null) ? collection : Enumerable.Empty<T>())
		{
		}

		public static NullableCollection<T> operator +(NullableCollection<T> collection, T item)
		{
			return new NullableCollection<T>(collection){ item };
		}

		public static NullableCollection<T> operator -(NullableCollection<T> collection, T item)
		{
			var newCollection = new NullableCollection<T>(collection);
			newCollection.Remove(item);
			return ((newCollection.Count > 0) ? newCollection : null);
		}

		public static NullableCollection<string> FromString(string str, char seperator = ',')
		{
			return (!String.IsNullOrEmpty(str) ? new NullableCollection<string>(str.Split(',')) : null);
		}
	}

	public static class NullableCollectionExtensions
	{
		public static IEnumerable<T> AsEnumerable<T>(this NullableCollection<T> collection)
		{
			return ((collection != null) ? collection : Enumerable.Empty<T>());
		}
	}
}

