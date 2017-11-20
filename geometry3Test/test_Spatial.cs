using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using g3;

namespace geometry3Test
{
    public static class test_Spatial
    {

        public static DMesh3 MakeSpatialTestMesh(int n)
        {
            if (n == 0)
                return TestUtil.MakeCappedCylinder(false);
            else if (n == 1)
                return TestUtil.MakeCappedCylinder(true);
            else if (n == 2)
                return TestUtil.MakeCappedCylinder(false, 256);
            else if (n == 3)
                return TestUtil.MakeCappedCylinder(true, 256);
            else if (n == 4)
                return TestUtil.MakeRemeshedCappedCylinder(1.0f);
            else if (n == 5)
                return TestUtil.MakeRemeshedCappedCylinder(0.5f);
            else if (n == 6)
                return TestUtil.MakeRemeshedCappedCylinder(0.25f);
            else if (n == 7)
                return TestUtil.MakeCappedCylinder(false, 128, true);
            else if (n == 8)
                return TestUtil.LoadTestInputMesh("bunny_solid.obj");
            throw new Exception("test_Spatial.MakeSpatialTestMesh: unknown mesh case");
        }
        public static int NumTestCases { get { return 9; } }



        public static void test_AABBTree_basic()
        {
            List<int> cases = new List<int>() { 0, 1, 2, 3, 4, 7, 8 };

            foreach (int meshCase in cases) {

                DMesh3 mesh = MakeSpatialTestMesh(meshCase);
                DMeshAABBTree3 treeMedian = new DMeshAABBTree3(mesh);
                treeMedian.Build(DMeshAABBTree3.BuildStrategy.TopDownMedian);
                treeMedian.TestCoverage();
                treeMedian.TotalVolume();

                DMeshAABBTree3 treeMidpoint = new DMeshAABBTree3(mesh);
                treeMidpoint.Build(DMeshAABBTree3.BuildStrategy.TopDownMidpoint);
                treeMidpoint.TestCoverage();
                treeMidpoint.TotalVolume();

                DMeshAABBTree3 treeUpFast = new DMeshAABBTree3(mesh);
                treeUpFast.Build(DMeshAABBTree3.BuildStrategy.BottomUpFromOneRings, DMeshAABBTree3.ClusterPolicy.Fastest);
                treeUpFast.TestCoverage();
                treeUpFast.TotalVolume();

                DMeshAABBTree3 treeUpN = new DMeshAABBTree3(mesh);
                treeUpN.Build(DMeshAABBTree3.BuildStrategy.BottomUpFromOneRings, DMeshAABBTree3.ClusterPolicy.FastVolumeMetric);
                treeUpN.TestCoverage();
                treeUpN.TotalVolume();
            }
        }



        public static void test_AABBTree_TriDist(int meshCase = 0)
        {
            DMesh3 mesh = MakeSpatialTestMesh(meshCase);
            DMeshAABBTree3 tree = new DMeshAABBTree3(mesh);
            tree.Build();

            AxisAlignedBox3d bounds = mesh.CachedBounds;
            Vector3d ext = bounds.Extents;
            Vector3d c = bounds.Center;

            Random rand = new Random(316136327);

            int N = 10000;
            for ( int ii = 0; ii < N; ++ii ) {
                Vector3d p = new Vector3d(
                    c.x + (4 * ext.x * (2 * rand.NextDouble() - 1)),
                    c.y + (4 * ext.y * (2 * rand.NextDouble() - 1)),
                    c.z + (4 * ext.z * (2 * rand.NextDouble() - 1)));

                int tNearBrute = MeshQueries.FindNearestTriangle_LinearSearch(mesh, p);
                int tNearTree = tree.FindNearestTriangle(p);

                DistPoint3Triangle3 qBrute = MeshQueries.TriangleDistance(mesh, tNearBrute, p);
                DistPoint3Triangle3 qTree = MeshQueries.TriangleDistance(mesh, tNearTree, p);

                if ( Math.Abs(qBrute.DistanceSquared - qTree.DistanceSquared) > MathUtil.ZeroTolerance )
                    Util.gBreakToDebugger();
            }
        }




