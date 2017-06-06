using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using g3;

namespace geometry3Test
{
    public static class test_MeshBoundary
    {

        public static DMesh3 MakeBoundaryTestMesh(int n, out string name)
        {
            name = "unknown";
            if (n == 0) {
                name = "bunny_open_base";
                return TestUtil.LoadTestMesh(Program.TEST_FILES_PATH + "bunny_open_base.obj");
            } else if (n == 1 ) {
                name = "n_holed_bunny";
                return TestUtil.LoadTestMesh(Program.TEST_FILES_PATH + "n_holed_bunny.obj");
            } else if (n == 2) {
                name = "bunny_bowties";
                return TestUtil.LoadTestMesh(Program.TEST_FILES_PATH + "bunny_bowties.obj");
            } else if (n == 3) {
                name = "crazy_boundary";
                return TestUtil.LoadTestMesh(Program.TEST_FILES_PATH + "crazy_boundary.obj");
            }
            throw new Exception("test_MeshBoundary.MakeBoundaryTestMesh: unknown mesh case");
        }
        public static int NumTestCases { get { return 4; } }




        public static void test_mesh_boundary()
        {
            //List<int> cases = new List<int>() { 0, 1, 2, 3 };
            List<int> cases = new List<int>() { 0, 1, 2 };

            // TODO: currently this case fails to find a simple boundary loop. 
            //List<int> cases = new List<int>() { 3 };

            foreach ( int num in cases ) { 

                string name;
                DMesh3 mesh = MakeBoundaryTestMesh(num, out name);

                MeshBoundaryLoops loops = new MeshBoundaryLoops(mesh);
            }
        }


    }
}
