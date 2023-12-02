namespace Tefin.Utils;

public static class PipeExtensions {

    /// <summary>
    /// Pipes 1 functions
    /// </summary>
    public static Void Then<A>(this A obj, Action func1) {
        func1();
        return new Void();
    }

    /// <summary>
    /// Pipes 1 functions
    /// </summary>
    public static Void Then<A>(this A obj, Action<A> func1) {
        func1(obj);
        return new Void();
    }

    /// <summary>
    /// Pipes 1 functions
    /// </summary>
    public static B Then<A, B>(this A obj, Func<B> func1) {
        return func1();
    }

    /// <summary>
    /// Pipes 1 functions
    /// </summary>
    public static B Then<A, B>(this A obj, Func<A, B> func1) {
        return func1(obj);
    }

    /// <summary>
    /// Pipes 2 functions
    /// </summary>
    public static C Then<A, B, C>(this A obj, Func<A, B> func1, Func<B, C> func2) {
        return func2(func1(obj));
    }

    /// <summary>
    /// Pipes 3 functions
    /// </summary>
    public static D Then<A, B, C, D>(this A obj, Func<A, B> func1, Func<B, C> func2, Func<C, D> func3) {
        return func3(func2(func1(obj)));
    }

    /// <summary>
    /// Pipes 4 functions
    /// </summary>
    public static E Then<A, B, C, D, E>(this A obj, Func<A, B> func1, Func<B, C> func2, Func<C, D> func3, Func<D, E> func4) {
        return func4(func3(func2(func1(obj))));
    }

    /// <summary>
    /// Pipes 5 functions
    /// </summary>
    public static F Then<A, B, C, D, E, F>(this A obj, Func<A, B> func1, Func<B, C> func2, Func<C, D> func3, Func<D, E> func4, Func<E, F> func5) {
        return func5(func4(func3(func2(func1(obj)))));
    }

    /// <summary>
    /// Pipes 6 functions
    /// </summary>
    public static G Then<A, B, C, D, E, F, G>(this A obj, Func<A, B> func1, Func<B, C> func2, Func<C, D> func3, Func<D, E> func4, Func<E, F> func5, Func<F, G> func6) {
        return func6(func5(func4(func3(func2(func1(obj))))));
    }

    /// <summary>
    /// Pipes 7 functions
    /// </summary>
    public static H Then<A, B, C, D, E, F, G, H>(this A obj, Func<A, B> func1, Func<B, C> func2, Func<C, D> func3, Func<D, E> func4, Func<E, F> func5, Func<F, G> func6,
        Func<G, H> func7) {
        return func7(func6(func5(func4(func3(func2(func1(obj)))))));
    }

    /// <summary>
    /// Pipes 8 functions
    /// </summary>
    public static I Then<A, B, C, D, E, F, G, H, I>(this A obj, Func<A, B> func1, Func<B, C> func2, Func<C, D> func3, Func<D, E> func4, Func<E, F> func5, Func<F, G> func6,
        Func<G, H> func7, Func<H, I> func8) {
        return func8(func7(func6(func5(func4(func3(func2(func1(obj))))))));
    }

    /// <summary>
    /// Pipes 9 functions
    /// </summary>
    public static J Then<A, B, C, D, E, F, G, H, I, J>(this A obj, Func<A, B> func1, Func<B, C> func2, Func<C, D> func3, Func<D, E> func4, Func<E, F> func5, Func<F, G> func6,
        Func<G, H> func7, Func<H, I> func8, Func<I, J> func9) {
        return func9(func8(func7(func6(func5(func4(func3(func2(func1(obj)))))))));
    }

    /// <summary>
    /// Pipes 10 functions
    /// </summary>
    public static K Then<A, B, C, D, E, F, G, H, I, J, K>(this A obj, Func<A, B> func1, Func<B, C> func2, Func<C, D> func3, Func<D, E> func4, Func<E, F> func5, Func<F, G> func6,
        Func<G, H> func7, Func<H, I> func8, Func<I, J> func9, Func<J, K> func10) {
        return func10(func9(func8(func7(func6(func5(func4(func3(func2(func1(obj))))))))));
    }

