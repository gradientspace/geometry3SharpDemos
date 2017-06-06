using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using g3;

namespace geometry3Test
{
    public class test_VectorTypes
    {


        public static void test_bitarrays()
        {
            List<int> testN = new List<int>() { 13, 215, 371, 1212, 23154, 5 * 32, 1024 * 32, 5 * 31, 1024 * 31, 5 * 33, 1024 * 33 };

            foreach (int N in testN) {
                BitArray bits = new BitArray(N);
                HBitArray hbits = new HBitArray(N);

                int[] jumps = new int[3] { 3, 1, 17 };

                List<int> values = new List<int>();
                int set_count = 0;
                for (int i = 0; i < N; i += jumps[i % jumps.Length]) {
                    if (bits[i] == false)
                        set_count++;
                    bits[i] = true;
                    hbits[i] = true;
                    values.Add(i);
                    Debug.Assert(bits[i] == hbits[i]);
                }
                Debug.Assert(hbits.TrueCount == set_count);
                Debug.Assert(hbits.Count == N);

                // test iteration
                int vi = 0;
                foreach (int idx in hbits) {
                    Debug.Assert(hbits[idx] == bits[idx]);
                    Debug.Assert(idx == values[vi]);
                    vi++;
                }
            }

        }



        public static void test_rcvector()
        {
            for (int ROUND = 0; ROUND < 2; ++ROUND) {
                RefCountVector vec = new RefCountVector();

                int N = 10 + ROUND;

                for (int k = 0; k < N; ++k) {
                    int i = vec.allocate();
                    Debug.Assert(i == k);
                }
                Debug.Assert(vec.count == N);
                Debug.Assert(vec.is_dense == true);

                int counter = 0;
                foreach (int i in vec) {
                    Debug.Assert(i == counter++);
                }

                int removed = 0;
                for (int k = 0; k < N; ++k) {
                    if (k % 2 == 0) {
                        vec.decrement(k);
                        ++removed;
                    }
                }
                Debug.Assert(vec.count == N - removed);
                Debug.Assert(vec.is_dense == false);

                System.Console.WriteLine("After remove:");
                System.Console.WriteLine(vec.debug_print());

                for (int k = 0; k < N; ++k) {
                    if (k % 2 == 0)
                        Debug.Assert(vec.refCount(k) == 0);
                    else
                        Debug.Assert(vec.refCount(k) == 1);
                }

                System.Console.WriteLine("\niteration:");
                int iter_count = 0;
                foreach (int i in vec) {
                    System.Console.Write(i.ToString() + ":" + vec.refCount(i) + " ");
                    iter_count++;
                }
                System.Console.WriteLine(" (done)");

                Debug.Assert(iter_count == N - removed);

                // re-allocate
                for (int k = 0; k < removed; ++k) {
                    int j = vec.allocate();
                    vec.increment(j);
                }

                System.Console.WriteLine("\nAfter adding back:");
                System.Console.WriteLine(vec.debug_print());
            }
        }
    }
}
