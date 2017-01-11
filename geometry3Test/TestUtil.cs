using System;
using System.Collections.Generic;
using g3;


namespace geometry3Test 
{
	public static class TestUtil 
	{
		public static string WRITE_PATH {
            get {
                if (Util.IsRunningOnMono())
                    return "/Users/rms/scratch/";
                else
                    return "c:\\scratch\\";
            }
        }



        public static void WriteDebugMesh(IMesh mesh, string sfilename)
        {
            OBJWriter writer = new OBJWriter();
            var s = new System.IO.StreamWriter(WRITE_PATH + sfilename, false);
            writer.Write(s, new List<IMesh> { mesh }, new WriteOptions() { bWriteGroups = true } );
			s.Close();
		}


        public static DMesh3 MakeTrivialRect()
        {
            DMesh3 rectMesh = new DMesh3();
            TrivialRectGenerator rgen = new TrivialRectGenerator();
            rgen.Generate();
            rgen.MakeMesh(rectMesh);
            rectMesh.CheckValidity();
            return rectMesh;
        }


		public static DMesh3 MakeOpenCylinder(bool bNoSharedVertices, int nSlices = 16) 
		{ 
			DMesh3 mesh = new DMesh3();
			OpenCylinderGenerator cylgen = new OpenCylinderGenerator() {
                NoSharedVertices = bNoSharedVertices, Slices = nSlices };
			cylgen.Generate();
			cylgen.MakeMesh(mesh);
			mesh.ReverseOrientation();
			return mesh;
		}

		public static DMesh3 MakeCappedCylinder(bool bNoSharedVertices, int nSlices = 16, bool bHole = false) 
		{ 
			DMesh3 mesh = new DMesh3(true, false, false, true);
			CappedCylinderGenerator cylgen = new CappedCylinderGenerator() {
                NoSharedVertices = bNoSharedVertices, Slices = nSlices };
			cylgen.Generate();
			cylgen.MakeMesh(mesh);
			mesh.ReverseOrientation();
            if (bHole)
                mesh.RemoveTriangle(0);
			return mesh;
		}


        // gets slow for small res factor...
		public static DMesh3 MakeRemeshedCappedCylinder(double fResFactor = 1.0) 
		{ 
			DMesh3 mesh = TestUtil.MakeCappedCylinder(false, 128);
			MeshUtil.ScaleMesh(mesh, Frame3f.Identity, new Vector3f(1,2,1));

            // construct mesh projection target
            DMesh3 meshCopy = new DMesh3(mesh);
            DMeshAABBTree3 tree = new DMeshAABBTree3(meshCopy);
            tree.Build();
            MeshProjectionTarget target = new MeshProjectionTarget() {
                Mesh = meshCopy, Spatial = tree
            };
            MeshConstraints cons = new MeshConstraints();
            EdgeRefineFlags useFlags = EdgeRefineFlags.NoFlip;
            foreach ( int eid in mesh.EdgeIndices() ) {
                double fAngle = MeshUtil.OpeningAngleD(mesh, eid);
                if (fAngle > 30.0f) {
                    cons.SetOrUpdateEdgeConstraint(eid, new EdgeConstraint(useFlags));
                    Index2i ev = mesh.GetEdgeV(eid);
                    int nSetID0 = (mesh.GetVertex(ev[0]).y > 1) ? 1 : 2;
                    int nSetID1 = (mesh.GetVertex(ev[1]).y > 1) ? 1 : 2;
                    cons.SetOrUpdateVertexConstraint(ev[0], new VertexConstraint(true, nSetID0));
                    cons.SetOrUpdateVertexConstraint(ev[1], new VertexConstraint(true, nSetID1));
                }
            }
			Remesher r = new Remesher(mesh);
            r.SetExternalConstraints(cons);
            r.SetProjectionTarget(target);
            r.Precompute();
			r.EnableFlips = r.EnableSplits = r.EnableCollapses = true;
            r.MinEdgeLength = 0.1f * fResFactor;
            r.MaxEdgeLength = 0.2f * fResFactor;
			r.EnableSmoothing = true;
            r.SmoothSpeedT = 0.5f;
            for (int k = 0; k < 20; ++k) 
                r.BasicRemeshPass();
            return mesh;
		}


	}
}