        public static void test_AABBTree_RayHit(int meshCase = 8)
        {
            DMesh3 mesh = MakeSpatialTestMesh(meshCase);
            DMeshAABBTree3 tree = new DMeshAABBTree3(mesh);
            tree.Build();
            tree.TestCoverage();

            AxisAlignedBox3d bounds = mesh.CachedBounds;
            Vector3d ext = bounds.Extents;
            Vector3d c = bounds.Center;
            double r = bounds.DiagonalLength / 4;

            Random rand = new Random(316136327);


            tree.FindNearestHitTriangle(
                new Ray3f(100 * Vector3f.One, Vector3f.One));


            // test rays out from center of box, and rays in towards it
            // (should all hit for standard test cases)
            int hits = 0;
            int N = (meshCase > 7 ) ? 1000 : 10000;
#if true
            for ( int ii = 0; ii < N; ++ii ) {
                if (ii % 100 == 0)
                    System.Console.WriteLine("{0} / {1}", ii, N);

                Vector3d p = (ii < N/2) ? c : c + 2*r*rand.Direction();
                Vector3d d = (ii < N/2) ? rand.Direction() : (c - p).Normalized;
                Ray3d ray = new Ray3d(p, d);

                int tNearBrute = MeshQueries.FindHitTriangle_LinearSearch(mesh, ray);
                int tNearTree = tree.FindNearestHitTriangle(ray);

                //System.Console.WriteLine("{0} - {1}", tNearBrute, tree.TRI_TEST_COUNT);

                if (tNearBrute == DMesh3.InvalidID) {
                    Debug.Assert(tNearBrute == tNearTree);
                    continue;
                }
                ++hits;

                IntrRay3Triangle3 qBrute = MeshQueries.TriangleIntersection(mesh, tNearBrute, ray);
                IntrRay3Triangle3 qTree = MeshQueries.TriangleIntersection(mesh, tNearTree, ray);

                double dotBrute = mesh.GetTriNormal(tNearBrute).Dot(ray.Direction);
                double dotTree = mesh.GetTriNormal(tNearTree).Dot(ray.Direction);

                Debug.Assert(Math.Abs(qBrute.RayParameter - qTree.RayParameter) < MathUtil.ZeroTolerance);
            }
            Debug.Assert(hits == N);
            System.Console.WriteLine("in/out rays: {0} hits out of {1} rays", hits, N);
#endif




            // random rays
            hits = 0;
            for (int ii = 0; ii < N; ++ii) {
                if (ii % 100 == 0)
                    System.Console.WriteLine("{0} / {1}", ii, N);

                Vector3d target = c + rand.PointInRange(r);
                Vector3d o = c + rand.PointInRange(10 * r);
                Ray3d ray = new Ray3d(o, (target - o).Normalized);

                int tNearBrute = MeshQueries.FindHitTriangle_LinearSearch(mesh, ray);
                int tNearTree = tree.FindNearestHitTriangle(ray);

                //System.Console.WriteLine("{0} - {1}", tNearBrute, tree.TRI_TEST_COUNT);

                if (tNearBrute == DMesh3.InvalidID) {
                    Debug.Assert(tNearBrute == tNearTree);
                    continue;
                }
                ++hits;

                IntrRay3Triangle3 qBrute = MeshQueries.TriangleIntersection(mesh, tNearBrute, ray);
                IntrRay3Triangle3 qTree = MeshQueries.TriangleIntersection(mesh, tNearTree, ray);

                double dotBrute = mesh.GetTriNormal(tNearBrute).Dot(ray.Direction);
                double dotTree = mesh.GetTriNormal(tNearTree).Dot(ray.Direction);

                Debug.Assert(Math.Abs(qBrute.RayParameter - qTree.RayParameter) < MathUtil.ZeroTolerance);
            }

            System.Console.WriteLine("random rays: hit {0} of {1} rays", hits, N);

        }








