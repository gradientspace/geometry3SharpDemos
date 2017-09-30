using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using g3;

namespace geometry3Test 
{
    public static class test_Dijkstra
    {
        public static bool WriteDebugMeshes = true;



        public static void test_dijkstra()
        {
            DMesh3 mesh = TestUtil.LoadTestInputMesh("bunny_solid.obj");

            int max_y = 0;
            foreach (int vid in mesh.VertexIndices()) {
                if (mesh.GetVertex(vid).y > mesh.GetVertex(max_y).y)
                    max_y = vid;
            }
            Func<int, int, float> VertexDistanceF = (v1, v2) => {
                return (float)mesh.GetVertex(v1).Distance(mesh.GetVertex(v2));
            };

            DijkstraGraphDistance dist = new DijkstraGraphDistance(mesh.MaxVertexID, false,
                mesh.IsVertex, VertexDistanceF, mesh.VtxVerticesItr);
            dist.AddSeed(max_y, 0);
            dist.Compute();
            TestUtil.SetColorsFromScalarF(mesh, dist.GetDistance, new Vector2f(0, dist.MaxDistance));
            TestUtil.WriteTestOutputMeshes(new List<IMesh>() { mesh, TestUtil.MakeMarker(mesh.GetVertex(max_y), 1, Colorf.Red) },
                "dijkstra_colormap.obj", false, true);

        }



        public static void profile_dijkstra_2b(int N = 500)
        {
            DMesh3 mesh = TestUtil.LoadTestInputMesh("sphere_50k_verts.obj");

            Func<int, int, float> VertexDistanceF = (v1, v2) => {
                return (float)mesh.GetVertex(v1).Distance(mesh.GetVertex(v2));
            };

            LocalProfiler profiler = new LocalProfiler();

            for (int k = 0; k < N; ++k) {

                profiler.Start("dijkstra_sparse");

                DijkstraGraphDistance dist = new DijkstraGraphDistance(mesh.MaxVertexID, true,
                    mesh.IsVertex, VertexDistanceF, mesh.VtxVerticesItr);
                dist.AddSeed(0, 0);
                dist.Compute();

                profiler.StopAndAccumulate("dijkstra_sparse");
            }

            GC.Collect();

            for (int k = 0; k < N; ++k) {

                profiler.Start("dijkstra_dense");

                DijkstraGraphDistance dist = new DijkstraGraphDistance(mesh.MaxVertexID, false,
                    mesh.IsVertex, VertexDistanceF, mesh.VtxVerticesItr);
                dist.AddSeed(0, 0);
                dist.Compute();

                profiler.StopAndAccumulate("dijkstra_dense");
            }


            profiler.DivideAllAccumulated(N);
            System.Console.WriteLine(profiler.AllAccumulatedTimes());
        }





        public static void profile_dijkstra_2b_reuse(int N = 500)
        {
            DMesh3 mesh = TestUtil.LoadTestInputMesh("sphere_50k_verts.obj");

            Func<int, int, float> VertexDistanceF = (v1, v2) => {
                return (float)mesh.GetVertex(v1).Distance(mesh.GetVertex(v2));
            };

            LocalProfiler profiler = new LocalProfiler();

            DijkstraGraphDistance dist_sparse = new DijkstraGraphDistance(mesh.MaxVertexID, true,
                mesh.IsVertex, VertexDistanceF, mesh.VtxVerticesItr);

            for (int k = 0; k < N; ++k) {

                profiler.Start("dijkstra_sparse");
                dist_sparse.Reset();
                dist_sparse.AddSeed(0, 0);
                dist_sparse.Compute();

                profiler.StopAndAccumulate("dijkstra_sparse");
            }

            GC.Collect();

            DijkstraGraphDistance dist_dense = new DijkstraGraphDistance(mesh.MaxVertexID, false,
                mesh.IsVertex, VertexDistanceF, mesh.VtxVerticesItr);

            for (int k = 0; k < N; ++k) {
                profiler.Start("dijkstra_dense");
                dist_dense.Reset();
                dist_dense.AddSeed(0, 0);
                dist_dense.Compute();
                profiler.StopAndAccumulate("dijkstra_dense");
            }


            profiler.DivideAllAccumulated(N);
            System.Console.WriteLine(profiler.AllAccumulatedTimes());
        }






