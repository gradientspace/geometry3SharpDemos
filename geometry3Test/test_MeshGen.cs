using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using g3;

namespace geometry3Test
{
    public static class test_MeshGen
    {
        public static void WriteGeneratedMesh(MeshGenerator gen, string sFilename)
        {
            DMesh3 mesh = gen.MakeDMesh();
            TestUtil.WriteTestOutputMesh(mesh, sFilename);
        }

        public static void test_basic_generators()
        {
            TrivialDiscGenerator disc_gen = new TrivialDiscGenerator();
            disc_gen.Generate();
            WriteGeneratedMesh(disc_gen, "meshgen_Disc.obj");

            TrivialRectGenerator rect_gen = new TrivialRectGenerator();
            rect_gen.Generate();
            WriteGeneratedMesh(rect_gen, "meshgen_Rect.obj");

            GriddedRectGenerator gridrect_gen = new GriddedRectGenerator();
            gridrect_gen.Generate();
            WriteGeneratedMesh(gridrect_gen, "meshgen_GriddedRect.obj");

            PuncturedDiscGenerator punc_disc_gen = new PuncturedDiscGenerator();
            punc_disc_gen.Generate();
            WriteGeneratedMesh(punc_disc_gen, "meshgen_PuncturedDisc.obj");

            TrivialBox3Generator box_gen = new TrivialBox3Generator();
            Frame3f f = Frame3f.Identity;
            f.Rotate(Quaternionf.AxisAngleD(Vector3f.AxisY, 45.0f));
            f.Rotate(Quaternionf.AxisAngleD(Vector3f.AxisZ, 45.0f));
            box_gen.Box = new Box3d(f.Origin, f.X, f.Y, f.Z, new Vector3d(3, 2, 1));
            box_gen.Generate();
            WriteGeneratedMesh(box_gen, "meshgen_TrivialBox_shared.obj");
            box_gen.NoSharedVertices = true;
            box_gen.Generate();
            WriteGeneratedMesh(box_gen, "meshgen_TrivialBox_noshared.obj");


            RoundRectGenerator roundrect_gen = new RoundRectGenerator();
            roundrect_gen.Width = 2;
            roundrect_gen.Generate();
            WriteGeneratedMesh(roundrect_gen, "meshgen_RoundRect.obj");


            GridBox3Generator gridbox_gen = new GridBox3Generator();
            gridbox_gen.Generate();
            WriteGeneratedMesh(gridbox_gen, "meshgen_GridBox_shared.obj");
            gridbox_gen.NoSharedVertices = true;
            gridbox_gen.Generate();
            WriteGeneratedMesh(gridbox_gen, "meshgen_GridBox_noshared.obj");

            Sphere3Generator_NormalizedCube normcube_gen = new Sphere3Generator_NormalizedCube();
            normcube_gen.Generate();
            WriteGeneratedMesh(normcube_gen, "meshgen_Sphere_NormalizedCube_shared.obj");
            normcube_gen.NoSharedVertices = true;
            normcube_gen.Box = new Box3d(new Frame3f(Vector3f.One, Vector3f.One), Vector3d.One * 1.3);
            normcube_gen.Generate();
            WriteGeneratedMesh(normcube_gen, "meshgen_Sphere_NormalizedCube_noshared.obj");


            TubeGenerator tube_gen = new TubeGenerator() {
                Vertices = new List<Vector3d>() { Vector3d.Zero, Vector3d.AxisX, 2 * Vector3d.AxisX, 3 * Vector3d.AxisX },
                Polygon = Polygon2d.MakeCircle(1, 16)
            };
            tube_gen.Generate();
            WriteGeneratedMesh(tube_gen, "meshgen_TubeGenerator.obj");

            tube_gen.Polygon.Translate(Vector2d.One);
            tube_gen.CapCenter = Vector2d.One;
            tube_gen.Generate();
            WriteGeneratedMesh(tube_gen, "meshgen_TubeGenerator_shifted.obj");
        }




