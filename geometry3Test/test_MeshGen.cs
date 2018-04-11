using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using g3;

namespace geometry3Test
{
    public static class test_MeshGen
    {
        public static void WriteGeneratedMesh(MeshGenerator gen, string sFilename)
        {
            gen.Generate();
            DMesh3 mesh = gen.MakeDMesh();
            TestUtil.WriteTestOutputMesh(mesh, sFilename, true, true, true);
            string tex_header = "mtllib checker.mtl\r\nusemtl checker\r\n";
            string cur = File.ReadAllText(Program.TEST_OUTPUT_PATH+sFilename);
            File.WriteAllText(Program.TEST_OUTPUT_PATH+sFilename, tex_header + cur);
        }

        public static void test_basic_generators()
        {
            TrivialDiscGenerator disc_gen = new TrivialDiscGenerator();
            WriteGeneratedMesh(disc_gen, "meshgen_Disc.obj");

            TrivialRectGenerator rect_gen = new TrivialRectGenerator();
            WriteGeneratedMesh(rect_gen, "meshgen_Rect.obj");

            GriddedRectGenerator gridrect_gen = new GriddedRectGenerator();
            WriteGeneratedMesh(gridrect_gen, "meshgen_GriddedRect.obj");

            PuncturedDiscGenerator punc_disc_gen = new PuncturedDiscGenerator();
            WriteGeneratedMesh(punc_disc_gen, "meshgen_PuncturedDisc.obj");

            TrivialBox3Generator box_gen = new TrivialBox3Generator();
            Frame3f f = Frame3f.Identity;
            f.Rotate(Quaternionf.AxisAngleD(Vector3f.AxisY, 45.0f));
            f.Rotate(Quaternionf.AxisAngleD(Vector3f.AxisZ, 45.0f));
            box_gen.Box = new Box3d(f.Origin, f.X, f.Y, f.Z, new Vector3d(3, 2, 1));
            WriteGeneratedMesh(box_gen, "meshgen_TrivialBox_shared.obj");
            box_gen.NoSharedVertices = true;
            WriteGeneratedMesh(box_gen, "meshgen_TrivialBox_noshared.obj");


            RoundRectGenerator roundrect_gen = new RoundRectGenerator();
            roundrect_gen.Width = 2;
            WriteGeneratedMesh(roundrect_gen, "meshgen_RoundRect.obj");


            GridBox3Generator gridbox_gen = new GridBox3Generator();
            WriteGeneratedMesh(gridbox_gen, "meshgen_GridBox_shared.obj");
            gridbox_gen.NoSharedVertices = true;
            WriteGeneratedMesh(gridbox_gen, "meshgen_GridBox_noshared.obj");

            Sphere3Generator_NormalizedCube normcube_gen = new Sphere3Generator_NormalizedCube();
            WriteGeneratedMesh(normcube_gen, "meshgen_Sphere_NormalizedCube_shared.obj");
            normcube_gen.NoSharedVertices = true;
            normcube_gen.Box = new Box3d(new Frame3f(Vector3f.One, Vector3f.One), Vector3d.One * 1.3);
            WriteGeneratedMesh(normcube_gen, "meshgen_Sphere_NormalizedCube_noshared.obj");


            TubeGenerator tube_gen = new TubeGenerator() {
                Vertices = new List<Vector3d>() { Vector3d.Zero, Vector3d.AxisX, 2 * Vector3d.AxisX, 3 * Vector3d.AxisX },
                Polygon = Polygon2d.MakeCircle(1, 16)
            };
            WriteGeneratedMesh(tube_gen, "meshgen_TubeGenerator.obj");

            tube_gen.Polygon.Translate(Vector2d.One);
            tube_gen.CapCenter = Vector2d.One;
            WriteGeneratedMesh(tube_gen, "meshgen_TubeGenerator_shifted.obj");
        }





