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
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            test_VectorTypes.test_rcvector();

            //test_MeshIO.test_simple_obj();

            System.Console.ReadLine();
        }
    }
}