        public static void test_mesh_builders()
        {
            // test mesh builder
            DMesh3 origMesh = TestUtil.LoadTestMesh(Program.TEST_FILES_PATH + "bunny_open_base.obj");
            float[] Vf = new float[origMesh.VertexCount * 3];
            List<Vector3f> Vl = new List<Vector3f>();
            int k = 0;
            foreach (Vector3d v in origMesh.Vertices()) {
                Vf[k++] = (float)v.x; Vf[k++] = (float)v.y; Vf[k++] = (float)v.z;
                Vl.Add((Vector3f)v);
            }
            double[] Nd = origMesh.NormalsBuffer.GetBufferCast<double>();
            Vector3d[] Nl = new Vector3d[origMesh.VertexCount];
            foreach (int vid in origMesh.VertexIndices())
                Nl[vid] = origMesh.GetVertexNormal(vid);

            int[] Ti = origMesh.TrianglesBuffer.GetBuffer();
            Index3i[] Tl = new Index3i[origMesh.TriangleCount];
            foreach (int tid in origMesh.TriangleIndices())
                Tl[tid] = origMesh.GetTriangle(tid);

            DMesh3 m1 = DMesh3Builder.Build(Vf, Ti, Nd);
            DMesh3 m2 = DMesh3Builder.Build(Vl, Tl, Nl);

            Util.gDevAssert(origMesh.IsSameMesh(m1, true));
            Util.gDevAssert(origMesh.IsSameMesh(m2, true));
        }




        public static void test_voxel_surface()
        {
            //DMesh3 mesh = TestUtil.LoadTestInputMesh("bunny_solid.obj");
            DMesh3 mesh = TestUtil.LoadTestInputMesh("holey_bunny_2.obj");
            DMeshAABBTree3 spatial = new DMeshAABBTree3(mesh, autoBuild: true);

            AxisAlignedBox3d bounds = mesh.CachedBounds;
            int numcells = 64;
            double cellsize = bounds.MaxDim / numcells;
            MeshSignedDistanceGrid levelSet = new MeshSignedDistanceGrid(mesh, cellsize);
            levelSet.UseParallel = true;
            levelSet.Compute();

            Bitmap3 bmp = new Bitmap3(levelSet.Dimensions);
            foreach (Vector3i idx in bmp.Indices()) {
                float f = levelSet[idx.x, idx.y, idx.z];
                bmp.Set(idx, (f < 0) ? true : false);
            }


            //AxisAlignedBox3d bounds = mesh.CachedBounds;
            //int numcells = 32;
            //double cellsize = bounds.MaxDim / numcells;
            //ShiftGridIndexer3 indexer = new ShiftGridIndexer3(bounds.Min-2*cellsize, cellsize);

            //Bitmap3 bmp = new Bitmap3(new Vector3i(numcells, numcells, numcells));
            //foreach (Vector3i idx in bmp.Indices()) {
            //    Vector3d v = indexer.FromGrid(idx);
            //    bmp.Set(idx, spatial.IsInside(v));
            //}

            //spatial.WindingNumber(Vector3d.Zero);
            //Bitmap3 bmp = new Bitmap3(new Vector3i(numcells+3, numcells+3, numcells+3));
            //gParallel.ForEach(bmp.Indices(), (idx) => {
            //    Vector3d v = indexer.FromGrid(idx);
            //    bmp.SafeSet(idx, spatial.WindingNumber(v) > 0.8);
            //});


            VoxelSurfaceGenerator voxGen = new VoxelSurfaceGenerator();
            voxGen.Voxels = bmp;
            voxGen.ColorSourceF = (idx) => {
                return new Colorf((float)idx.x, (float)idx.y, (float)idx.z) * (1.0f / numcells);
            };
            voxGen.Generate();
            DMesh3 voxMesh = voxGen.Meshes[0];

            Util.WriteDebugMesh(voxMesh, "c:\\scratch\\temp.obj");

            TestUtil.WriteTestOutputMesh(voxMesh, "voxel_surf.obj", true, true);
        }




