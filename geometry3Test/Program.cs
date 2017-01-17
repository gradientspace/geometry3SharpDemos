using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using g3;

namespace geometry3Test
{
    static class Program
    {
        public const string TEST_FILES_PATH = "..\\..\\test_files\\";
        public const string TEST_OUTPUT_PATH = "..\\..\\test_output\\";


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            //test_VectorTypes.test_rcvector();
			//test_MeshIO.test_simple_obj();
			//test_Math.test_VectorTanCot();


			//test_dmesh();

			test_Remesher.WriteDebugMeshes = true;
            //test_Remesher.test_basic_closed_remesh();
            //test_Remesher.test_remesh_smoothing();
            //test_Remesher.test_remesh_constraints_fixedverts();
            //test_Remesher.test_remesh_constraints_vertcurves();


            //test_Spatial.test_AABBTree_basic();
            //test_Spatial.test_AABBTree_TriDist();
            //test_Spatial.test_AABBTree_profile();


            test_MeshGen.test_basic_generators();


            System.Console.WriteLine("Done tests, press enter key to exit");
            System.Console.ReadLine();
        }


		static void test_dmesh() {
			test_DMesh3.basic_tests();
            test_DMesh3.test_remove();


			int split_rounds = 100;
			test_DMesh3.split_tests(true, split_rounds);
			test_DMesh3.split_tests(false, split_rounds);

			int flip_rounds = 100;
			test_DMesh3.flip_tests(true, flip_rounds);
			test_DMesh3.flip_tests(false, flip_rounds);

			int collapse_rounds = 1000;
			test_DMesh3.collapse_tests(true, collapse_rounds);
			test_DMesh3.collapse_tests(false, collapse_rounds);

			test_DMesh3.collapse_test_convergence_cyl_noshared();
			test_DMesh3.collapse_test_closed_mesh();
			test_DMesh3.collapse_test_convergence_opencyl();
		}

    }
}
