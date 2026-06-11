using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public static class IEnumerableExtensions
{
	public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
	{
		HashSet<TKey> seenKeys = new HashSet<TKey>();
		foreach (TSource item in source)
		{
			if (seenKeys.Add(keySelector(item)))
			{
				yield return item;
			}
		}
	}

	public static IList<int> AllIndexOf(this string text, string str, bool standardizeUpperCase = true, StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase)
	{
		IList<int> list = new List<int>();
		string text2 = (standardizeUpperCase ? text.ToUpper() : text);
		string text3 = (standardizeUpperCase ? str.ToUpper() : str);
		for (int num = text2.IndexOf(text3, comparisonType); num != -1; num = text2.IndexOf(text3, num + text3.Length, comparisonType))
		{
			list.Add(num);
		}
		return list;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int BinarySearchBy<TValue, TComparison>(this List<TValue> list, TComparison value, Func<TValue, TComparison> getter) where TComparison : IComparable<TComparison>
	{
		return list.AsSpan().BinarySearchBy(value, getter);
	}

	public static int BinarySearchBy<TValue, TComparison>(this Span<TValue> span, TComparison value, Func<TValue, TComparison> getter) where TComparison : IComparable<TComparison>
	{
		int num = 0;
		int num2 = span.Length - 1;
		int num3 = 0;
		while (num <= num2)
		{
			num3 = (num + num2) / 2;
			TComparison other = getter(span[num3]);
			int num4 = value.CompareTo(other);
			if (num4 < 1)
			{
				if (num4 == 0)
				{
					return num3;
				}
				num2 = num3 - 1;
			}
			else
			{
				num = num3 + 1;
			}
		}
		return ~num3;
	}

	public static int CountNoAlloc<T>(this List<T> list, Func<T, bool> predicate)
	{
		Span<T> span = list.AsSpan();
		int num = 0;
		int length = span.Length;
		for (int i = 0; i < length; i++)
		{
			if (predicate(span[i]))
			{
				num++;
			}
		}
		return num;
	}
}
