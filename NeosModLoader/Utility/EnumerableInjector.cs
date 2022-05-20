using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeosModLoader.Utility
{
    /// <summary>
    /// Provides the ability to inject actions into the execution of an enumeration while transforming it.
    /// </summary>
    /// <typeparam name="TIn">The type of the original enumeration.</typeparam>
    /// <typeparam name="TOut">The type of the transformed enumeration.</typeparam>
    public class EnumerableInjector<TIn, TOut> : IEnumerable<TOut>
    {
        /// <summary>
        /// Internal enumerator for iteration.
        /// </summary>
        private readonly IEnumerator<TIn> enumerator;

        private Action postfix = nothing;
        private Action<TIn, TOut, bool> postItem = nothing;
        private Action prefix = nothing;
        private Func<TIn, bool> preItem = yes;
        private Func<TIn, TOut> transformItem;

        /// <summary>
        /// Gets called when the wrapped enumeration returned the last item.
        /// </summary>
        public Action Postfix
        {
            get => postfix;
            set => postfix = value ?? throw new ArgumentNullException(nameof(value), "Postfix can't be null!");
        }

        /// <summary>
        /// Gets called for each item, with the transformed item, and whether it was passed through.
        /// First thing to be called after execution returns to the enumerator after a yield return.
        /// </summary>
        public Action<TIn, TOut, bool> PostItem
        {
            get => postItem;
            set => postItem = value ?? throw new ArgumentNullException(nameof(value), "PostItem can't be null!");
        }

        /// <summary>
        /// Gets called before the enumeration returns the first item.
        /// </summary>
        public Action Prefix
        {
            get => prefix;
            set => prefix = value ?? throw new ArgumentNullException(nameof(value), "Prefix can't be null!");
        }

        /// <summary>
        /// Gets called for each item to determine whether it should be passed through.
        /// </summary>
        public Func<TIn, bool> PreItem
        {
            get => preItem;
            set => preItem = value ?? throw new ArgumentNullException(nameof(value), "PreItem can't be null!");
        }

        /// <summary>
        /// Gets called for each item to transform it, even if it won't be passed through.
        /// </summary>
        public Func<TIn, TOut> TransformItem
        {
            get => transformItem;
            set => transformItem = value ?? throw new ArgumentNullException(nameof(value), "TransforItem can't be null!");
        }

        /// <summary>
        /// Creates a new instance of the <see cref="EnumerableInjector{TIn, TOut}"/> class using the supplied input enumerable and transform function.
        /// </summary>
        /// <param name="enumerable">The enumerable to inject into and transform.</param>
        /// <param name="transformItem">The transformation function.</param>
        public EnumerableInjector(IEnumerable<TIn> enumerable, Func<TIn, TOut> transformItem)
            : this(enumerable.GetEnumerator(), transformItem)
        { }

        /// <summary>
        /// Creates a new instance of the <see cref="EnumerableInjector{TIn, TOut}"/> class using the supplied input enumerator and transform function.
        /// </summary>
        /// <param name="enumerable">The enumerator to inject into and transform.</param>
        /// <param name="transformItem">The transformation function.</param>
        public EnumerableInjector(IEnumerator<TIn> enumerator, Func<TIn, TOut> transformItem)
        {
            this.enumerator = enumerator;
            TransformItem = transformItem;
        }

        /// <summary>
        /// Injects into and transforms the input enumeration.
        /// </summary>
        /// <returns>The injected and transformed enumeration.</returns>
        public IEnumerator<TOut> GetEnumerator()
        {
            Prefix();

            while (enumerator.MoveNext())
            {
                var item = enumerator.Current;
                var returnItem = PreItem(item);
                var transformedItem = TransformItem(item);

                if (returnItem)
                    yield return transformedItem;

                PostItem(item, transformedItem, returnItem);
            }

            Postfix();
        }

        /// <summary>
        /// Injects into and transforms the input enumeration without a generic type.
        /// </summary>
        /// <returns>The injected and transformed enumeration without a generic type.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private static void nothing()
        { }

        private static void nothing(TIn _, TOut __, bool ___)
        { }

        private static bool yes(TIn _) => true;
    }

    /// <summary>
    /// Provides the ability to inject actions into the execution of an enumeration without transforming it.
    /// </summary>
    /// <typeparam name="T">The type of the enumeration.</typeparam>
    public class EnumerableInjector<T> : EnumerableInjector<T, T>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="EnumerableInjector{T}"/> class using the supplied input enumerable.
        /// </summary>
        /// <param name="enumerable">The enumerable to inject into.</param>
        public EnumerableInjector(IEnumerable<T> enumerable)
            : this(enumerable.GetEnumerator())
        { }

        /// <summary>
        /// Creates a new instance of the <see cref="EnumerableInjector{T}"/> class using the supplied input enumerator.
        /// </summary>
        /// <param name="enumerable">The enumerator to inject into.</param>
        public EnumerableInjector(IEnumerator<T> enumerator)
            : base(enumerator, identity)
        { }

        private static T identity(T item) => item;
    }
}