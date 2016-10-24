using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using g3;

namespace frame3Test
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
			SimpleMeshBuilder builder = new SimpleMeshBuilder();
			var readResult = StandardMeshReader.ReadFile("c:\\scratch\\temp.obj", new ReadOptions(), builder);
			if (readResult.result != ReadResult.Ok)
				throw new Exception("fuck");

			var writeResult = StandardMeshWriter.WriteFile("c:\\scratch\\temp_new.obj",
				builder.Meshes.Cast<IMesh>().ToList(), new WriteOptions());
			if (writeResult.result != WriteResult.Ok)
				throw new Exception("fuck");
        }
    }
}
