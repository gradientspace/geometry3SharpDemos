using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using g3;

namespace geometry3Test
{
    public class test_DMesh3
    {
        public static void basic_tests()
        {
            System.Console.WriteLine("DMesh3:basic_tests() starting");

            DMesh3 tmp = new DMesh3();
            CappedCylinderGenerator cylgen = new CappedCylinderGenerator();
            cylgen.Generate();
            cylgen.MakeMesh(tmp);

            foreach (int vid in tmp.VertexIndices())
                System.Console.WriteLine(tmp.GetVertex(vid).ToString());

            System.Console.WriteLine("cylinder ok");

        }

    }
}
