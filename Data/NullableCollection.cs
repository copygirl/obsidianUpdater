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

		public static IEnumerable<T> operator ~(NullableCollection<T> collection)
		{
			return ((collection != null) ? collection : Enumerable.Empty<T>());
		}
	}
}

