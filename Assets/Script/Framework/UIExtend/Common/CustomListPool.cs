

using System.Collections.Generic;

namespace UnityEngine.UI
{
	internal static class CustomListPool<T>
	{
		private static readonly CustomObjectPool<List<T>> s_ListPool = new CustomObjectPool<List<T>>(null, l => l.Clear());

		public static List<T> Get()
		{
			return s_ListPool.Get();
		}

		public static void Release(List<T> toRelease)
		{
			s_ListPool.Release(toRelease);
		}
	}

}

