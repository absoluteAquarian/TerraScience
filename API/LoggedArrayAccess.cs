using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Terraria;

namespace TerraScience.API{
	internal class LoggedArrayAccess<T> : IEnumerable<T>{
		private T[] values;

		private static readonly HashSet<string> knownStackTraces = new HashSet<string>();

		public T[] Values{
			[MethodImpl(MethodImplOptions.NoInlining)]
			get{
				ReportTrace(Environment.StackTrace);
				return values;
			}
			[MethodImpl(MethodImplOptions.NoInlining)]
			set{
				ReportTrace(Environment.StackTrace);
				values = value;
			}
		}

		public int Length => values.Length;

		public ref T this[int index]{
			[MethodImpl(MethodImplOptions.NoInlining)]
			get{
				ReportTrace(Environment.StackTrace);
				return ref values[index];
			}
		}

		public LoggedArrayAccess(T[] arr)
			=> values = arr;

		private static void ReportTrace(string trace){
			if(knownStackTraces.Add(trace))
				TechMod.Instance.Logger.Debug($"Stack trace found for accessing LoggedArrayAccess<{typeof(T).FullName}>" +
					"\n" + trace);
		}

		public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)Values).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => Values.GetEnumerator();

		public static implicit operator LoggedArrayAccess<T>(T[] arr) => new LoggedArrayAccess<T>(arr);

		public static implicit operator T[](LoggedArrayAccess<T> log) => log.Values;
	}
}
