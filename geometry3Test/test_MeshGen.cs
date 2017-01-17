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
        public static void test_basic_generators()
        {
            TrivialDiscGenerator disc_gen = new TrivialDiscGenerator();
            disc_gen.Generate();
            SimpleMesh disc_mesh = new SimpleMesh();
            disc_gen.MakeMesh(disc_mesh);

            TestUtil.WriteDebugMesh(disc_mesh, "__g3Test_disc.obj");


            TrivialRectGenerator rect_gen = new TrivialRectGenerator();
            rect_gen.Generate();
            SimpleMesh rect_mesh = new SimpleMesh();
            rect_gen.MakeMesh(rect_mesh);

            TestUtil.WriteDebugMesh(rect_mesh, "__g3Test_rect.obj");



            PuncturedDiscGenerator punc_disc_gen = new PuncturedDiscGenerator();
            punc_disc_gen.Generate();
            SimpleMesh punc_disc_mesh = new SimpleMesh();
            punc_disc_gen.MakeMesh(punc_disc_mesh);

            TestUtil.WriteDebugMesh(punc_disc_mesh, "__g3Test_punctured_disc.obj");

        }
    }
}
