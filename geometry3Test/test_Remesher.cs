using System;
using g3;

namespace geometry3Test 
{
	public static class test_Remesher 
	{
		public static bool WriteDebugMeshes = false;


		public static DMesh3 make_good_cylinder(float fResScale = 1.0f) {
			DMesh3 mesh = TestUtil.MakeCappedCylinder(false);
			MeshUtil.ScaleMesh(mesh, Frame3f.Identity, new Vector3f(1,2,1));
			mesh.CheckValidity();

			Remesher r = new Remesher(mesh);
			r.EnableFlips = r.EnableSplits = r.EnableCollapses = true;
			r.MinEdgeLength = 0.1f * fResScale;
			r.MaxEdgeLength = 0.2f * fResScale;
			r.EnableSmoothing = true;
			r.SmoothSpeedT = 0.1f;

			r.EnableFlips = r.EnableSmoothing = false;
			r.MinEdgeLength = 0.05f * fResScale;
			for ( int k = 0; k < 10; ++k ) {
				r.BasicRemeshPass();
				mesh.CheckValidity();
			}

			r.MinEdgeLength = 0.1f * fResScale;
			r.MaxEdgeLength = 0.2f * fResScale;
			r.EnableFlips = r.EnableCollapses = r.EnableSmoothing = true;

			for ( int k = 0; k < 10; ++k ) {
				r.BasicRemeshPass();
				mesh.CheckValidity();
			}

			return mesh;

		}



		public static void test_basic_closed_remesh() {
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




		public static void test_remesh_smoothing() {
			DMesh3 mesh = make_good_cylinder(1.0f);

			Remesher r = new Remesher(mesh);
			r.EnableFlips = r.EnableSplits = r.EnableCollapses = false;
			r.EnableSmoothing = true;
			r.SmoothSpeedT = 0.5f;
			r.SmoothType = Remesher.SmoothTypes.MeanValue;

			for ( int k = 0; k < 100; ++k ) {
				r.BasicRemeshPass();
				mesh.CheckValidity();
			}

			if ( WriteDebugMeshes )
				TestUtil.WriteDebugMesh(mesh, "remesh_smoothing_test_after.obj");
		}



        public static void test_remesh_constraints_1()
        {
			DMesh3 mesh = TestUtil.MakeCappedCylinder(false);
			MeshUtil.ScaleMesh(mesh, Frame3f.Identity, new Vector3f(1,2,1));
			mesh.CheckValidity();

            if ( WriteDebugMeshes )
				TestUtil.WriteDebugMesh(mesh, "remesh_constraints_test_before.obj");

            // construct constraint set
            MeshConstraints cons = new MeshConstraints();

            //EdgeRefineFlags useFlags = EdgeRefineFlags.NoFlip | EdgeRefineFlags.NoCollapse;
            EdgeRefineFlags useFlags = EdgeRefineFlags.NoFlip;

            foreach ( int eid in mesh.EdgeIndices() ) {
                double fAngle = MeshUtil.OpeningAngleD(mesh, eid);
                if (fAngle > 30.0f) {
                    cons.SetOrUpdateEdgeConstraint(eid, new EdgeConstraint(useFlags));
                    Index2i ev = mesh.GetEdgeV(eid);
                    cons.SetOrUpdateVertexConstraint(ev[0], new VertexConstraint(true));
                    cons.SetOrUpdateVertexConstraint(ev[1], new VertexConstraint(true));
                }
            }


            double fResScale = 1.0f;
			Remesher r = new Remesher(mesh);
            r.SetExternalConstraints(cons);
			r.EnableFlips = r.EnableSplits = r.EnableCollapses = true;
			r.MinEdgeLength = 0.1f * fResScale;
			r.MaxEdgeLength = 0.2f * fResScale;
			r.EnableSmoothing = true;
			r.SmoothSpeedT = 0.1f;

            r.EnableFlips = r.EnableSmoothing = false;
            r.MinEdgeLength = 0.05f * fResScale;
            for (int k = 0; k < 10; ++k) {
                r.BasicRemeshPass();
                mesh.CheckValidity();
            }

            r.MinEdgeLength = 0.1f * fResScale;
            r.MaxEdgeLength = 0.2f * fResScale;
            r.EnableFlips = true;
            r.EnableCollapses = true;
            r.EnableSplits = true;
            r.EnableSmoothing = true;
            r.SmoothSpeedT = 0.5f;

            for (int k = 0; k < 10; ++k) {
                r.BasicRemeshPass();
                mesh.CheckValidity();
            }


            if ( WriteDebugMeshes )
				TestUtil.WriteDebugMesh(mesh, "remesh_constraints_test_after.obj");
        }



	}
}
