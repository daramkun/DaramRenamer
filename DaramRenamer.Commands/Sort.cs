using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DaramRenamer;

internal static class Sort
{
    public static void Quicksort<T>(IList<T> arr) where T : IComparable<T>
    {
        Quicksort(arr, 0, arr.Count - 1);
    }

    private static void Quicksort<T>(IList<T> arr, int left, int right) where T : IComparable<T>
    {
        if (right <= left)
            return;

        var pivot = Partition(arr, left, right);
        Quicksort(arr, left, pivot - 1);
        Quicksort(arr, pivot + 1, right);
    }

    public static void QuicksortParallel<T>(IList<T> arr) where T : IComparable<T>
    {
        QuicksortParallel(arr, 0, arr.Count - 1);
    }

    private static void QuicksortParallel<T>(IList<T> arr, int left, int right) where T : IComparable<T>
    {
        if (right <= left)
            return;

        var pivot = Partition(arr, left, right);
        Parallel.Invoke(
            () => QuicksortParallel(arr, left, pivot - 1),
            () => QuicksortParallel(arr, pivot + 1, right)
        );
    }

    private static void Swap<T>(IList<T> a, int i, int j)
    {
        var t = a[i];
        a[i] = a[j];
        a[j] = t;
    }

    private static int Partition<T>(IList<T> arr, int low, int high) where T : IComparable<T>
    {
        int pivotPos = (high + low) / 2, left = low;
        var pivot = arr[pivotPos];
        Swap(arr, low, pivotPos);
        for (var i = low + 1; i <= high; i++)
            if (arr[i].CompareTo(pivot) < 0)
                Swap(arr, i, ++left);
        Swap(arr, low, left);
        return left;
    }
}