        public static void test_marching_cubes()
        {
            MarchingCubes c = new MarchingCubes();

            LocalProfiler profiler = new LocalProfiler();
            profiler.Start("Generate");

            c.ParallelCompute = true;
            c.Generate();

            profiler.Stop("Generate");

            System.Console.WriteLine("Tris: {0} Times: {1}", c.Mesh.TriangleCount, profiler.AllTimes());

            Reducer r = new Reducer(c.Mesh);
            r.ReduceToEdgeLength(c.CubeSize * 0.25);

            System.Console.WriteLine("after reduce: {0}", c.Mesh.TriangleCount);

            MeshNormals.QuickCompute(c.Mesh);
            TestUtil.WriteTestOutputMesh(c.Mesh, "marching_cubes.obj");
        }





        public static void test_marching_cubes_levelset()
        {
            //DMesh3 mesh = TestUtil.LoadTestInputMesh("bunny_solid.obj");
            DMesh3 mesh = TestUtil.LoadTestInputMesh("bunny_overlap_solids.obj");
            //Sphere3Generator_NormalizedCube gen = new Sphere3Generator_NormalizedCube() { EdgeVertices = 100, Radius = 5 };
            //DMesh3 mesh = gen.Generate().MakeDMesh();

            AxisAlignedBox3d bounds = mesh.CachedBounds;
            int numcells = 128;
            double cellsize = bounds.MaxDim / numcells;

            MeshSignedDistanceGrid levelSet = new MeshSignedDistanceGrid(mesh, cellsize);
            levelSet.ExactBandWidth = 3;
            //levelSet.InsideMode = MeshSignedDistanceGrid.InsideModes.CrossingCount;
            levelSet.UseParallel = true;
            levelSet.ComputeMode = MeshSignedDistanceGrid.ComputeModes.NarrowBandOnly;
            levelSet.Compute();

            var iso = new DenseGridTrilinearImplicit(levelSet.Grid, levelSet.GridOrigin, levelSet.CellSize);

            MarchingCubes c = new MarchingCubes();
            c.Implicit = iso;
            c.Bounds = mesh.CachedBounds;
            c.Bounds.Expand(c.Bounds.MaxDim * 0.1);
            c.CubeSize = c.Bounds.MaxDim / 128;
            //c.CubeSize = levelSet.CellSize;

            c.Generate();

            TestUtil.WriteTestOutputMesh(c.Mesh, "marching_cubes_levelset.obj");
        }



        public static void test_marching_cubes_topology()
        {
            AxisAlignedBox3d bounds = new AxisAlignedBox3d(1.0);
            int numcells = 64;
            double cellsize = bounds.MaxDim / numcells;

            Random r = new Random(31337);
            for (int ii= 0; ii < 100; ++ii) {

                DenseGrid3f grid = new DenseGrid3f();
                grid.resize(numcells, numcells, numcells);
                grid.assign(1);
                for (int k = 2; k < numcells - 3; k++) {
                    for (int j = 2; j < numcells - 3; j++) {
                        for (int i = 2; i < numcells - 3; i++) {
                            double d = r.NextDouble();
                            if (d > 0.9)
                                grid[i, j, k] = 0.0f;
                            else if (d > 0.5)
                                grid[i, j, k] = 1.0f;
                            else
                                grid[i, j, k] = -1.0f;
                        }
                    }
                }

                var iso = new DenseGridTrilinearImplicit(grid, Vector3f.Zero, cellsize);

                MarchingCubes c = new MarchingCubes();
                c.Implicit = iso;
                c.Bounds = bounds;
                //c.Bounds.Max += 3 * cellsize * Vector3d.One;
                //c.Bounds.Expand(2*cellsize);

                // this produces holes
                c.CubeSize = cellsize * 4.1;
                //c.CubeSize = cellsize * 2;

                //c.Bounds = new AxisAlignedBox3d(2.0);

                c.Generate();

                for (float f = 2.0f; f < 8.0f; f += 0.13107f) {
                    c.CubeSize = cellsize * 4.1;
                    c.Generate();
                    c.Mesh.CheckValidity(false);

                    MeshBoundaryLoops loops = new MeshBoundaryLoops(c.Mesh);
                    if (loops.Count > 0)
                        throw new Exception("found loops!");

                }
            }

            //c.Mesh.CheckValidity(false);


            //TestUtil.WriteTestOutputMesh(c.Mesh, "marching_cubes_topotest.obj");
        }





