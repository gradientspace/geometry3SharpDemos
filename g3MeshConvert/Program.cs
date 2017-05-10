using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using g3;

namespace g3MeshConvert
{
    class Program
    {
        static void Main(string[] args)
        {
            bool INTERACTIVE = true;


            //string inFile = "c:\\scratch\\test_bunny.obj";
            //string inFile = "c:\\scratch\\bunny_100k.obj";
            //string inFile = "c:\\scratch\\test_bunny_ascii.stl";
            string inFile = "c:\\scratch\\test_bunny_binary.stl";
            string outFile = "c:\\scratch\\test_bunny_out.obj";

            ReadOptions read_options = new ReadOptions();
            read_options.ReadMaterials = false;
            StandardMeshReader reader = new StandardMeshReader();
            DMesh3Builder builder = new DMesh3Builder();
            IOReadResult read_result = StandardMeshReader.ReadFile(inFile, read_options, builder);

            if (read_result.code != IOCode.Ok) {
                System.Console.WriteLine("Error reading " + inFile + " : " + read_result.message);
                if (INTERACTIVE) {
                    System.Console.WriteLine("press enter key to exit");
                    System.Console.ReadLine();
                }
                return;
            }


            WriteOptions write_options = new WriteOptions();
            write_options.bCombineMeshes = true;
            write_options.bWriteBinary = true;

            List<WriteMesh> outMeshes = new List<WriteMesh>();
            foreach (DMesh3 m in builder.Meshes)
                outMeshes.Add(new WriteMesh(m));

            IOWriteResult write_result = StandardMeshWriter.WriteFile(outFile, outMeshes, write_options);
            
            if ( write_result.code != IOCode.Ok ) {
                System.Console.WriteLine("Error writing " + outFile + " : " + write_result.message);
                if (INTERACTIVE) {
                    System.Console.WriteLine("press enter key to exit");
                    System.Console.ReadLine();
                }
                return;
            }


            if (INTERACTIVE) {
                System.Console.WriteLine("Done conversion, press enter key to exit");
                System.Console.ReadLine();
            }

        }
    }
}