        public static void test_AABBTree_profile()
        {
            System.Console.WriteLine("Building test meshes");
            DMesh3[] meshes = new DMesh3[NumTestCases];
            for ( int i = 0; i < NumTestCases; ++i )
                meshes[i] = MakeSpatialTestMesh(i);
            System.Console.WriteLine("done!");


            int N = 10;

            // avoid garbage collection
            List<DMeshAABBTree3> trees = new List<DMeshAABBTree3>();
            DMeshAABBTree3 tree = null;



            for (int i = 0; i < NumTestCases; ++i) {
                Stopwatch w = new Stopwatch();
                for (int j = 0; j < N; ++j) {
                    tree = new DMeshAABBTree3(meshes[i]);
                    w.Start();
                    tree.Build(DMeshAABBTree3.BuildStrategy.TopDownMidpoint);
                    //tree.Build(DMeshAABBTree3.BuildStrategy.TopDownMedian);
                    //tree.Build(DMeshAABBTree3.BuildStrategy.BottomUpFromOneRings, DMeshAABBTree3.ClusterPolicy.FastVolumeMetric);
                    w.Stop();
                    trees.Add(tree);
                }
                double avg_time = w.ElapsedTicks / (double)N;
                System.Console.WriteLine(string.Format("Case {0}: time {1}  tris {2} vol {3}  len {4}", i, avg_time, tree.Mesh.TriangleCount, tree.TotalVolume(), tree.TotalExtentSum()));
            }

        }



        public static void test_Winding()
        {
            Sphere3Generator_NormalizedCube gen = new Sphere3Generator_NormalizedCube() { EdgeVertices = 200 };
            DMesh3 mesh = gen.Generate().MakeDMesh();
            //DMesh3 mesh = TestUtil.LoadTestInputMesh("bunny_solid.obj");
            DMeshAABBTree3 spatial = new DMeshAABBTree3(mesh, true);
            GC.Collect();

            double maxdim = mesh.CachedBounds.MaxDim;
            Vector3d center = mesh.CachedBounds.Center;

            var pts = TestUtil.RandomPoints3(100, new Random(31337), center, maxdim*0.5);
            int num_inside = 0;
            foreach (Vector3d pt in pts) {
                bool inside = spatial.IsInside(pt);
                double distSqr = MeshQueries.TriDistanceSqr(mesh, spatial.FindNearestTriangle(pt), pt);
                double ptDist = pt.Length;
                double winding = mesh.WindingNumber(pt);
                double winding_2 = spatial.WindingNumber(pt);
                if (Math.Abs(winding - winding_2) > 0.00001)
                    System.Diagnostics.Debugger.Break();
                bool winding_inside = Math.Abs(winding) > 0.5;
                if (inside != winding_inside)
                    System.Diagnostics.Debugger.Break();
                if (inside)
                    num_inside++;
            }
            System.Console.WriteLine("inside {0} / {1}", num_inside, pts.Length);

            // force rebuild for profiling code
            GC.Collect();

            LocalProfiler p = new LocalProfiler();
            pts = TestUtil.RandomPoints3(1000, new Random(31337), center, maxdim * 0.5);

            p.Start("full eval");
            double sum = 0;
            foreach (Vector3d pt in pts) {
                double winding = mesh.WindingNumber(pt);
                sum += winding;
            }
            p.StopAll();

            p.Start("tree build");
            spatial = new DMeshAABBTree3(mesh, true);
            spatial.WindingNumber(Vector3d.Zero);
            p.StopAll();

            GC.Collect();
            GC.Collect();

            p.Start("tree eval");
            sum = 0;
            foreach (Vector3d pt in pts) {
                double winding = spatial.WindingNumber(pt);
                sum += winding;
            }

            p.StopAll();

            System.Console.WriteLine(p.AllTimes());
        }


    }
}
