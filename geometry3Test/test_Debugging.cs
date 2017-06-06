using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using g3;

namespace geometry3Test
{
    class test_Debugging
    {


        public static void test()
        {
            DMesh3 Mesh = StandardMeshReader.ReadMesh("C:\\scratch\\TOOTH_upper_1_b.obj");
            Mesh.CheckValidity();

            Mesh = new DMesh3(Mesh, true);

            double RemeshMinEdgeLen = 0.25;
            double RemeshMaxEdgeLen = 0.5;

            // do N-1 rounds of smooth + remesh, then 1 of just smooth
            DMesh3 copy = new DMesh3(Mesh);
            DMeshAABBTree3 tree = new DMeshAABBTree3(copy);
            tree.Build();
            MeshProjectionTarget target = new MeshProjectionTarget(copy, tree);

            int N = 2;
            for (int i = 0; i < N; ++i) {
                MeshBoundaryLoops loops = new MeshBoundaryLoops(Mesh);
                loops.Compute();
                int nLargest = loops.MaxVerticesLoopIndex;

                MeshLoopSmooth smooth = new MeshLoopSmooth(Mesh, loops[nLargest]);
                smooth.ProjectF = target.Project;
                smooth.Rounds = 10;
                smooth.Smooth();

                Mesh.CheckValidity();

                foreach (int vid in smooth.Loop.Vertices)
                    Util.gDevAssert(Mesh.IsVertex(vid));

                // skip remesh on last pass
                if (i < N - 1) {
                    MeshFaceSelection select = new MeshFaceSelection(Mesh);
                    select.SelectVertexOneRings(smooth.Loop.Vertices);
                    select.ExpandToOneRingNeighbours(2);
                    select.LocalOptimize(true, true);

                    foreach (int tid in select)
                        Util.gDevAssert(Mesh.IsTriangle(tid));

                    RegionRemesher.QuickRemesh(Mesh, select.ToArray(),
                        RemeshMinEdgeLen, RemeshMaxEdgeLen, 0.1f, 5, target);
                    Mesh.CheckValidity();

                }

                Mesh = new DMesh3(Mesh, true);
            }


            TestUtil.WriteDebugMesh(Mesh, "TOOTH_upper_1_b_processed.obj");

        }

    }
}
