using System;
using System.Collections;
using System.Collections.Generic;

public class SortedTupleBag<TKey, TValue> : SortedSet<Tuple<TKey, TValue>>
where TKey : IComparable
{
    private class TupleComparer : Comparer<Tuple<TKey, TValue>>
    {
        public override int Compare(Tuple<TKey, TValue> x, Tuple<TKey, TValue> y)
        {
            if (x == null || y == null) 
                return 0;
            
            return x.Item1.Equals(y.Item1) ? 1 : Comparer<TKey>.Default.Compare(x.Item1, y.Item1);
        }
    }
    public SortedTupleBag() : base(new TupleComparer()) { }
    public void Add(TKey key, TValue value)
    {
        Add(new Tuple<TKey, TValue>(key, value));
    }
}
