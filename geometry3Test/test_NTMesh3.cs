using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using g3;

namespace geometry3Test
{
    public static class test_NTMesh3
    {
        public static void basic_tests()
        {
            System.Console.WriteLine("DMesh3:basic_tests() starting");

            CappedCylinderGenerator cylgen = new CappedCylinderGenerator();
            cylgen.Generate();
            NTMesh3 tmp = cylgen.MakeNTMesh();
            tmp.CheckValidity();
            System.Console.WriteLine("cylinder ok");
        }


        public static void test_remove()
        {
            System.Console.WriteLine("DMesh3:test_remove() starting");

            List<NTMesh3> testMeshes = new List<NTMesh3>() {
                new CappedCylinderGenerator().Generate().MakeNTMesh()
            };

            // remove-one tests
            foreach (NTMesh3 mesh in testMeshes) {
                int N = mesh.TriangleCount;
                for (int j = 0; j < N; ++j) {
                    NTMesh3 r1 = new NTMesh3(mesh);
                    r1.RemoveTriangle(j, false);
                    r1.CheckValidity();         // remove might create non-manifold tris at bdry

                    NTMesh3 r2 = new NTMesh3(mesh);
                    r2.RemoveTriangle(j, true);
                    r2.CheckValidity();

                    NTMesh3 r3 = new NTMesh3(mesh);
                    r3.RemoveTriangle(j, false);
                    r3.CheckValidity();         // remove might create non-manifold tris at bdry

                    NTMesh3 r4 = new NTMesh3(mesh);
                    r4.RemoveTriangle(j, true);
                    r4.CheckValidity();
                }
            }


            // grinder tests
            foreach (NTMesh3 mesh in testMeshes ) {

                // sequential
                NTMesh3 tmp = new NTMesh3(mesh);
                bool bDone = false;
                while (!bDone) {
                    bDone = true;
                    foreach ( int ti in tmp.TriangleIndices() ) {
                        if ( tmp.IsTriangle(ti) && tmp.RemoveTriangle(ti, true) == MeshResult.Ok ) {
                            bDone = false;
                            tmp.CheckValidity();
                        }
                    }
                }
                System.Console.WriteLine(string.Format("remove_all sequential: before {0} after {1}", mesh.TriangleCount, tmp.TriangleCount));

                // randomized
                tmp = new NTMesh3(mesh);
                bDone = false;
                while (!bDone) {
                    bDone = true;
                    foreach ( int ti in tmp.TriangleIndices() ) {
                        int uset = (ti + 256) % tmp.MaxTriangleID;        // break symmetry
                        if ( tmp.IsTriangle(uset) && tmp.RemoveTriangle(uset, true) == MeshResult.Ok ) {
                            bDone = false;
                            tmp.CheckValidity();
                        }
                    }
                }
                System.Console.WriteLine(string.Format("remove_all randomized: before {0} after {1}", mesh.TriangleCount, tmp.TriangleCount));
            }


            System.Console.WriteLine("remove ok");
        }





		public static void split_tests(bool bTestBoundary, int N = 100) {
			System.Console.WriteLine("split_tests() starting");

            NTMesh3 mesh = new NTMesh3(TestUtil.MakeCappedCylinder(bTestBoundary));
			mesh.CheckValidity(FailMode.DebugAssert);

			Random r = new Random(31377);
			for ( int k = 0; k < N; ++k ) {
				int eid = r.Next() % mesh.EdgeCount;
				if ( ! mesh.IsEdge(eid) )
					continue;

				NTMesh3.EdgeSplitInfo splitInfo; 
				MeshResult result = mesh.SplitEdge(eid, out splitInfo);
				Debug.Assert(result == MeshResult.Ok);
				mesh.CheckValidity(FailMode.DebugAssert);
			}

            System.Console.WriteLine("splits ok");
		}




        public static void split_tests_nonmanifold(int N = 100)
        {
            System.Console.WriteLine("split_tests_nonmanifold() starting");

            Random r = new Random(31337);
            NTMesh3 mesh = new NTMesh3();
            int a = mesh.AppendVertex(Vector3d.Zero);
            int b = mesh.AppendVertex(Vector3d.AxisZ);
            for (int k = 0; k < 5; ++k) {
                int c = mesh.AppendVertex(TestUtil.RandomPoints3(1, r, Vector3d.Zero, 1)[0]);
                int tid = mesh.AppendTriangle( new Index3i(a, b, c) );
                Debug.Assert(tid >= 0);
            }

            TestUtil.WriteTestOutputMesh(mesh.Deconstruct(), "nonmanifold_split_input.obj");

            for (int k = 0; k < N; ++k) {
                int eid = r.Next() % mesh.EdgeCount;
                if (!mesh.IsEdge(eid))
                    continue;

                NTMesh3.EdgeSplitInfo splitInfo;
                MeshResult result = mesh.SplitEdge(eid, out splitInfo);
                Debug.Assert(result == MeshResult.Ok);
                mesh.CheckValidity(FailMode.DebugAssert);
            }

            TestUtil.WriteTestOutputMesh(mesh.Deconstruct(), "nonmanifold_split_output.obj");
        }


        public static void poke_test()
        {
            NTMesh3 mesh = new NTMesh3(TestUtil.LoadTestInputMesh("plane_250v.obj"));
            //DMesh3 mesh = TestUtil.LoadTestInputMesh("sphere_bowtie_groups.obj");
            mesh.CheckValidity();

            int NT = mesh.TriangleCount;
            for ( int i = 0; i < NT; i += 5 ) {
                Vector3d n = mesh.GetTriNormal(i);
                NTMesh3.PokeTriangleInfo pokeinfo;
                MeshResult result = mesh.PokeTriangle(i, out pokeinfo);

                Vector3d v = mesh.GetVertex(pokeinfo.new_vid);
                v += 0.25f * n;
                mesh.SetVertex(pokeinfo.new_vid, v);

                mesh.CheckValidity();
            }

            System.Console.WriteLine("pokes ok");

            //TestUtil.WriteTestOutputMesh(mesh, "poke_test_result.obj");

        }



        

    }
}
