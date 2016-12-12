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
				Vector2i ev = mesh.GetEdgeV(eid);

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
					Vector2i ev = mesh.GetEdgeV(eid);
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
			Util.gDevAssert( mesh.TriangleCount == 4 );
			Util.gDevAssert( mesh.VertexCount == 4 );
			foreach ( int eid in mesh.EdgeIndices() )
				Util.gDevAssert( mesh.edge_is_boundary(eid) == false );
		}

    }
}
