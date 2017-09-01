using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using g3;

namespace geometry3Test
{
    public static class test_DMesh3
    {
        public static void basic_tests()
        {
            System.Console.WriteLine("DMesh3:basic_tests() starting");

            DMesh3 tmp = new DMesh3();
            CappedCylinderGenerator cylgen = new CappedCylinderGenerator();
            cylgen.Generate();
            cylgen.MakeMesh(tmp);
            tmp.CheckValidity();

            System.Console.WriteLine("cylinder ok");
        }


        public static void test_remove()
        {
            System.Console.WriteLine("DMesh3:test_remove() starting");

            List<DMesh3> testMeshes = new List<DMesh3>() {
                TestUtil.MakeTrivialRect(),
                TestUtil.MakeOpenCylinder(false),       // removing any creates bowtie!
                TestUtil.MakeOpenCylinder(true),
                TestUtil.MakeCappedCylinder(false),
                TestUtil.MakeCappedCylinder(true)
            };

            // remove-one tests
            foreach (DMesh3 mesh in testMeshes) {
                int N = mesh.TriangleCount;
                for (int j = 0; j < N; ++j) {
                    DMesh3 r1 = new DMesh3(mesh);
                    r1.RemoveTriangle(j, false);
                    r1.CheckValidity(true);         // remove might create non-manifold tris at bdry

                    DMesh3 r2 = new DMesh3(mesh);
                    r2.RemoveTriangle(j, true);
                    r2.CheckValidity(true);

                    DMesh3 r3 = new DMesh3(mesh);
                    r3.RemoveTriangle(j, false, true);
                    r3.CheckValidity(false);         // remove might create non-manifold tris at bdry

                    DMesh3 r4 = new DMesh3(mesh);
                    r4.RemoveTriangle(j, true, true);
                    r4.CheckValidity(false);
                }
            }


            // grinder tests
            foreach ( DMesh3 mesh in testMeshes ) {

                // sequential
                DMesh3 tmp = new DMesh3(mesh);
                bool bDone = false;
                while (!bDone) {
                    bDone = true;
                    foreach ( int ti in tmp.TriangleIndices() ) {
                        if ( tmp.IsTriangle(ti) && tmp.RemoveTriangle(ti, true, true) == MeshResult.Ok ) {
                            bDone = false;
                            tmp.CheckValidity(false);
                        }
                    }
                }
                System.Console.WriteLine(string.Format("remove_all sequential: before {0} after {1}", mesh.TriangleCount, tmp.TriangleCount));

                // randomized
                tmp = new DMesh3(mesh);
                bDone = false;
                while (!bDone) {
                    bDone = true;
                    foreach ( int ti in tmp.TriangleIndices() ) {
                        int uset = (ti + 256) % tmp.MaxTriangleID;        // break symmetry
                        if ( tmp.IsTriangle(uset) && tmp.RemoveTriangle(uset, true, true) == MeshResult.Ok ) {
                            bDone = false;
                            tmp.CheckValidity(false);
                        }
                    }
                }
                System.Console.WriteLine(string.Format("remove_all randomized: before {0} after {1}", mesh.TriangleCount, tmp.TriangleCount));
            }


            System.Console.WriteLine("remove ok");
        }



		public static void split_tests(bool bTestBoundary, int N = 100) {
			System.Console.WriteLine("DMesh3:split_tests() starting");

			DMesh3 mesh = TestUtil.MakeCappedCylinder(bTestBoundary);
			mesh.CheckValidity();

			Random r = new Random(31377);
			for ( int k = 0; k < N; ++k ) {
				int eid = r.Next() % mesh.EdgeCount;
				if ( ! mesh.IsEdge(eid) )
					continue;

				DMesh3.EdgeSplitInfo splitInfo; 
				MeshResult result = mesh.SplitEdge(eid, out splitInfo);
				Debug.Assert(result == MeshResult.Ok);
				mesh.CheckValidity();
			}

			System.Console.WriteLine("splits ok");
		}


