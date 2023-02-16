using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Example
{
    public class JobsExp1 : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            Debug.Log("start");
            NativeArray<float> a = new NativeArray<float>(2, Allocator.TempJob);

            NativeArray<float> b = new NativeArray<float>(2, Allocator.TempJob);

            NativeArray<float> result = new NativeArray<float>(2, Allocator.TempJob);

            a[0] = 1.1f;
            b[0] = 2.2f;
            a[1] = 3.3f;
            b[1] = 4.4f;

            MyParallelJob jobData = new MyParallelJob();
            jobData.a = a;
            jobData.b = b;
            jobData.result = result;

            // Schedule the job with one Execute per index in the results array and only 1 item per processing batch
            JobHandle handle = jobData.Schedule(result.Length, 1);

            // Wait for the job to complete
            handle.Complete();
            Debug.Log("result: " + jobData.result[0] + "------" + jobData.result[1]);
            // Free the memory allocated by the arrays
            a.Dispose();
            b.Dispose();
            result.Dispose();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

    public struct MyParallelJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<float> a;
        [ReadOnly]
        public NativeArray<float> b;
        public NativeArray<float> result;

        public void Execute(int i)
        {
            result[i] = a[i] + b[i];
        }
    }

}

