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




        class TestDynamicNode : DynamicPriorityQueueNode
        {
            public int id;

            public void Initialize(int id)
            {
                this.id = id;
            }
        }


        public static void test_pq()
        {
            System.Console.WriteLine("testing priority queues...");

            int MAXID = 1000000;
            int MAXCOUNT = 100;
            int mod = 31337;

            for (int kk = 0; kk < 3; ++kk) {
                if (kk == 1)
                    MAXCOUNT = MAXID / 10;
                else if (kk == 2)
                    MAXCOUNT = MAXID;

                IndexPriorityQueue PQ_Index = new IndexPriorityQueue(MAXID);
                DynamicPriorityQueue<TestDynamicNode> PQ_Dynamic = new DynamicPriorityQueue<TestDynamicNode>();
                MemoryPool<TestDynamicNode> Dynamic_Pool = new MemoryPool<TestDynamicNode>();
                Dictionary<int, TestDynamicNode> DynamicNodes = new Dictionary<int, TestDynamicNode>();

                System.Console.WriteLine("inserting {0} of {1}", MAXCOUNT, MAXID);

                int count = 0;
                int id = 0;
                while (count < MAXCOUNT) {
                    id = (id + mod) % MAXID;

                    PQ_Index.Enqueue(id, count);
                    TestDynamicNode node = Dynamic_Pool.Allocate();
                    node.Initialize(id);
                    PQ_Dynamic.Enqueue(node, count);
                    DynamicNodes[id] = node;
                    count++;
                }
                Util.gDevAssert(PQ_Index.IsValidQueue());
                Util.gDevAssert(PQ_Dynamic.IsValidQueue());
                Util.gDevAssert(PQ_Index.Count == PQ_Dynamic.Count);
                Util.gDevAssert(PQ_Index.First == PQ_Dynamic.First.id);
                check_same(PQ_Index, PQ_Dynamic);

                Random r = new Random(31337);

                System.Console.WriteLine("updating...");

                id = 0; count = 0;
                while (count++ < MAXCOUNT) {
                    id = (id + mod) % MAXID;
                    float new_p = count + ((r.Next() % 1000) - 1000);
                    PQ_Index.Update(id, new_p);
                    PQ_Dynamic.Update(DynamicNodes[id], new_p);
                }
                Util.gDevAssert(PQ_Index.IsValidQueue());
                Util.gDevAssert(PQ_Dynamic.IsValidQueue());
                check_same(PQ_Index, PQ_Dynamic);

                System.Console.WriteLine("removing...");

                while (PQ_Index.Count > 0) {
                    int index_id = PQ_Index.Dequeue();
                    TestDynamicNode node = PQ_Dynamic.Dequeue();
                    Util.gDevAssert(index_id == node.id);
                }
            }
        }
        static void check_same(IndexPriorityQueue PQ_Index, DynamicPriorityQueue<TestDynamicNode> PQ_Dynamic)
        {
            List<int> indices = new List<int>(PQ_Index);
            List<TestDynamicNode> nodes = new List<TestDynamicNode>(PQ_Dynamic);
            Util.gDevAssert(indices.Count == nodes.Count);
            for (int i = 0; i < indices.Count; ++i)
                Util.gDevAssert(indices[i] == nodes[i].id);
        }





        public static void profile_pq()
        {
            LocalProfiler index_p = new LocalProfiler();
            LocalProfiler dynamic_p = new LocalProfiler();

            int MAXID = 1000000;
            int MAXCOUNT = 100000;
            int mod = 31337;
            int rounds = 10;

            int iters = 25;
            for ( int k = 0; k < iters; ++k ) {
                System.Console.WriteLine("profile_pq: round {0}", k);
                time_index_pq(MAXID, MAXCOUNT, mod, rounds, index_p);
                GC.Collect();
                time_dynamic_pq(MAXID, MAXCOUNT, mod, rounds, dynamic_p);
                GC.Collect();
            }

            index_p.DivideAllAccumulated(iters);
            dynamic_p.DivideAllAccumulated(iters);

            System.Console.WriteLine(index_p.AllAccumulatedTimes("Index: "));
            System.Console.WriteLine(dynamic_p.AllAccumulatedTimes("Dynam: "));
        }



        static void time_index_pq(int MAXID, int MAXCOUNT, int mod, int rounds, LocalProfiler profiler)
        {
            profiler.Start("index_all");

            profiler.Start("index_initialize");
            IndexPriorityQueue PQ_Index = new IndexPriorityQueue(MAXID);
            profiler.StopAndAccumulate("index_initialize");

            for (int ri = 0; ri < rounds; ++ri) {

                profiler.Start("index_push");

                int count = 0;
                int id = 0;
                while (count < MAXCOUNT) {
                    id = (id + mod) % MAXID;
                    PQ_Index.Enqueue(id, count);
                    count++;
                }
            
                profiler.StopAndAccumulate("index_push");
                profiler.Start("index_update");

                Random r = new Random(31337);
                id = 0; count = 0;
                while (count++ < MAXCOUNT) {
                    id = (id + mod) % MAXID;
                    float new_p = count + ((r.Next() % 1000) - 1000);
                    PQ_Index.Update(id, new_p);
                }

                profiler.StopAndAccumulate("index_update");
                profiler.Start("index_pop");

                while (PQ_Index.Count > 0) {
                    int index_id = PQ_Index.Dequeue();
                }

                profiler.StopAndAccumulate("index_pop");
            }

            profiler.StopAndAccumulate("index_all");
        }





        static void time_dynamic_pq(int MAXID, int MAXCOUNT, int mod, int rounds, LocalProfiler profiler)
        {
            profiler.Start("dynam_all");

            profiler.Start("dynam_initialize");
            DynamicPriorityQueue<TestDynamicNode> PQ_Dynamic = new DynamicPriorityQueue<TestDynamicNode>();
            MemoryPool<TestDynamicNode> Dynamic_Pool = new MemoryPool<TestDynamicNode>();
            SparseObjectList<TestDynamicNode> IDMap = new SparseObjectList<TestDynamicNode>(MAXID, MAXCOUNT);
            profiler.StopAndAccumulate("dynam_initialize");

            for (int ri = 0; ri < rounds; ++ri) {

                profiler.Start("dynam_push");

                int count = 0;
                int id = 0;
                while (count < MAXCOUNT) {
                    id = (id + mod) % MAXID;
                    TestDynamicNode node = new TestDynamicNode(); //Dynamic_Pool.Allocate();
                    //node.Initialize(id);
                    node.id = id;
                    PQ_Dynamic.Enqueue(node, count);
                    IDMap[id] = node;
                    count++;
                }

                profiler.StopAndAccumulate("dynam_push");
                profiler.Start("dynam_update");

                Random r = new Random(31337);
                id = 0; count = 0;
                while (count++ < MAXCOUNT) {
                    id = (id + mod) % MAXID;
                    float new_p = count + ((r.Next() % 1000) - 1000);
                    PQ_Dynamic.Update(IDMap[id], new_p);
                }

                profiler.StopAndAccumulate("dynam_update");
                profiler.Start("dynam_pop");

                while (PQ_Dynamic.Count > 0) {
                    TestDynamicNode node = PQ_Dynamic.Dequeue();
                    //if (rounds > 1)
                    //    Dynamic_Pool.Return(node);
                }

                profiler.StopAndAccumulate("dynam_pop");
            }

            //Dynamic_Pool.FreeAll();

            profiler.StopAndAccumulate("dynam_all");
        }










        public static void test_pq_debuggable()
        {

            int MAXID = 10;

            IndexPriorityQueue QIndex = new IndexPriorityQueue(MAXID);
            DynamicPriorityQueue<TestDynamicNode> QDynamic = new DynamicPriorityQueue<TestDynamicNode>();
            TestDynamicNode[] dyn_nodes = new TestDynamicNode[MAXID];

            bool verbose = false;

            //int n = 1;
            for (int i = 0; i < MAXID; ++i) {
                //n = (n + 17) % 17;
                int id = i;
                float priority = 1.0f - (float)i / 10.0f;
                QIndex.Enqueue(id, priority);
                if (verbose) System.Console.WriteLine("i = {0}", i);
                QIndex.DebugPrint();
                if (verbose) System.Console.WriteLine("---", i);
                dyn_nodes[i] = new TestDynamicNode() { id = id };
                QDynamic.Enqueue(dyn_nodes[i], priority);
                QDynamic.DebugPrint();

            }

            System.Console.WriteLine("Dequeing...");


            for ( int i = 0; i < MAXID; ++i ) {
                float newp = (float)((i + MAXID/2) % MAXID)  / 10.0f;
                QIndex.Update(i, newp);
                QDynamic.Update(dyn_nodes[i], newp);
                //System.Console.WriteLine("UPDATE {0} {1}", QIndex.First, QDynamic.First.id);
                //QIndex.DebugPrint();
                //System.Console.WriteLine("---", i);
                //QDynamic.DebugPrint();
                Util.gDevAssert(QIndex.First == QDynamic.First.id);
            }


            for ( int i = 0; i < MAXID; ++i ) {
                int id = QIndex.Dequeue();
                var node = QDynamic.Dequeue();
                Util.gDevAssert(id == node.id);
                if (verbose) System.Console.WriteLine("DEQUEUE {0} {1}", id, node.id);

                if (verbose) QIndex.DebugPrint();
                if (verbose) System.Console.WriteLine("---", i);
                if (verbose) QDynamic.DebugPrint();
            }



        }


    }
}
