using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using g3;

namespace geometry3Test
{
    public static class test_Spatial
    {

        public static DMesh3 MakeSpatialTestMesh(int n)
        {
            if ( n == 0 )
                return TestUtil.MakeCappedCylinder(false);
            else if ( n == 1 )
                return TestUtil.MakeCappedCylinder(true);
            throw new Exception("test_Spatial.MakeSpatialTestMesh: unknown mesh case");
        }



        public static void test_AABBTree_basic()
        {
            int meshCase = 0;
            DMesh3 mesh = MakeSpatialTestMesh(meshCase);
            DMeshAABBTree3 tree = new DMeshAABBTree3(mesh);
            tree.Build();

            tree.TestCoverage();
        }



        public static void test_AABBTree_TriDist()
        {
            int meshCase = 0;
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

    }
}