		public static void flip_tests(bool bTestBoundary, int N = 100) {
			System.Console.WriteLine("DMesh3:flip_tests() starting");

			DMesh3 mesh = TestUtil.MakeCappedCylinder(bTestBoundary);
			mesh.CheckValidity();

			Random r = new Random(31377);
			for ( int k = 0; k < N; ++k ) {
				int eid = r.Next() % mesh.EdgeCount;
				if ( ! mesh.IsEdge(eid) )
					continue;
				bool bBoundary = mesh.edge_is_boundary(eid);

				DMesh3.EdgeFlipInfo flipInfo; 
				MeshResult result = mesh.FlipEdge(eid, out flipInfo);
				if ( bBoundary )
					Debug.Assert(result == MeshResult.Failed_IsBoundaryEdge);
				else
					Debug.Assert(result == MeshResult.Ok || result == MeshResult.Failed_FlippedEdgeExists);
				mesh.CheckValidity();
			}

			System.Console.WriteLine("flips ok");
		}



		public static void collapse_tests(bool bTestBoundary, int N = 100) {

			bool write_debug_meshes = false;

			DMesh3 mesh = TestUtil.MakeCappedCylinder(bTestBoundary);
			mesh.CheckValidity();

			System.Console.WriteLine( string.Format("DMesh3:collapse_tests() starting - test bdry {2}, verts {0} tris {1}", 
			                                        mesh.VertexCount, mesh.TriangleCount, bTestBoundary) );

			if(write_debug_meshes)
				TestUtil.WriteDebugMesh(mesh, string.Format("before_collapse_{0}.obj", ((bTestBoundary)?"boundary":"noboundary")));


			Random r = new Random(31377);
			for ( int k = 0; k < N; ++k ) {
				int eid = r.Next() % mesh.EdgeCount;
				if ( ! mesh.IsEdge(eid) )
					continue;
				//bool bBoundary = mesh.edge_is_boundary(eid);
				//if (bTestBoundary && bBoundary == false)
				//	 continue;
				Index2i ev = mesh.GetEdgeV(eid);

				DMesh3.EdgeCollapseInfo collapseInfo; 
				MeshResult result = mesh.CollapseEdge(ev[0], ev[1], out collapseInfo);
				Debug.Assert(
					result != MeshResult.Failed_NotAnEdge &&
					result != MeshResult.Failed_FoundDuplicateTriangle );

				mesh.CheckValidity();
			}

			System.Console.WriteLine( string.Format("random collapses ok - verts {0} tris {1}", 
			                                        mesh.VertexCount, mesh.TriangleCount) );


			collapse_to_convergence(mesh);

			System.Console.WriteLine( string.Format("all possible collapses ok - verts {0} tris {1}", 
			                                        mesh.VertexCount, mesh.TriangleCount) );

			if(write_debug_meshes)
				TestUtil.WriteDebugMesh(mesh, string.Format("after_collapse_{0}.obj", ((bTestBoundary)?"boundary":"noboundary")));
		}




		// this function collapses edges until it can't anymore
		static void collapse_to_convergence(DMesh3 mesh) {
			bool bContinue = true;
			while (bContinue) {
				bContinue = false;
				for ( int eid = 0; eid < mesh.MaxEdgeID; ++eid) { 
					if ( ! mesh.IsEdge(eid) )
						continue;
					Index2i ev = mesh.GetEdgeV(eid);
					DMesh3.EdgeCollapseInfo collapseInfo; 
					MeshResult result = mesh.CollapseEdge(ev[0], ev[1], out collapseInfo);
					if ( result == MeshResult.Ok ) {
						bContinue = true;
						break;
					}
				}

			}
		}





		// cyl with no shared verts should collapse down to two triangles
		public static void collapse_test_convergence_cyl_noshared() {
			DMesh3 mesh = TestUtil.MakeCappedCylinder(true);
			mesh.CheckValidity();
			collapse_to_convergence(mesh);
			Util.gDevAssert( mesh.TriangleCount == 3 );
			Util.gDevAssert( mesh.VertexCount == 9 );
			foreach ( int tid in mesh.TriangleIndices() )
				Util.gDevAssert( mesh.tri_is_boundary(tid) );
		}

		// open cylinder (ie a tube) should collapse down to having two boundary loops with 3 verts/edges each
		public static void collapse_test_convergence_opencyl() {
			DMesh3 mesh = TestUtil.MakeOpenCylinder(false);
			mesh.CheckValidity();

			collapse_to_convergence(mesh);
			int bdry_v = 0, bdry_t = 0, bdry_e = 0;
			foreach ( int eid in mesh.EdgeIndices() ) {
				if ( mesh.edge_is_boundary(eid) )
					bdry_e++;
			}
			Util.gDevAssert(bdry_e == 6);
			foreach ( int tid in mesh.TriangleIndices() ) {
				if ( mesh.tri_is_boundary(tid) )
					bdry_t++;
			}
			Util.gDevAssert(bdry_t == 6);			
			foreach ( int vid in mesh.VertexIndices() ) {
				if ( mesh.vertex_is_boundary(vid) )
					bdry_v++;
			}
			Util.gDevAssert(bdry_v == 6);					
		}

