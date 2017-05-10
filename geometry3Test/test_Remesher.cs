using System;
using System.Diagnostics;
using g3;

namespace geometry3Test 
{
	public static class test_Remesher 
	{
		public static bool WriteDebugMeshes = true;


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



        public static void test_remesh_constraints_fixedverts()
        {
            int Slices = 128;
			DMesh3 mesh = TestUtil.MakeCappedCylinder(false, Slices);
			MeshUtil.ScaleMesh(mesh, Frame3f.Identity, new Vector3f(1,2,1));
			mesh.CheckValidity();
            AxisAlignedBox3d bounds = mesh.CachedBounds;

            // construct mesh projection target
            DMesh3 meshCopy = new DMesh3(mesh);
            meshCopy.CheckValidity();
            DMeshAABBTree3 tree = new DMeshAABBTree3(meshCopy);
            tree.Build();
            MeshProjectionTarget target = new MeshProjectionTarget() {
                Mesh = meshCopy, Spatial = tree
            };

            if ( WriteDebugMeshes )
				TestUtil.WriteDebugMesh(mesh, "remesh_fixed_constraints_test_before.obj");

            // construct constraint set
            MeshConstraints cons = new MeshConstraints();

            //EdgeRefineFlags useFlags = EdgeRefineFlags.NoFlip | EdgeRefineFlags.NoCollapse;
            EdgeRefineFlags useFlags = EdgeRefineFlags.NoFlip;

            foreach ( int eid in mesh.EdgeIndices() ) {
                double fAngle = MeshUtil.OpeningAngleD(mesh, eid);
                if (fAngle > 30.0f) {
                    cons.SetOrUpdateEdgeConstraint(eid, new EdgeConstraint(useFlags));
                    Index2i ev = mesh.GetEdgeV(eid);
                    int nSetID0 = (mesh.GetVertex(ev[0]).y > bounds.Center.y) ? 1 : 2;
                    int nSetID1 = (mesh.GetVertex(ev[1]).y > bounds.Center.y) ? 1 : 2;
                    cons.SetOrUpdateVertexConstraint(ev[0], new VertexConstraint(true, nSetID0));
                    cons.SetOrUpdateVertexConstraint(ev[1], new VertexConstraint(true, nSetID1));
                }
            }

			Remesher r = new Remesher(mesh);
            r.Precompute();
            r.SetExternalConstraints(cons);
            r.SetProjectionTarget(target);

            var stopwatch = Stopwatch.StartNew();

            //double fResScale = 1.0f;
            double fResScale = 0.5f;
			r.EnableFlips = r.EnableSplits = r.EnableCollapses = true;
			r.MinEdgeLength = 0.1f * fResScale;
			r.MaxEdgeLength = 0.2f * fResScale;
            r.EnableSmoothing = true;
            r.SmoothSpeedT = 0.5f;

            try {
                for (int k = 0; k < 20; ++k) {
                    r.BasicRemeshPass();
                    mesh.CheckValidity();
                }
            } catch {
                // ignore
            }

            stopwatch.Stop();
            System.Console.WriteLine("Second Pass Timing: " + stopwatch.Elapsed);

            if ( WriteDebugMeshes )
                TestUtil.WriteDebugMesh(mesh, "remesh_fixed_constraints_test_after.obj");
        }