        public static void test_tube_generator()
        {
            Polygon2d circle_path = Polygon2d.MakeCircle(50, 64);
            PolyLine2d arc_path = new PolyLine2d(circle_path.Vertices.Take(circle_path.VertexCount/2));
            Polygon2d irreg_path = new Polygon2d();
            for ( int k = 0; k < circle_path.VertexCount; ++k ) {
                irreg_path.AppendVertex(circle_path[k]);
                k += k / 2;
            }
            PolyLine2d irreg_arc_path = new PolyLine2d(irreg_path.Vertices.Take(circle_path.VertexCount-1));

            Polygon2d square_profile = Polygon2d.MakeCircle(7, 32);
            square_profile.Translate(4*Vector2d.One);
            //square_profile[0] = 20 * square_profile[0].Normalized;

            bool no_shared = true;

            WriteGeneratedMesh(
                new TubeGenerator(circle_path, Frame3f.Identity, square_profile) { WantUVs = true, NoSharedVertices = no_shared },
                "tubegen_loop_standarduv.obj");

            WriteGeneratedMesh(
                new TubeGenerator(irreg_path, Frame3f.Identity, square_profile) { WantUVs = true, NoSharedVertices = no_shared },
                "tubegen_irregloop_standarduv.obj");

            WriteGeneratedMesh(
                new TubeGenerator(arc_path, Frame3f.Identity, square_profile) { WantUVs = true, NoSharedVertices = no_shared },
                "tubegen_arc_standarduv.obj");

            WriteGeneratedMesh(
                new TubeGenerator(irreg_arc_path, Frame3f.Identity, square_profile) { WantUVs = true, NoSharedVertices = no_shared },
                "tubegen_irregarc_standarduv.obj");
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





        public static void test_marching_cubes_demos()
        {
            // generateMeshF() meshes the input implicit function at
            // the given cell resolution, and writes out the resulting mesh  
            Action<BoundedImplicitFunction3d, int, string> generateMeshF = (root, numcells, path) => {
                MarchingCubes c = new MarchingCubes();
                c.Implicit = root;
                c.RootMode = MarchingCubes.RootfindingModes.LerpSteps;      // cube-edge convergence method
                c.RootModeSteps = 5;                                        // number of iterations
                c.Bounds = root.Bounds();
                c.CubeSize = c.Bounds.MaxDim / numcells;
                c.Bounds.Expand(3 * c.CubeSize);                            // leave a buffer of cells
                c.Generate();
                MeshNormals.QuickCompute(c.Mesh);                           // generate normals
                StandardMeshWriter.WriteMesh(path, c.Mesh, WriteOptions.Defaults);   // write mesh
            };

            // meshToImplicitF() generates a narrow-band distance-field and
            // returns it as an implicit surface, that can be combined with other implicits           
            Func<DMesh3, int, double, BoundedImplicitFunction3d> meshToImplicitF = (meshIn, numcells, max_offset) => {
                double meshCellsize = meshIn.CachedBounds.MaxDim / numcells;
                MeshSignedDistanceGrid levelSet = new MeshSignedDistanceGrid(meshIn, meshCellsize);
                levelSet.ExactBandWidth = (int)(max_offset / meshCellsize) + 1;
                levelSet.Compute();
                return new DenseGridTrilinearImplicit(levelSet.Grid, levelSet.GridOrigin, levelSet.CellSize);
            };

            // meshToBlendImplicitF() computes the full distance-field grid for the input 
            // mesh. The bounds are expanded quite a bit to allow for blending,
            // probably more than necessary in most cases
            Func<DMesh3, int, BoundedImplicitFunction3d> meshToBlendImplicitF = (meshIn, numcells) => {
                double meshCellsize = meshIn.CachedBounds.MaxDim / numcells;
                MeshSignedDistanceGrid levelSet = new MeshSignedDistanceGrid(meshIn, meshCellsize);
                levelSet.ExpandBounds = meshIn.CachedBounds.Diagonal * 0.25;        // need some values outside mesh
                levelSet.ComputeMode = MeshSignedDistanceGrid.ComputeModes.FullGrid;
                levelSet.Compute();
                return new DenseGridTrilinearImplicit(levelSet.Grid, levelSet.GridOrigin, levelSet.CellSize);
            };



            // generate union/difference/intersection of sphere and cube

            ImplicitSphere3d sphere = new ImplicitSphere3d() {
                Origin = Vector3d.Zero, Radius = 1.0
            };
            ImplicitBox3d box = new ImplicitBox3d() {
                Box = new Box3d(new Frame3f(Vector3f.AxisX), 0.5 * Vector3d.One)
            };
            generateMeshF(new ImplicitUnion3d() { A = sphere, B = box }, 128, "c:\\demo\\union.obj");
            generateMeshF(new ImplicitDifference3d() { A = sphere, B = box }, 128, "c:\\demo\\difference.obj");
            generateMeshF(new ImplicitIntersection3d() { A = sphere, B = box }, 128, "c:\\demo\\intersection.obj");


            // generate bunny offset surfaces

            //double offset = 0.2f;
            //DMesh3 mesh = TestUtil.LoadTestInputMesh("bunny_solid.obj");
            //MeshTransforms.Scale(mesh, 3.0 / mesh.CachedBounds.MaxDim);
            //BoundedImplicitFunction3d meshImplicit = meshToImplicitF(mesh, 64, offset);

            //generateMeshF(meshImplicit, 128, "c:\\demo\\mesh.obj");
            //generateMeshF(new ImplicitOffset3d() { A = meshImplicit, Offset = offset }, 128, "c:\\demo\\mesh_outset.obj");
            //generateMeshF(new ImplicitOffset3d() { A = meshImplicit, Offset = -offset }, 128, "c:\\demo\\mesh_inset.obj");


            // compare offset of sharp and smooth union

            //var smooth_union = new ImplicitSmoothDifference3d() { A = sphere, B = box };
            //generateMeshF(smooth_union, 128, "c:\\demo\\smooth_union.obj");
            //generateMeshF(new ImplicitOffset3d() { A = smooth_union, Offset = 0.2 }, 128, "c:\\demo\\smooth_union_offset.obj");

            //var union = new ImplicitUnion3d() { A = sphere, B = box };
            //generateMeshF(new ImplicitOffset3d() { A = union, Offset = offset }, 128, "c:\\demo\\union_offset.obj");


            // blending

            //ImplicitSphere3d sphere1 = new ImplicitSphere3d() {
            //    Origin = Vector3d.Zero, Radius = 1.0
            //};
            //ImplicitSphere3d sphere2 = new ImplicitSphere3d() {
            //    Origin = 1.5 * Vector3d.AxisX, Radius = 1.0
            //};
            //generateMeshF(new ImplicitBlend3d() { A = sphere1, B = sphere2, Blend = 1.0 }, 128, "c:\\demo\\blend_1.obj");
            //generateMeshF(new ImplicitBlend3d() { A = sphere1, B = sphere2, Blend = 4.0 }, 128, "c:\\demo\\blend_4.obj");
            //generateMeshF(new ImplicitBlend3d() { A = sphere1, B = sphere2, Blend = 16.0 }, 128, "c:\\demo\\blend_16.obj");
            //generateMeshF(new ImplicitBlend3d() { A = sphere1, B = sphere2, Blend = 64.0 }, 128, "c:\\demo\\blend_64.obj");
            //sphere1.Radius = sphere2.Radius = 2.0f;
            //sphere2.Origin = 1.5 * sphere1.Radius * Vector3d.AxisX;
            //generateMeshF(new ImplicitBlend3d() { A = sphere1, B = sphere2, Blend = 1.0 }, 128, "c:\\demo\\blend_2x_1.obj");
            //generateMeshF(new ImplicitBlend3d() { A = sphere1, B = sphere2, Blend = 4.0 }, 128, "c:\\demo\\blend_2x_4.obj");
            //generateMeshF(new ImplicitBlend3d() { A = sphere1, B = sphere2, Blend = 16.0 }, 128, "c:\\demo\\blend_2x_16.obj");
            //generateMeshF(new ImplicitBlend3d() { A = sphere1, B = sphere2, Blend = 64.0 }, 128, "c:\\demo\\blend_2x_64.obj");


            // mesh blending

            //DMesh3 mesh1 = TestUtil.LoadTestInputMesh("bunny_solid.obj");
            //MeshTransforms.Scale(mesh1, 3.0 / mesh1.CachedBounds.MaxDim);
            //DMesh3 mesh2 = new DMesh3(mesh1);
            //MeshTransforms.Rotate(mesh2, mesh2.CachedBounds.Center, Quaternionf.AxisAngleD(Vector3f.OneNormalized, 45.0f));

            //var meshImplicit1 = meshToImplicitF(mesh1, 64, 0);
            //var meshImplicit2 = meshToImplicitF(mesh2, 64, 0);
            //generateMeshF(new ImplicitBlend3d() { A = meshImplicit1, B = meshImplicit2, Blend = 0.0 }, 256, "c:\\demo\\blend_mesh_union.obj");
            //generateMeshF(new ImplicitBlend3d() { A = meshImplicit1, B = meshImplicit2, Blend = 10.0 }, 256, "c:\\demo\\blend_mesh_bad.obj");

            //var meshFullImplicit1 = meshToBlendImplicitF(mesh1, 64);
            //var meshFullImplicit2 = meshToBlendImplicitF(mesh2, 64);
            //generateMeshF(new ImplicitBlend3d() { A = meshFullImplicit1, B = meshFullImplicit2, Blend = 0.0 }, 256, "c:\\demo\\blend_mesh_union.obj");
            //generateMeshF(new ImplicitBlend3d() { A = meshFullImplicit1, B = meshFullImplicit2, Blend = 1.0 }, 256, "c:\\demo\\blend_mesh_1.obj");
            //generateMeshF(new ImplicitBlend3d() { A = meshFullImplicit1, B = meshFullImplicit2, Blend = 10.0 }, 256, "c:\\demo\\blend_mesh_10.obj");
            //generateMeshF(new ImplicitBlend3d() { A = meshFullImplicit1, B = meshFullImplicit2, Blend = 50.0 }, 256, "c:\\demo\\blend_mesh_100.obj");


            //DMesh3 mesh = TestUtil.LoadTestInputMesh("bunny_solid.obj");
            //MeshTransforms.Scale(mesh, 3.0 / mesh.CachedBounds.MaxDim);
            //MeshTransforms.Translate(mesh, -mesh.CachedBounds.Center);
            //Reducer r = new Reducer(mesh);
            //r.ReduceToTriangleCount(100);

            //double radius = 0.1;
            //List<BoundedImplicitFunction3d> Lines = new List<BoundedImplicitFunction3d>();
            //foreach (Index4i edge_info in mesh.Edges()) {
            //    var segment = new Segment3d(mesh.GetVertex(edge_info.a), mesh.GetVertex(edge_info.b));
            //    Lines.Add(new ImplicitLine3d() { Segment = segment, Radius = radius });
            //}
            //ImplicitNaryUnion3d unionN = new ImplicitNaryUnion3d() { Children = Lines };
            //generateMeshF(unionN, 128, "c:\\demo\\mesh_edges.obj");

            //radius = 0.05;
            //List<BoundedImplicitFunction3d> Elements = new List<BoundedImplicitFunction3d>();
            //foreach (int eid in mesh.EdgeIndices()) {
            //    var segment = new Segment3d(mesh.GetEdgePoint(eid, 0), mesh.GetEdgePoint(eid, 1));
            //    Elements.Add(new ImplicitLine3d() { Segment = segment, Radius = radius });
            //}
            //foreach (Vector3d v in mesh.Vertices())
            //    Elements.Add(new ImplicitSphere3d() { Origin = v, Radius = 2 * radius });
            //generateMeshF(new ImplicitNaryUnion3d() { Children = Elements }, 256, "c:\\demo\\mesh_edges_and_vertices.obj");


            //double lattice_radius = 0.05;
            //double lattice_spacing = 0.4;
            //double shell_thickness = 0.05;
            //int mesh_resolution = 64;   // set to 256 for image quality

            //var shellMeshImplicit = meshToImplicitF(mesh, 128, shell_thickness);
            //double max_dim = mesh.CachedBounds.MaxDim;
            //AxisAlignedBox3d bounds = new AxisAlignedBox3d(mesh.CachedBounds.Center, max_dim / 2);
            //bounds.Expand(2 * lattice_spacing);
            //AxisAlignedBox2d element = new AxisAlignedBox2d(lattice_spacing);
            //AxisAlignedBox2d bounds_xy = new AxisAlignedBox2d(bounds.Min.xy, bounds.Max.xy);
            //AxisAlignedBox2d bounds_xz = new AxisAlignedBox2d(bounds.Min.xz, bounds.Max.xz);
            //AxisAlignedBox2d bounds_yz = new AxisAlignedBox2d(bounds.Min.yz, bounds.Max.yz);

            //List<BoundedImplicitFunction3d> Tiling = new List<BoundedImplicitFunction3d>();
            //foreach (Vector2d uv in TilingUtil.BoundedRegularTiling2(element, bounds_xy, 0)) {
            //    Segment3d seg = new Segment3d(new Vector3d(uv.x, uv.y, bounds.Min.z), new Vector3d(uv.x, uv.y, bounds.Max.z));
            //    Tiling.Add(new ImplicitLine3d() { Segment = seg, Radius = lattice_radius });
            //}
            //foreach (Vector2d uv in TilingUtil.BoundedRegularTiling2(element, bounds_xz, 0)) {
            //    Segment3d seg = new Segment3d(new Vector3d(uv.x, bounds.Min.y, uv.y), new Vector3d(uv.x, bounds.Max.y, uv.y));
            //    Tiling.Add(new ImplicitLine3d() { Segment = seg, Radius = lattice_radius });
            //}
            //foreach (Vector2d uv in TilingUtil.BoundedRegularTiling2(element, bounds_yz, 0)) {
            //    Segment3d seg = new Segment3d(new Vector3d(bounds.Min.x, uv.x, uv.y), new Vector3d(bounds.Max.x, uv.x, uv.y));
            //    Tiling.Add(new ImplicitLine3d() { Segment = seg, Radius = lattice_radius });
            //}
            //ImplicitNaryUnion3d lattice = new ImplicitNaryUnion3d() { Children = Tiling };
            //generateMeshF(lattice, 128, "c:\\demo\\lattice.obj");

            //ImplicitIntersection3d lattice_clipped = new ImplicitIntersection3d() { A = lattice, B = shellMeshImplicit };
            //generateMeshF(lattice_clipped, mesh_resolution, "c:\\demo\\lattice_clipped.obj");

            //var shell = new ImplicitDifference3d() {
            //    A = shellMeshImplicit, B = new ImplicitOffset3d() { A = shellMeshImplicit, Offset = -shell_thickness }
            //};
            //var shell_cut = new ImplicitDifference3d() {
            //    A = shell, B = new ImplicitAxisAlignedBox3d() { AABox = new AxisAlignedBox3d(Vector3d.Zero, max_dim / 2, 0.4, max_dim / 2) }
            //};
            //generateMeshF(new ImplicitUnion3d() { A = lattice_clipped, B = shell_cut }, mesh_resolution, "c:\\demo\\lattice_result.obj");

        }



    }
}
