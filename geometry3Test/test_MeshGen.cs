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
            normcube_gen.Generate();
            WriteGeneratedMesh(normcube_gen, "meshgen_Sphere_NormalizedCube_noshared.obj");
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

            Util.gDevAssert(origMesh.IsSameMesh(m1));
            Util.gDevAssert(origMesh.IsSameMesh(m2));
           

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

    }
}
