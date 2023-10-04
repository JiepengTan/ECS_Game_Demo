/* C# Program for Bitonic Sort. Note that this program
works only when size of input is a power of 2. */

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

/*The parameter dir indicates the sorting direction, ASCENDING
or DESCENDING; if (a[i] > a[j]) agrees with the direction,
then a[i] and a[j] are interchanged.*/
class GFG
{
    /* To swap values */
    static void Swap<T>(ref T lhs, ref T rhs)
    {
        T temp;
        temp = lhs;
        lhs = rhs;
        rhs = temp;
    }

    public static void compAndSwap(int[] a, int i, int j, int dir)
    {
        int k;
        if ((a[i] > a[j]))
            k = 1;
        else
            k = 0;
        if (dir == k)
            Swap(ref a[i], ref a[j]);
    }

    /*It recursively sorts a bitonic sequence in ascending order,
    if dir = 1, and in descending order otherwise (means dir=0).
    The sequence to be sorted starts at index position low,
    the parameter cnt is the number of elements to be sorted.*/
    public static void bitonicMerge(int[] a, int low, int cnt, int dir)
    {
        if (cnt > 1)
        {
            int k = cnt / 2;
            for (int i = low; i < low + k; i++)
                compAndSwap(a, i, i + k, dir);
            bitonicMerge(a, low, k, dir);
            bitonicMerge(a, low + k, k, dir);
        }
    }

    /* This function first produces a bitonic sequence by recursively
        sorting its two halves in opposite sorting orders, and then
        calls bitonicMerge to make them in the same order */
    public static void bitonicSort(int[] a, int low, int cnt, int dir)
    {
        if (cnt > 1)
        {
            int k = cnt / 2;

            // sort in ascending order since dir here is 1
            bitonicSort(a, low, k, 1);

            // sort in descending order since dir here is 0
            bitonicSort(a, low + k, k, 0);

            // Will merge whole sequence in ascending order
            // since dir=1.
            bitonicMerge(a, low, cnt, dir);
        }
    }

    /* Caller of bitonicSort for sorting the entire array of
    length N in ASCENDING order */
    public static void sort(int[] a, int N, int up)
    {
        bitonicSort(a, 0, N, up);
    }




    public static void sort2(int[] ary, int N, int up)
    {
        int[] _SharedData= new int[N];
        void CopyData() { for (int i = 0; i < ary.Length; i++) { _SharedData[i] = ary[i];} }
        for (int _Level = 2; _Level <= N; _Level <<= 1)// 2 4 8
        {
            int _LevelMask = _Level;
            // Sort the row data
            for (int j = _Level >> 1; j > 0; j >>= 1) // 1 // 2,1 // 4,2,1
            {
                CopyData();
                for (int GI = 0; GI < N; GI++)
                {
                    int tID = GI;
                    var result = _SharedData[GI];
                    var inst1 = _SharedData[GI & ~j];// 提取左边的元素
                    var inst2 = _SharedData[GI | j]; // 提取右边的元素
                    float dist1 = inst1;
                    float dist2 = inst2;
                    if ((dist1 <= dist2) == ((_LevelMask & tID) !=0)) // _LevelMask & tID 表示当前是不是降序 的part   ， _LevelMask == level = j*2
                    {
                        result = _SharedData[GI ^ j];//左=>右  右=>左  // swap
                    }
                    ary[GI] = result;
                }
            }
        }
    }
}


public class TestBitonicSort : MonoBehaviour
{
    public int count = 128;
    private void Start()
    {

        Random.InitState(12);
        int[] nums = new int[count];
        for (int i = 0; i < count; i++)
        {
            nums[i] = Random.Range(0, count);
        }

        var lst = new List<int>();
        lst.AddRange(nums);
        lst.Sort();
        GFG.sort2(nums, nums.Length, 1);
        //Print(nums);
        //Print(lst.ToArray());
    }

    private static void Print(int[] nums)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var item in nums)
        {
            sb.Append(item + " ,");
        }

        Debug.LogError(sb);
    }
}