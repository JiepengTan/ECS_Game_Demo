using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TestSum : MonoBehaviour
{
    private int count = 8;

    private int[] nums = { 7, 3, 2, 5, 6, 8, 1, 4 };
    private int[] threads = { 0, 1, 2, 3, 4, 5, 6, 7 };

    // Start is called before the first frame update
    void Start()
    {
        int[] temp = new int[count];
        for (int i = 0; i < temp.Length; i++)
        {
            temp[i] = nums[i];
        }
        int[] tempSum = new int[count];
        tempSum[0] = temp[0];
        for (int i = 1; i < tempSum.Length; i++)
        {
            tempSum[i] +=tempSum[i-1] + temp[i];
        }
        
        Print(temp, "init = 0 = ");
        Print(tempSum, "tsum = 0 = ");
        Debug.Log("=======================");
        int NoofElements = count;
        int offset = 1;
   
            for (int d = NoofElements >> 1; d > 0; d >>= 1)
            {
                for (int threadID = 0; threadID < count / 2; threadID++)
                {
                    if (threadID < d)
                    {
                        int ai = offset * (2 * threadID + 1) - 1;
                        int bi = offset * (2 * threadID + 2) - 1;
                        temp[bi] += temp[ai];
                    }
                }
                offset *= 2;
                Print(temp, "numd = " + d);
            }

        int sum = 0;
        //if (threadID == 0)
        {
            sum = temp[NoofElements - 1];
            temp[NoofElements - 1] = 0;
        }
        Debug.Log($"============= sum = {sum}============");

        for (int d = 1; d < NoofElements; d *= 2)
        {
            offset >>= 1;
            for (int threadID = 0; threadID < count / 2; threadID++)
            {
                if (threadID < d)
                {
                    int ai = offset * (2 * threadID + 1) - 1;
                    int bi = offset * (2 * threadID + 2) - 1;
                    int t = temp[ai];
                    temp[ai] = temp[bi];
                    temp[bi] += t;
                }
            }

            Print(temp, "numd = " + d);
        }

        int[] _ScannedInstancePredicates = new int[count];
        for (int threadID = 0; threadID < count / 2; threadID++)
        {
            _ScannedInstancePredicates[2 * threadID] = temp[2 * threadID]; // store to main memory
            _ScannedInstancePredicates[2 * threadID + 1] = temp[2 * threadID + 1];
        }
    }

    void Print(int[] temp, string msg)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(msg + " =[ ");
        foreach (var id in temp)
        {
            sb.Append("" + id + ", ");
        }

        sb.Append("]");
        Debug.Log(sb);
    }
}