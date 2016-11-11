using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using g3;

namespace geometry3Test
{
    public class test_MeshIO
    {
        public static void test_simple_obj()
        {
            SimpleMeshBuilder builder = new SimpleMeshBuilder();
            StandardMeshReader reader = new StandardMeshReader();
            reader.MeshBuilder = builder;
            var readResult = StandardMeshReader.ReadFile("c:\\scratch\\temp.obj", new ReadOptions());

            if (readResult.result != ReadResult.Ok)
                throw new Exception("fuck");

            var writeResult = StandardMeshWriter.WriteFile("c:\\scratch\\temp_new.obj",
                builder.Meshes.Cast<IMesh>().ToList(), new WriteOptions());
            if (writeResult.result != WriteResult.Ok)
                throw new Exception("fuck");
        }


    }
}
