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

            //test_MeshIO.test_simple_obj();

            System.Console.ReadLine();
        }
    }
}
