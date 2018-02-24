using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Configure
{
    class AsyncIterator<T>
    {
		private readonly IEnumerator<T> enumerator;
		private readonly Func<T, T> coercer;
		private readonly Func<T, bool> matcher;

		private T current;
		private bool hasCurrent;

		public AsyncIterator(IEnumerable<T> enumerable, Func<T, T> coercer, Func<T, bool> matcher)
			: this(enumerable.GetEnumerator())
		{
			this.coercer = coercer;
			this.matcher = matcher;
		}

		public AsyncIterator(IEnumerable<T> enumerable)
			: this(enumerable.GetEnumerator())
		{
		}

		public AsyncIterator(IEnumerator<T> enumerator)
		{
			this.enumerator = enumerator;
			this.coercer = x => x;
			this.matcher = x => true;
		}

		public T Current
		{
			get { return current; }
		}

		public bool HasCurrent
		{
			get { return hasCurrent; }
		}

		public async Task<AsyncIterator<T>> NextAsync()
		{
			do
			{
				hasCurrent = await Task.Run(() => enumerator.MoveNext());
				if (hasCurrent)
				{
					current = coercer(enumerator.Current);
				}
			}
			while (hasCurrent && !matcher(current));

			return this;
		}
    }
}