    /// <summary>
    /// Pipes 11 functions
    /// </summary>
    public static L Then<A, B, C, D, E, F, G, H, I, J, K, L>(this A obj, Func<A, B> func1, Func<B, C> func2, Func<C, D> func3, Func<D, E> func4, Func<E, F> func5, Func<F, G> func6,
        Func<G, H> func7, Func<H, I> func8, Func<I, J> func9, Func<J, K> func10, Func<K, L> func11) {
        return func11(func10(func9(func8(func7(func6(func5(func4(func3(func2(func1(obj)))))))))));
    }

    /// <summary>
    /// Pipes 12 functions
    /// </summary>
    public static M Then<A, B, C, D, E, F, G, H, I, J, K, L, M>(this A obj, Func<A, B> func1, Func<B, C> func2, Func<C, D> func3, Func<D, E> func4, Func<E, F> func5,
        Func<F, G> func6, Func<G, H> func7, Func<H, I> func8, Func<I, J> func9, Func<J, K> func10, Func<K, L> func11, Func<L, M> func12) {
        return func12(func11(func10(func9(func8(func7(func6(func5(func4(func3(func2(func1(obj))))))))))));
    }

    /// <summary>
    /// Pipes 13 functions
    /// </summary>
    public static N Then<A, B, C, D, E, F, G, H, I, J, K, L, M, N>(this A obj, Func<A, B> func1, Func<B, C> func2, Func<C, D> func3, Func<D, E> func4, Func<E, F> func5,
        Func<F, G> func6, Func<G, H> func7, Func<H, I> func8, Func<I, J> func9, Func<J, K> func10, Func<K, L> func11, Func<L, M> func12, Func<M, N> func13) {
        return func13(func12(func11(func10(func9(func8(func7(func6(func5(func4(func3(func2(func1(obj)))))))))))));
    }

    /// <summary>
    /// Pipes 14 functions
    /// </summary>
    public static O Then<A, B, C, D, E, F, G, H, I, J, K, L, M, N, O>(this A obj, Func<A, B> func1, Func<B, C> func2, Func<C, D> func3, Func<D, E> func4, Func<E, F> func5,
        Func<F, G> func6, Func<G, H> func7, Func<H, I> func8, Func<I, J> func9, Func<J, K> func10, Func<K, L> func11, Func<L, M> func12, Func<M, N> func13, Func<N, O> func14) {
        return func14(func13(func12(func11(func10(func9(func8(func7(func6(func5(func4(func3(func2(func1(obj))))))))))))));
    }

    /// <summary>
    /// Pipes 15 functions
    /// </summary>
    public static P Then<A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P>(this A obj, Func<A, B> func1, Func<B, C> func2, Func<C, D> func3, Func<D, E> func4, Func<E, F> func5,
        Func<F, G> func6, Func<G, H> func7, Func<H, I> func8, Func<I, J> func9, Func<J, K> func10, Func<K, L> func11, Func<L, M> func12, Func<M, N> func13, Func<N, O> func14,
        Func<O, P> func15) {
        return func15(func14(func13(func12(func11(func10(func9(func8(func7(func6(func5(func4(func3(func2(func1(obj)))))))))))))));
    }

    /// <summary>
    /// Pipes 16 functions
    /// </summary>
    public static Q Then<A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q>(this A obj, Func<A, B> func1, Func<B, C> func2, Func<C, D> func3, Func<D, E> func4, Func<E, F> func5,
        Func<F, G> func6, Func<G, H> func7, Func<H, I> func8, Func<I, J> func9, Func<J, K> func10, Func<K, L> func11, Func<L, M> func12, Func<M, N> func13, Func<N, O> func14,
        Func<O, P> func15, Func<P, Q> func16) {
        return func16(func15(func14(func13(func12(func11(func10(func9(func8(func7(func6(func5(func4(func3(func2(func1(obj))))))))))))))));
    }

