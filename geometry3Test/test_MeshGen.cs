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
            SimpleMesh mesh = new SimpleMesh();
            gen.MakeMesh(mesh);
            TestUtil.WriteDebugMesh(mesh, sFilename);
        }

        public static void test_basic_generators()
        {
            TrivialDiscGenerator disc_gen = new TrivialDiscGenerator();
            disc_gen.Generate();
            WriteGeneratedMesh(disc_gen, "__g3Test_disc.obj");

            TrivialRectGenerator rect_gen = new TrivialRectGenerator();
            rect_gen.Generate();
            WriteGeneratedMesh(rect_gen, "__g3Test_rect.obj");


            PuncturedDiscGenerator punc_disc_gen = new PuncturedDiscGenerator();
            punc_disc_gen.Generate();
            WriteGeneratedMesh(punc_disc_gen, "__g3Test_punctured_disc.obj");

            TrivialBox3Generator box_gen = new TrivialBox3Generator();
            Frame3f f = Frame3f.Identity;
            f.Rotate(Quaternionf.AxisAngleD(Vector3f.AxisY, 45.0f));
            f.Rotate(Quaternionf.AxisAngleD(Vector3f.AxisZ, 45.0f));
            box_gen.Box = new Box3d(f.Origin, f.X, f.Y, f.Z, new Vector3d(3, 2, 1));
            //box_gen.NoSharedVertices = true;
            box_gen.Generate();
            WriteGeneratedMesh(box_gen, "__g3Test_trivial_box.obj");



            RoundRectGenerator roundrect_gen = new RoundRectGenerator();
            roundrect_gen.Width = 2;
            roundrect_gen.Generate();
            WriteGeneratedMesh(roundrect_gen, "__g3Test_round_rect.obj");
        }
    }
}
