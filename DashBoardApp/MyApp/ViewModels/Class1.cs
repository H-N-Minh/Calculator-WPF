using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;

namespace DashBoardApp.ViewModels;

public class ObservableHashSet<T> : ISet<T>, INotifyCollectionChanged
{
    private readonly ISet<T> set;
    public ObservableHashSet(ISet<T> set)
    {
        this.set = set;
    }

    public int Count => set.Count;

    public bool IsReadOnly => set.IsReadOnly;

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public bool Add(T item)
    {
        bool ret = set.Add(item);
        if (ret)
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }
        return ret;
    }

    public void Clear()
    {
        set.Clear();
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public bool Contains(T item)
    {
        return set.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        set.CopyTo(array, arrayIndex);
    }

    public void ExceptWith(IEnumerable<T> other)
    {
        set.ExceptWith(other);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return set.GetEnumerator();
    }

    public void IntersectWith(IEnumerable<T> other)
    {
        set.IntersectWith(other);
    }

    public bool IsProperSubsetOf(IEnumerable<T> other)
    {
        return set.IsProperSubsetOf(other);
    }

    public bool IsProperSupersetOf(IEnumerable<T> other)
    {
        return set.IsProperSupersetOf(other);
    }

    public bool IsSubsetOf(IEnumerable<T> other)
    {
        return set.IsSubsetOf(other);
    }

    public bool IsSupersetOf(IEnumerable<T> other)
    {
        return set.IsSupersetOf(other);
    }

    public bool Overlaps(IEnumerable<T> other)
    {
        return set.Overlaps(other);
    }

    public bool Remove(T item)
    {
        return set.Remove(item);
    }

    public bool SetEquals(IEnumerable<T> other)
    {
        return set.SetEquals(other);
    }

    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        set.SymmetricExceptWith(other);
    }

    public void UnionWith(IEnumerable<T> other)
    {
        List<T> newItems;
        if (other.TryGetNonEnumeratedCount(out var count))
        {
            newItems = new List<T>(count);
        }
        else
        {
            newItems = [];
        }


        foreach (T item in other)
        {
            if (set.Add(item))
            {
                newItems.Add(item);
            }
        }

        if (newItems.Count > 0)
        {
            NotifyCollectionChangedEventArgs arg = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (IList) newItems);
            CollectionChanged?.Invoke(this, arg);
        }
    }

    void ICollection<T>.Add(T item)
    {
        ((ICollection<T>)set).Add(item);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)set).GetEnumerator();
    }
}