    /// <summary>
    /// Pipes 17 functions
    /// </summary>
    public static R Then<A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R>(this A obj, Func<A, B> func1, Func<B, C> func2, Func<C, D> func3, Func<D, E> func4, Func<E, F> func5,
        Func<F, G> func6, Func<G, H> func7, Func<H, I> func8, Func<I, J> func9, Func<J, K> func10, Func<K, L> func11, Func<L, M> func12, Func<M, N> func13, Func<N, O> func14,
        Func<O, P> func15, Func<P, Q> func16, Func<Q, R> func17) {
        return func17(func16(func15(func14(func13(func12(func11(func10(func9(func8(func7(func6(func5(func4(func3(func2(func1(obj)))))))))))))))));
    }

    /// <summary>
    /// Pipes 18 functions
    /// </summary>
    public static S Then<A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S>(this A obj, Func<A, B> func1, Func<B, C> func2, Func<C, D> func3, Func<D, E> func4,
        Func<E, F> func5, Func<F, G> func6, Func<G, H> func7, Func<H, I> func8, Func<I, J> func9, Func<J, K> func10, Func<K, L> func11, Func<L, M> func12, Func<M, N> func13,
        Func<N, O> func14, Func<O, P> func15, Func<P, Q> func16, Func<Q, R> func17, Func<R, S> func18) {
        return func18(func17(func16(func15(func14(func13(func12(func11(func10(func9(func8(func7(func6(func5(func4(func3(func2(func1(obj))))))))))))))))));
    }

    /// <summary>
    /// Pipes 19 functions
    /// </summary>
    public static T Then<A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T>(this A obj, Func<A, B> func1, Func<B, C> func2, Func<C, D> func3, Func<D, E> func4,
        Func<E, F> func5, Func<F, G> func6, Func<G, H> func7, Func<H, I> func8, Func<I, J> func9, Func<J, K> func10, Func<K, L> func11, Func<L, M> func12, Func<M, N> func13,
        Func<N, O> func14, Func<O, P> func15, Func<P, Q> func16, Func<Q, R> func17, Func<R, S> func18, Func<S, T> func19) {
        return func19(func18(func17(func16(func15(func14(func13(func12(func11(func10(func9(func8(func7(func6(func5(func4(func3(func2(func1(obj)))))))))))))))))));
    }

    /// <summary>
    /// Pipes 20 functions
    /// </summary>
    public static U Then<A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U>(this A obj, Func<A, B> func1, Func<B, C> func2, Func<C, D> func3, Func<D, E> func4,
        Func<E, F> func5, Func<F, G> func6, Func<G, H> func7, Func<H, I> func8, Func<I, J> func9, Func<J, K> func10, Func<K, L> func11, Func<L, M> func12, Func<M, N> func13,
        Func<N, O> func14, Func<O, P> func15, Func<P, Q> func16, Func<Q, R> func17, Func<R, S> func18, Func<S, T> func19, Func<T, U> func20) {
        return func20(func19(func18(func17(func16(func15(func14(func13(func12(func11(func10(func9(func8(func7(func6(func5(func4(func3(func2(func1(obj))))))))))))))))))));
    }

    /// <summary>
    /// Pipes 21 functions
    /// </summary>
    public static V Then<A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V>(this A obj, Func<A, B> func1, Func<B, C> func2, Func<C, D> func3, Func<D, E> func4,
        Func<E, F> func5, Func<F, G> func6, Func<G, H> func7, Func<H, I> func8, Func<I, J> func9, Func<J, K> func10, Func<K, L> func11, Func<L, M> func12, Func<M, N> func13,
        Func<N, O> func14, Func<O, P> func15, Func<P, Q> func16, Func<Q, R> func17, Func<R, S> func18, Func<S, T> func19, Func<T, U> func20, Func<U, V> func21) {
        return func21(func20(func19(func18(func17(func16(func15(func14(func13(func12(func11(func10(func9(func8(func7(func6(func5(func4(func3(func2(func1(obj)))))))))))))))))))));
    }