        public static void test_remesh_constraints_vertcurves()
        {
            int Slices = 16;
            DMesh3 mesh = TestUtil.MakeCappedCylinder(false, Slices);
            MeshUtil.ScaleMesh(mesh, Frame3f.Identity, new Vector3f(1, 2, 1));
            //DMesh3 mesh = TestUtil.MakeRemeshedCappedCylinder(0.25);
            //DMesh3 mesh = TestUtil.MakeRemeshedCappedCylinder(1.0);
            mesh.CheckValidity();
            AxisAlignedBox3d bounds = mesh.CachedBounds;

            // construct mesh projection target
            DMesh3 meshCopy = new DMesh3(mesh);
            meshCopy.CheckValidity();
            DMeshAABBTree3 tree = new DMeshAABBTree3(meshCopy);
            tree.Build();
            MeshProjectionTarget mesh_target = new MeshProjectionTarget() {
                Mesh = meshCopy, Spatial = tree
            };

            // cylinder projection target
            CylinderProjectionTarget cyl_target = new CylinderProjectionTarget() {
                Cylinder = new Cylinder3d(new Vector3d(0, 1, 0), Vector3d.AxisY, 1, 2)
            };

            //IProjectionTarget target = mesh_target;
            IProjectionTarget target = cyl_target;

            // construct projection target circles
            CircleProjectionTarget bottomCons = new CircleProjectionTarget() {
                Circle = new Circle3d(bounds.Center, 1.0) };
            bottomCons.Circle.Center.y = bounds.Min.y;
            CircleProjectionTarget topCons = new CircleProjectionTarget() {
                Circle = new Circle3d(bounds.Center, 1.0) };
            topCons.Circle.Center.y = bounds.Max.y;


            if ( WriteDebugMeshes )
				TestUtil.WriteDebugMesh(mesh, "remesh_analytic_constraints_test_before.obj");

            // construct constraint set
            MeshConstraints cons = new MeshConstraints();

            //EdgeRefineFlags useFlags = EdgeRefineFlags.NoFlip | EdgeRefineFlags.NoCollapse;
            EdgeRefineFlags useFlags = EdgeRefineFlags.NoFlip;

            bool bConstrainVertices = true;
            foreach ( int eid in mesh.EdgeIndices() ) {
                double fAngle = MeshUtil.OpeningAngleD(mesh, eid);
                if (fAngle > 30.0f) {
                    Index2i ev = mesh.GetEdgeV(eid);
                    Vector3d ev0 = mesh.GetVertex(ev[0]);
                    Vector3d ev1 = mesh.GetVertex(ev[1]);
                    CircleProjectionTarget loopTarget = null;
                    if (ev0.y > bounds.Center.y && ev1.y > bounds.Center.y)
                        loopTarget = topCons;
                    else if (ev0.y < bounds.Center.y && ev1.y < bounds.Center.y)
                        loopTarget = bottomCons;

                    cons.SetOrUpdateEdgeConstraint(eid, new EdgeConstraint(useFlags, loopTarget));
                    if (bConstrainVertices && loopTarget != null) {
                        cons.SetOrUpdateVertexConstraint(ev[0], new VertexConstraint(loopTarget));
                        cons.SetOrUpdateVertexConstraint(ev[1], new VertexConstraint(loopTarget));
                    }
                }
            }


			Remesher r = new Remesher(mesh);
            //r.SetExternalConstraints(cons);
            r.SetProjectionTarget(target);
            r.Precompute();
            r.ENABLE_PROFILING = true;

            var stopwatch = Stopwatch.StartNew();

            //double fResScale = 1.0f;
            double fResScale = 0.5f;
            r.EnableFlips = r.EnableSplits = r.EnableCollapses = true;
            r.MinEdgeLength = 0.1f * fResScale;
            r.MaxEdgeLength = 0.2f * fResScale;
            r.EnableSmoothing = true;
            r.SmoothSpeedT = 1.0f;

            try {
                for (int k = 0; k < 20; ++k) {
                    r.BasicRemeshPass();
                    mesh.CheckValidity();
                }
            } catch {
                // continue;
            }
            stopwatch.Stop();
            System.Console.WriteLine("Second Pass Timing: " + stopwatch.Elapsed);

            if ( WriteDebugMeshes )
				TestUtil.WriteDebugMesh(mesh, "remesh_analytic_constraints_test_after.obj");
        }







        public static void test_remesh_region()
        {
            int Slices = 16;
            DMesh3 mesh = TestUtil.MakeCappedCylinder(false, Slices);
            MeshUtil.ScaleMesh(mesh, Frame3f.Identity, new Vector3f(1, 2, 1));
            mesh.CheckValidity();

            int[] tris = TestUtil.GetTrisOnPositiveSide(mesh, new Frame3f(Vector3f.Zero, Vector3f.AxisY));

            RegionRemesher r = new RegionRemesher(mesh, tris);
            r.Region.SubMesh.CheckValidity(true);

            TestUtil.WriteTestOutputMesh(r.Region.SubMesh, "remesh_region_submesh.obj");

            r.Precompute();
            double fResScale = 0.5f;
            r.EnableFlips = r.EnableSplits = r.EnableCollapses = true;
            r.MinEdgeLength = 0.1f * fResScale;
            r.MaxEdgeLength = 0.2f * fResScale;
            r.EnableSmoothing = true;
            r.SmoothSpeedT = 1.0f;

            for (int k = 0; k < 5; ++k) {
                r.BasicRemeshPass();
                mesh.CheckValidity();
            }

            TestUtil.WriteTestOutputMesh(r.Region.SubMesh, "remesh_region_submesh_refined.obj");

            r.BackPropropagate();

            TestUtil.WriteTestOutputMesh(mesh, "remesh_region_submesh_merged_1.obj");


            for (int k = 0; k < 5; ++k) {
                r.BasicRemeshPass();
                mesh.CheckValidity();
            }

            r.BackPropropagate();

            TestUtil.WriteTestOutputMesh(mesh, "remesh_region_submesh_merged_2.obj");
        }


	}
}
