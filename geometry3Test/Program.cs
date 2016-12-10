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

            test_DMesh3.basic_tests();


			int split_rounds = 100;
			test_DMesh3.split_tests(true, split_rounds);
			test_DMesh3.split_tests(false, split_rounds);

			int flip_rounds = 100;
			test_DMesh3.flip_tests(true, flip_rounds);
			test_DMesh3.flip_tests(false, flip_rounds);

            //test_MeshIO.test_simple_obj();

            System.Console.ReadLine();
        }
    }
}