        public static void test_local_param()
        {
            //DMesh3 mesh = TestUtil.LoadTestInputMesh("plane_250v.obj");
            //DMesh3 mesh = TestUtil.LoadTestInputMesh("hemisphere_nicemesh_3k.obj");
            DMesh3 mesh = TestUtil.LoadTestInputMesh("bunny_open_base.obj");

            mesh.EnableVertexUVs(Vector2f.Zero);

            DMeshAABBTree3 spatial = new DMeshAABBTree3(mesh);
            spatial.Build();

            //int tid = spatial.FindNearestTriangle(Vector3d.Zero);
            //Frame3f seedF = new Frame3f(Vector3d.Zero, Vector3d.AxisY);
            int tid = 3137;
            Frame3f seedF = mesh.GetTriFrame(tid);

            Index3i seedNbrs = mesh.GetTriangle(tid);

            MeshLocalParam param = new MeshLocalParam(mesh.MaxVertexID,
                mesh.GetVertexf, mesh.GetVertexNormal, mesh.VtxVerticesItr);
            param.ComputeToMaxDistance(seedF, seedNbrs, float.MaxValue);

            float fR = param.MaxUVDistance;
            param.TransformUV(0.5f / fR, 0.5f*Vector2f.One);

            param.ApplyUVs((vid, uv) => { mesh.SetVertexUV(vid, uv); });

            TestUtil.SetColorsFromScalarF(mesh, (vid) => { return param.GetUV(vid).Distance(0.5f*Vector2f.One); }, new Vector2f(0, 0.5f));

            OBJWriter writer = new OBJWriter();
            var s = new System.IO.StreamWriter(Program.TEST_OUTPUT_PATH + "mesh_local_param.obj", false);
            List<WriteMesh> wm = new List<WriteMesh>() { new WriteMesh(mesh) };
            WriteOptions opt = new WriteOptions() {
                bCombineMeshes = false, bWriteGroups = false, bPerVertexColors = true, bPerVertexUVs = true,
                AsciiHeaderFunc = () => { return "mttllib checkerboard.mtl\r\nusemtl checkerboard\r\n"; }
            };
            writer.Write(s, wm, opt);
            s.Close();
        }





        public static void test_uv_insert_segment()
        {
            DMesh3 mesh = TestUtil.LoadTestInputMesh("plane_250v.obj");
            mesh.EnableVertexUVs(Vector2f.Zero);

            MeshTransforms.ConvertYUpToZUp(mesh);

            DMeshAABBTree3 spatial = new DMeshAABBTree3(mesh);
            spatial.Build();
            int tid = spatial.FindNearestTriangle(Vector3d.Zero);

            //Polygon2d poly = Polygon2d.MakeRectangle(Vector2d.Zero, 5, 5);
            Polygon2d poly = Polygon2d.MakeCircle(5, 13);
            //PolyLine2d poly = new PolyLine2d( new Vector2d[] { -5 * Vector2d.One, 5 * Vector2d.One });


            //int tri_edge0 = mesh.GetTriEdge(tid, 0);
            //Index2i edge0_tris = mesh.GetEdgeT(tri_edge0);
            //Index2i edge0_verts = mesh.GetEdgeV(tri_edge0);
            //Vector3d v0 = mesh.GetVertex(edge0_verts.a), v1 = mesh.GetVertex(edge0_verts.b);
            //Vector3d c = mesh.GetTriCentroid(tid);
            //Polygon2d poly = new Polygon2d(new Vector2d[] {
            //    Vector2d.Lerp(v0.xy, v1.xy, -0.25),
            //    Vector2d.Lerp(v0.xy, v1.xy, 1.5),
            //    c.xy
            //});

            MeshInsertUVPolyCurve insert = new MeshInsertUVPolyCurve(mesh, poly);
            insert.Apply();



            Polygon2d test_poly = new Polygon2d();
            List<double> distances = new List<double>();
            List<int> nearests = new List<int>();
            for (int i = 0; i < insert.Loops[0].VertexCount; ++i) {
                Vector2d v = mesh.GetVertex(insert.Loops[0].Vertices[i]).xy;
                test_poly.AppendVertex(v);
                int iNear; double fNear;
                distances.Add(poly.DistanceSquared(v, out iNear, out fNear));
                nearests.Add(iNear);
            }

            System.Console.WriteLine("inserted loop poly has {0} edges", insert.Loops[0].EdgeCount);

            // find a triangle connected to loop that is inside the polygon
            //   [TODO] maybe we could be a bit more robust about this? at least
            //   check if triangle is too degenerate...
            int seed_tri = -1;
            for (int i = 0; i < insert.Loops[0].EdgeCount; ++i) {
                Index2i et = mesh.GetEdgeT(insert.Loops[0].Edges[i]);
                Vector3d ca = mesh.GetTriCentroid(et.a);
                bool in_a = poly.Contains(ca.xy);
                Vector3d cb = mesh.GetTriCentroid(et.b);
                bool in_b = poly.Contains(cb.xy);
                if (in_a && in_b == false) {
                    seed_tri = et.a;
                    break;
                } else if (in_b && in_a == false) {
                    seed_tri = et.b;
                    break;
                }
            }
            Util.gDevAssert(seed_tri != -1);

            // flood-fill inside loop
            HashSet<int> loopEdges = new HashSet<int>(insert.Loops[0].Edges);
            MeshFaceSelection sel = new MeshFaceSelection(mesh);
            sel.FloodFill(seed_tri, null, (eid) => { return loopEdges.Contains(eid) == false; });

            // delete inside loop
            MeshEditor editor = new MeshEditor(mesh);
            editor.RemoveTriangles(sel, true);


            MeshTransforms.ConvertZUpToYUp(mesh);

            TestUtil.WriteTestOutputMesh(mesh, "insert_uv_segment.obj");



            //OBJWriter writer = new OBJWriter();
            //var s = new System.IO.StreamWriter(Program.TEST_OUTPUT_PATH + "mesh_local_param.obj", false);
            //List<WriteMesh> wm = new List<WriteMesh>() { new WriteMesh(mesh) };
            //WriteOptions opt = new WriteOptions() {
            //    bCombineMeshes = false, bWriteGroups = false, bPerVertexColors = true, bPerVertexUVs = true,
            //    AsciiHeaderFunc = () => { return "mttllib checkerboard.mtl\r\nusemtl checkerboard\r\n"; }
            //};
            //writer.Write(s, wm, opt);
            //s.Close();
        }











