using System;
using g3;

namespace geometry3Test 
{
	public static class test_Remesher 
	{
		public static bool WriteDebugMeshes = false;


		public static void basic_closed_remesh_test() {
			DMesh3 mesh = TestUtil.MakeCappedCylinder(false);
			MeshUtil.ScaleMesh(mesh, Frame3f.Identity, new Vector3f(1,2,1));
			//DMesh3 mesh = TestUtil.MakeOpenCylinder(false);
			mesh.CheckValidity();

			if ( WriteDebugMeshes )
				TestUtil.WriteDebugMesh(mesh, "basic_closed_remesh_before.obj");

			Remesher r = new Remesher(mesh);
			r.EnableFlips = r.EnableSplits = r.EnableCollapses = true;
			r.MinEdgeLength = 0.1f;
			r.MaxEdgeLength = 0.2f;
			r.EnableSmoothing = true;
			r.SmoothSpeedT = 0.1f;

			r.EnableFlips = r.EnableSmoothing = false;
			r.MinEdgeLength = 0.05f;
			for ( int k = 0; k < 10; ++k ) {
				r.BasicRemeshPass();
				mesh.CheckValidity();
			}

			r.MinEdgeLength = 0.1f;
			r.MaxEdgeLength = 0.2f;
			r.EnableFlips = r.EnableCollapses = r.EnableSmoothing = true;

			for ( int k = 0; k < 10; ++k ) {
				r.BasicRemeshPass();
				mesh.CheckValidity();
			}

			r.EnableSplits = r.EnableCollapses = false;

			for ( int k = 0; k < 10; ++k ) {
				r.BasicRemeshPass();
				mesh.CheckValidity();
			}

			if ( WriteDebugMeshes )
				TestUtil.WriteDebugMesh(mesh, "basic_closed_remesh_after.obj");
		}


	}
}