		// closed mesh should collapse to a tetrahedron
		public static void collapse_test_closed_mesh() {
			DMesh3 mesh = TestUtil.MakeCappedCylinder(false);
			mesh.CheckValidity();
			collapse_to_convergence(mesh);
            mesh.CheckValidity();
			Util.gDevAssert( mesh.TriangleCount == 4 );
			Util.gDevAssert( mesh.VertexCount == 4 );
			foreach ( int eid in mesh.EdgeIndices() )
				Util.gDevAssert( mesh.edge_is_boundary(eid) == false );
		}







		public static void merge_test_closed_mesh()
		{
			DMesh3 mesh = TestUtil.MakeCappedCylinder(true, 4);
			mesh.CheckValidity();

			DMesh3.MergeEdgesInfo info;

			int merges = 0;
			while (true) {
				List<int> be = new List<int>(mesh.BoundaryEdgeIndices());
				if (be.Count == 0)
					break;
				int ea = be[0];
				int eo = find_pair_edge(mesh, ea, be);
				if (eo != DMesh3.InvalidID) {
					var result = mesh.MergeEdges(ea, eo, out info);
					Util.gDevAssert(result == MeshResult.Ok);
					TestUtil.WriteTestOutputMesh(mesh, "after_last_merge.obj");
					mesh.CheckValidity();
					merges++;
				}
			}
			mesh.CheckValidity();


			DMesh3 originalMesh = TestUtil.LoadTestInputMesh("three_edge_crack.obj");
			List<int> bdryedges = new List<int>(originalMesh.BoundaryEdgeIndices());
			for (int k = 0; k < bdryedges.Count; ++k) {
				DMesh3 copyMesh = new DMesh3(originalMesh);
				List<int> be = new List<int>(copyMesh.BoundaryEdgeIndices());
				int ea = be[k];
				int eo = find_pair_edge(copyMesh, ea, be);
				if (eo != DMesh3.InvalidID) {
					var result = copyMesh.MergeEdges(ea, eo, out info);
					Util.gDevAssert(result == MeshResult.Ok);
					if ( k == 3 )
						TestUtil.WriteTestOutputMesh(copyMesh, "after_last_merge.obj");
					mesh.CheckValidity();
				}
			}

			// this should fail at every edge because it would create bad-orientation edges
			DMesh3 dupeMesh = TestUtil.LoadTestInputMesh("duplicate_4tris.obj");
			List<int> dupeBE = new List<int>(dupeMesh.BoundaryEdgeIndices());
			for (int k = 0; k < dupeBE.Count; ++k) {
				int ea = dupeBE[k];
				int eo = find_pair_edge(dupeMesh, ea, dupeBE);
				if (eo != DMesh3.InvalidID) {
					var result = dupeMesh.MergeEdges(ea, eo, out info);
					Util.gDevAssert(result == MeshResult.Failed_SameOrientation);
					mesh.CheckValidity();
					TestUtil.WriteTestOutputMesh(dupeMesh, "after_last_merge.obj");
				}
			}
		}



		static int find_pair_edge(DMesh3 mesh, int eid, List<int> candidates) {
			Index2i ev = mesh.GetEdgeV(eid);
			Vector3d a = mesh.GetVertex(ev.a), b = mesh.GetVertex(ev.b);
			double eps = 100 * MathUtil.Epsilonf;
			foreach (int eother in candidates ) {
				if (eother == eid)
					continue;
				Index2i ov = mesh.GetEdgeV(eother);
				Vector3d c = mesh.GetVertex(ov.a), d = mesh.GetVertex(ov.b);
				if ((a.EpsilonEqual(c, eps) && b.EpsilonEqual(d, eps)) ||
				    (b.EpsilonEqual(c, eps) && a.EpsilonEqual(d, eps)))
					return eother;
			};
			return DMesh3.InvalidID;
		}


    }
}