		public static void test_marching_cubes_implicits()
		{
			DMesh3 mesh = TestUtil.LoadTestInputMesh("bunny_solid.obj");
			MeshTransforms.Translate(mesh, -mesh.CachedBounds.Center);
			double meshCellsize = mesh.CachedBounds.MaxDim / 32;
			MeshSignedDistanceGrid levelSet = new MeshSignedDistanceGrid(mesh, meshCellsize);
			levelSet.ExactBandWidth = 3;
			levelSet.UseParallel = true;
			levelSet.ComputeMode = MeshSignedDistanceGrid.ComputeModes.NarrowBandOnly;
			levelSet.Compute();
			var meshIso = new DenseGridTrilinearImplicit(levelSet.Grid, levelSet.GridOrigin, levelSet.CellSize);


			ImplicitOffset3d offsetMeshIso = new ImplicitOffset3d() {
				A = meshIso, Offset = 2.0
			};

			double r = 15.0;
			ImplicitSphere3d sphere1 = new ImplicitSphere3d() {
				Origin = Vector3d.Zero,
				Radius = r
			};
			ImplicitSphere3d sphere2 = new ImplicitSphere3d() {
				Origin = r*Vector3d.AxisX,
				Radius = r
			};
			ImplicitAxisAlignedBox3d aabox1 = new ImplicitAxisAlignedBox3d() {
				AABox = new AxisAlignedBox3d(r * 0.5 * Vector3d.One, r, r * 0.75, r * 0.5)
			};
			ImplicitBox3d box1 = new ImplicitBox3d() {
				Box = new Box3d(new Frame3f(r * 0.5 * Vector3d.One, Vector3d.One.Normalized),
								new Vector3d(r, r * 0.75, r * 0.5))
			};
			ImplicitLine3d line1 = new ImplicitLine3d() {
				Segment = new Segment3d(Vector3d.Zero, r * Vector3d.One),
				Radius = 3.0
			};
			ImplicitHalfSpace3d half1 = new ImplicitHalfSpace3d() {
				Origin = Vector3d.Zero, Normal = Vector3d.One.Normalized
			};

			ImplicitUnion3d union = new ImplicitUnion3d() {
				A = sphere1, B = line1
			};
			ImplicitDifference3d difference = new ImplicitDifference3d() {
				A = meshIso, B = aabox1
			};
			ImplicitIntersection3d intersect = new ImplicitIntersection3d() {
				A = meshIso, B = half1
			};
			ImplicitNaryUnion3d nunion = new ImplicitNaryUnion3d() {
				Children = new List<BoundedImplicitFunction3d>() { offsetMeshIso, sphere1, sphere2 }
			};
			ImplicitNaryDifference3d ndifference = new ImplicitNaryDifference3d() {
				A = offsetMeshIso,
				BSet = new List<BoundedImplicitFunction3d>() { sphere1, sphere2 }
			};
			ImplicitBlend3d blend = new ImplicitBlend3d() {
				A = sphere1, B = sphere2 
			};

			BoundedImplicitFunction3d root = intersect;

			AxisAlignedBox3d bounds = root.Bounds();
			int numcells = 64;
			MarchingCubes c = new MarchingCubes();
			c.RootMode = MarchingCubes.RootfindingModes.LerpSteps;
			c.RootModeSteps = 5;
			c.Implicit = root;
			c.Bounds = bounds;
			c.CubeSize = bounds.MaxDim / numcells;
			c.Bounds.Expand(3 * c.CubeSize);

			c.Generate();

			MeshNormals.QuickCompute(c.Mesh);
			TestUtil.WriteTestOutputMesh(c.Mesh, "marching_cubes_implicit.obj");
		}



    }
}