        public static void test_uv_insert_string()
        {
            DMesh3 mesh = TestUtil.LoadTestInputMesh("plane_xy_25x25.obj");
            mesh.EnableVertexUVs(Vector2f.Zero);

            DMeshAABBTree3 spatial = new DMeshAABBTree3(mesh);
            spatial.Build();
            int tid = spatial.FindNearestTriangle(Vector3d.Zero);

            PolygonFont2d font = PolygonFont2d.ReadFont("c:\\scratch\\font.bin");

            //List<GeneralPolygon2d> letter = new List<GeneralPolygon2d>(font.Characters.First().Value.Polygons);
            //double targetWidth = 20.0f;
            List<GeneralPolygon2d> letter = font.GetCharacter('a');
            double targetWidth = 10.0f;

            AxisAlignedBox2d bounds = font.MaxBounds;
            Vector2d center = bounds.Center;
            Vector2d scale2d = (targetWidth / font.MaxBounds.Width) * new Vector2d(1, 1);


            for (int li = 0; li < letter.Count; ++li) {
                GeneralPolygon2d gp = new GeneralPolygon2d(letter[li]);
                gp.Scale(scale2d, center);
                gp.Translate(-center);
                letter[li] = gp;
            }


            List<MeshFaceSelection> letter_interiors = new List<MeshFaceSelection>();

            bool bSimplify = true;
            for (int li = 0; li < letter.Count; ++li) {
                GeneralPolygon2d gp = letter[li];

                MeshInsertUVPolyCurve outer = new MeshInsertUVPolyCurve(mesh, gp.Outer);
                Util.gDevAssert(outer.Validate() == ValidationStatus.Ok);
                outer.Apply();
                if (bSimplify)
                    outer.Simplify();

                List<MeshInsertUVPolyCurve> holes = new List<MeshInsertUVPolyCurve>(gp.Holes.Count);
                for (int hi = 0; hi < gp.Holes.Count; ++hi) {
                    MeshInsertUVPolyCurve insert = new MeshInsertUVPolyCurve(mesh, gp.Holes[hi]);
                    Util.gDevAssert(insert.Validate() == ValidationStatus.Ok);
                    insert.Apply();
                    if (bSimplify)
                        insert.Simplify();
                    holes.Add(insert);
                }


                // find a triangle connected to loop that is inside the polygon
                //   [TODO] maybe we could be a bit more robust about this? at least
                //   check if triangle is too degenerate...
                int seed_tri = -1;
                EdgeLoop outer_loop = outer.Loops[0];
                for (int i = 0; i < outer_loop.EdgeCount; ++i) {
                    if (!mesh.IsEdge(outer_loop.Edges[i]))
                        continue;

                    Index2i et = mesh.GetEdgeT(outer_loop.Edges[i]);
                    Vector3d ca = mesh.GetTriCentroid(et.a);
                    bool in_a = gp.Outer.Contains(ca.xy);
                    Vector3d cb = mesh.GetTriCentroid(et.b);
                    bool in_b = gp.Outer.Contains(cb.xy);
                    if (in_a && in_b == false) {
                        seed_tri = et.a;
                        break;
                    } else if (in_b && in_a == false) {
                        seed_tri = et.b;
                        break;
                    }
                }
                Util.gDevAssert(seed_tri != -1);
                
                // make list of all outer & hole edges
                HashSet<int> loopEdges = new HashSet<int>(outer_loop.Edges);
                foreach (var insertion in holes) {
                    foreach (int eid in insertion.Loops[0].Edges)
                        loopEdges.Add(eid);
                }

                // flood-fill inside loop from seed triangle
                MeshFaceSelection sel = new MeshFaceSelection(mesh);
                sel.FloodFill(seed_tri, null, (eid) => { return loopEdges.Contains(eid) == false; });
                letter_interiors.Add(sel);
            }

            // extrude regions
            Func<Vector3d, Vector3f, int, Vector3d> OffsetF = (v, n, i) => {
                return v + Vector3d.AxisZ;
            };
            foreach (var interior in letter_interiors) {
                MeshExtrudeFaces extrude = new MeshExtrudeFaces(mesh, interior);
                extrude.ExtrudedPositionF = OffsetF;
                extrude.Extrude();
            }

            TestUtil.WriteTestOutputMesh(mesh, "insert_uv_string.obj");

        }




    }
}
