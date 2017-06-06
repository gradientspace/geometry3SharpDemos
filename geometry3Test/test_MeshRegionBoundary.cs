using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using g3;

namespace geometry3Test
{
    public static class test_MeshRegionBoundary
    {

        public static DMesh3 MakeBoundaryTestMesh(int n, out string name)
        {
            name = "unknown";
            if (n == 0) {
                name = "sphere_3cleangroups_1hole";
                return TestUtil.LoadTestMesh(Program.TEST_FILES_PATH + "sphere_3cleangroups_1hole.obj");
            } else if (n == 1 ) {
                name = "sphere_bowtie_groups";
                return TestUtil.LoadTestMesh(Program.TEST_FILES_PATH + "sphere_bowtie_groups.obj");
            } else if (n == 2) {
                name = "sphere_bowtie_groups_2";
                return TestUtil.LoadTestMesh(Program.TEST_FILES_PATH + "sphere_bowtie_groups_2.obj");
            } else if (n == 3) {
                name = "sphere_2groups_complexholes";
                return TestUtil.LoadTestMesh(Program.TEST_FILES_PATH + "sphere_2groups_complexholes.obj");
            } 
            throw new Exception("test_MeshBoundary.MakeBoundaryTestMesh: unknown mesh case");
        }
        public static int NumTestCases { get { return 4; } }




        public static void test_region_boundary()
        {
            List<int> cases = new List<int>() { 0, 1, 2, 3 };

            foreach ( int num in cases ) { 

                string name;
                DMesh3 mesh = MakeBoundaryTestMesh(num, out name);

                int[][] groups = FaceGroupUtil.FindTriangleSetsByGroup(mesh, 0);

                System.Console.WriteLine("case {0}:", name);
                for (int k = 0; k < groups.Length; ++k) {
                    int gid = mesh.GetTriangleGroup(groups[k][0]);
                    MeshRegionBoundaryLoops loops = new MeshRegionBoundaryLoops(mesh, groups[k]);
                    System.Console.WriteLine("  gid {0} : found {1} loops", gid, loops.Loops.Count);
                }

            }
        }


    }
}