    /// <summary>
    /// Pipes 22 functions
    /// </summary>
    public static W Then<A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W>(this A obj, Func<A, B> func1, Func<B, C> func2, Func<C, D> func3, Func<D, E> func4,
        Func<E, F> func5, Func<F, G> func6, Func<G, H> func7, Func<H, I> func8, Func<I, J> func9, Func<J, K> func10, Func<K, L> func11, Func<L, M> func12, Func<M, N> func13,
        Func<N, O> func14, Func<O, P> func15, Func<P, Q> func16, Func<Q, R> func17, Func<R, S> func18, Func<S, T> func19, Func<T, U> func20, Func<U, V> func21, Func<V, W> func22) {
        return func22(func21(
            func20(func19(func18(func17(func16(func15(func14(func13(func12(func11(func10(func9(func8(func7(func6(func5(func4(func3(func2(func1(obj))))))))))))))))))))));
    }

    /// <summary>
    /// Pipes 23 functions
    /// </summary>
    public static X Then<A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X>(this A obj, Func<A, B> func1, Func<B, C> func2, Func<C, D> func3, Func<D, E> func4,
        Func<E, F> func5, Func<F, G> func6, Func<G, H> func7, Func<H, I> func8, Func<I, J> func9, Func<J, K> func10, Func<K, L> func11, Func<L, M> func12, Func<M, N> func13,
        Func<N, O> func14, Func<O, P> func15, Func<P, Q> func16, Func<Q, R> func17, Func<R, S> func18, Func<S, T> func19, Func<T, U> func20, Func<U, V> func21, Func<V, W> func22,
        Func<W, X> func23) {
        return func23(func22(func21(
            func20(func19(func18(func17(func16(func15(func14(func13(func12(func11(func10(func9(func8(func7(func6(func5(func4(func3(func2(func1(obj)))))))))))))))))))))));
    }

    /// <summary>
    /// Pipes 24 functions
    /// </summary>
    public static Y Then<A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y>(this A obj, Func<A, B> func1, Func<B, C> func2, Func<C, D> func3,
        Func<D, E> func4, Func<E, F> func5, Func<F, G> func6, Func<G, H> func7, Func<H, I> func8, Func<I, J> func9, Func<J, K> func10, Func<K, L> func11, Func<L, M> func12,
        Func<M, N> func13, Func<N, O> func14, Func<O, P> func15, Func<P, Q> func16, Func<Q, R> func17, Func<R, S> func18, Func<S, T> func19, Func<T, U> func20, Func<U, V> func21,
        Func<V, W> func22, Func<W, X> func23, Func<X, Y> func24) {
        return func24(func23(func22(func21(
            func20(func19(func18(func17(func16(func15(func14(func13(func12(func11(func10(func9(func8(func7(func6(func5(func4(func3(func2(func1(obj))))))))))))))))))))))));
    }

    /// <summary>
    /// Pipes 25 functions
    /// </summary>
    public static Z Then<A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z>(this A obj, Func<A, B> func1, Func<B, C> func2, Func<C, D> func3,
        Func<D, E> func4, Func<E, F> func5, Func<F, G> func6, Func<G, H> func7, Func<H, I> func8, Func<I, J> func9, Func<J, K> func10, Func<K, L> func11, Func<L, M> func12,
        Func<M, N> func13, Func<N, O> func14, Func<O, P> func15, Func<P, Q> func16, Func<Q, R> func17, Func<R, S> func18, Func<S, T> func19, Func<T, U> func20, Func<U, V> func21,
        Func<V, W> func22, Func<W, X> func23, Func<X, Y> func24, Func<Y, Z> func25) {
        return func25(func24(func23(func22(func21(
            func20(func19(func18(func17(func16(func15(func14(func13(func12(func11(func10(func9(func8(func7(func6(func5(func4(func3(func2(func1(obj)))))))))))))))))))))))));
    }
}