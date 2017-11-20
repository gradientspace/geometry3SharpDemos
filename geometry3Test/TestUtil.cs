using System;
using System.Collections.Generic;
using System.IO;
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


		public static string GetTestOutputPath(string filename) {
			return Path.Combine(Program.TEST_OUTPUT_PATH + filename);
		}



        public static void WriteTestOutputMesh(IMesh mesh, string sFilename, bool write_groups = true, bool write_vtxcolors = false, bool write_vtxuv = false)
        {
            OBJWriter writer = new OBJWriter();
            var s = new System.IO.StreamWriter(Program.TEST_OUTPUT_PATH + sFilename, false);
            writer.Write(s, new List<WriteMesh> { new WriteMesh(mesh) }, 
                new WriteOptions() { bPerVertexNormals = true, bWriteGroups = write_groups, bPerVertexColors = write_vtxcolors, bPerVertexUVs = write_vtxuv } );
			s.Close();
        }
        public static void WriteTestOutputMeshes(List<IMesh> meshes, string sFilename, bool write_groups = true, bool write_vtxcolors = false, bool write_vtxuv = false)
        {
            OBJWriter writer = new OBJWriter();
            var s = new System.IO.StreamWriter(Program.TEST_OUTPUT_PATH + sFilename, false);
            List<WriteMesh> wm = new List<WriteMesh>();
            foreach (var m in meshes)
                wm.Add(new WriteMesh(m));
            WriteOptions opt = new WriteOptions() { bCombineMeshes = false, bPerVertexNormals = true, bWriteGroups = write_groups, bPerVertexColors = write_vtxcolors, bPerVertexUVs = write_vtxuv };
            writer.Write(s, wm, opt );
            s.Close();
        }



        public static void WriteTestOutputGraph(DGraph2 graph, string sFilename, double vtxRadius = 2.5)
        {
            WriteTestOutputGraphs(new List<DGraph2>() { graph }, sFilename, SVGWriter.Style.Outline("black", 1.0f),
                SVGWriter.Style.Outline("red", 1.0f), vtxRadius);
        }
        public static void WriteTestOutputGraphs(List<DGraph2> graphs, string sFilename, double vtxRadius = 2.5) {
            WriteTestOutputGraphs(graphs, sFilename, SVGWriter.Style.Outline("black", 1.0f),
                SVGWriter.Style.Outline("red", 1.0f), vtxRadius);
        }
        public static void WriteTestOutputGraphs(List<DGraph2> graphs, string sFilename,
            SVGWriter.Style lineStyle, SVGWriter.Style vtxStyle, double vtxRadius = 2.5 )
        {
            Vector2d a = Vector2d.Zero, b = Vector2d.Zero;
            SVGWriter svg = new SVGWriter();
            foreach ( DGraph2 graph in graphs ) {
                foreach ( int eid in graph.EdgeIndices() ) {
                    graph.GetEdgeV(eid, ref a, ref b);
                    svg.AddLine(new Segment2d(a, b), lineStyle);
                }
            }
            if (vtxRadius > 0) {
                foreach (DGraph2 graph in graphs) {
                    foreach (int vid in graph.VertexIndices()) {
                        a = graph.GetVertex(vid);
                        svg.AddCircle(new Circle2d(a, vtxRadius), vtxStyle);
                    }
                }
            }
            svg.Write(Program.TEST_OUTPUT_PATH + sFilename);
        }




        public static void WriteDebugMesh(IMesh mesh, string sfilename, bool write_groups = true, bool write_vtxcolors = false, bool write_vtxuv = false)
        {
            OBJWriter writer = new OBJWriter();
            var s = new System.IO.StreamWriter(WRITE_PATH + sfilename, false);
			List<WriteMesh> meshes = new List<WriteMesh>() { new WriteMesh(mesh) };
            writer.Write(s, meshes, new WriteOptions() { bWriteGroups = write_groups, bPerVertexColors = write_vtxcolors, bPerVertexUVs = write_vtxuv } );
			s.Close();
		}

        public static void WriteDebugMeshes(List<IMesh> meshes, string sfilename, bool write_groups = true, bool write_vtxcolors = false, bool write_vtxuv = false)
        {
            OBJWriter writer = new OBJWriter();
            var s = new System.IO.StreamWriter(WRITE_PATH + sfilename, false);
            List<WriteMesh> wm = new List<WriteMesh>();
            foreach (var m in meshes)
                wm.Add(new WriteMesh(m));
            writer.Write(s, wm, new WriteOptions() { bCombineMeshes = false, bWriteGroups = write_groups, bPerVertexColors = write_vtxcolors, bPerVertexUVs = write_vtxuv } );
			s.Close();
		}


        // extension methods for Random
        public static Vector3d Direction(this Random rand)
        {
            Vector3d r = new Vector3d((2 * rand.NextDouble() - 1), 
                                      (2 * rand.NextDouble() - 1), 
                                      (2 * rand.NextDouble() - 1));
            r.Normalize();
            return r;
        }
        public static Vector3d PointInRange(this Random rand, double extent)
        {
            Vector3d r = new Vector3d( extent * (2 * rand.NextDouble() - 1), 
                                      extent * (2 * rand.NextDouble() - 1), 
                                      extent * (2 * rand.NextDouble() - 1));
            return r;
        }
        public static Vector3d PositivePoint(this Random rand)
        {
            return new Vector3d(rand.NextDouble(), rand.NextDouble(), rand.NextDouble());
        }



        public static DMesh3 LoadTestMesh(string sPath)
        {
            StandardMeshReader reader = new StandardMeshReader();
            reader.MeshBuilder = new DMesh3Builder();
            reader.Read(sPath, new ReadOptions());
            return (reader.MeshBuilder as DMesh3Builder).Meshes[0];
        }

		public static DMesh3 LoadTestInputMesh(string sPath)
		{
			StandardMeshReader reader = new StandardMeshReader();
			reader.MeshBuilder = new DMesh3Builder();
			reader.Read(Program.TEST_FILES_PATH + sPath, new ReadOptions());
			return (reader.MeshBuilder as DMesh3Builder).Meshes[0];
		}



        public static DMesh3 MakeMarker(Vector3d vPos, float fRadius, Colorf color)
        {
            DMesh3 markerMesh = new DMesh3(true, true, false);
            TrivialDiscGenerator gen = new TrivialDiscGenerator() { Slices = 8 };
            gen.Radius = fRadius;
            gen.Generate();
            gen.MakeMesh(markerMesh);
            foreach (int vid in markerMesh.VertexIndices())
                markerMesh.SetVertexColor(vid, color);
            MeshTransforms.Translate(markerMesh, vPos.x, vPos.y, vPos.z);
            return markerMesh;
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





        public static int[] GetTrisOnPositiveSide(DMesh3 mesh, Frame3f plane)
        {
            DVector<int> keep_tris = new DVector<int>();

            Vector3d[] tri = new Vector3d[3];
            foreach ( int tid in mesh.TriangleIndices() ) {
                mesh.GetTriVertices(tid, ref tri[0], ref tri[1], ref tri[2]);
                bool ok = true;
                for ( int j = 0; j < 3; ++j ) {
                    double d = (tri[j] - plane.Origin).Dot(plane.Z);
                    if (d < 0)
                        ok = false;
                }
                if (ok)
                    keep_tris.Add(tid);
            }

            return keep_tris.GetBuffer();
        }




        public static void SetColorsFromScalarF(DMesh3 mesh, Func<int,float> ScalarF, Vector2f scaleRange)
        {
            mesh.EnableVertexColors(Vector3f.Zero);

            foreach (int vid in mesh.VertexIndices()) {
                float f = ScalarF(vid);
                float t = (f - scaleRange[0]) / (scaleRange[1] - scaleRange[0]);
                mesh.SetVertexColor(vid, t * Vector3f.One);
            }
        }









        public static Vector2d[] RandomPoints2(int Count, Random r, Vector2d center, double scale = 1.0)
        {
            Vector2d[] v = new Vector2d[Count];
            for (int k = 0; k < Count; ++k)
                v[k] = new Vector2d(scale * 2.0*(r.NextDouble()-0.5), scale * 2.0 * (r.NextDouble() - 0.5)) + center;
            return v;
        }

        public static Vector3d[] RandomPoints3(int Count, Random r, Vector3d center, double scale = 1.0)
        {
            Vector3d[] v = new Vector3d[Count];
            for (int k = 0; k < Count; ++k)
                v[k] = new Vector3d(scale * 2.0 * (r.NextDouble() - 0.5), scale * 2.0 * (r.NextDouble() - 0.5), scale * 2.0 * (r.NextDouble() - 0.5)) + center;
            return v;
        }


        public static double[] RandomScalars(int Count, Random r, Interval1d range)
        {
            double[] v = new double[Count];
            for (int k = 0; k < Count; ++k) {
                v[k] = range.a + r.NextDouble() * range.Length;
            }
            return v;
        }

    }
}
