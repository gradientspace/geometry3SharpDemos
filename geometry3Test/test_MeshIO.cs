using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using g3;

namespace geometry3Test
{
    public class test_MeshIO
    {
        public static void test_simple_obj()
        {
            string cwd = System.IO.Directory.GetCurrentDirectory();

            System.Console.WriteLine("MeshIOTests : test_simple_obj() starting");

            //SimpleMeshBuilder builder = new SimpleMeshBuilder();
            DMesh3Builder builder = new DMesh3Builder();
            StandardMeshReader reader = new StandardMeshReader();
            reader.MeshBuilder = builder;
            var readResult = reader.Read(Program.TEST_FILES_PATH + "socket_with_groups.obj", new ReadOptions());

            System.Console.WriteLine("read complete");

			if (readResult.code != IOCode.Ok) {
				System.Console.WriteLine("read failed : " + readResult.message);
                throw new Exception("failed");
            }

			List<WriteMesh> meshes = new List<WriteMesh>();
			foreach ( IMesh m in builder.Meshes )
				meshes.Add( new WriteMesh(m) );
            var writeResult = StandardMeshWriter.WriteFile(Program.TEST_OUTPUT_PATH + "temp_write.obj",
                meshes, new WriteOptions());

            System.Console.WriteLine("write complete");

            if (writeResult.code != IOCode.Ok) {
                System.Console.WriteLine("write failed : " + writeResult.message);
                throw new Exception("fuck");
            }
        }




        public static void test_read_thingi10k()
        {
            //const string THINGIROOT = "D:\\meshes\\Thingi10K\\raw_meshes\\";
            const string THINGIROOT = "F:\\Thingi10K\\raw_meshes\\";
            string[] files = Directory.GetFiles(THINGIROOT);

            SafeListBuilder<string> failures = new SafeListBuilder<string>();

            SafeListBuilder<string> empty = new SafeListBuilder<string>();
            SafeListBuilder<string> closed = new SafeListBuilder<string>();
            SafeListBuilder<string> open = new SafeListBuilder<string>();
            SafeListBuilder<string> boundaries_failed = new SafeListBuilder<string>();

            int k = 0;

            gParallel.ForEach(files, (filename) => {
                int i = k;
                Interlocked.Increment(ref k);
                System.Console.WriteLine("{0} : {1}", i, filename);

                DMesh3Builder builder = new DMesh3Builder();
                StandardMeshReader reader = new StandardMeshReader() { MeshBuilder = builder };
                IOReadResult result = reader.Read(filename, ReadOptions.Defaults);
                if (result.code != IOCode.Ok) {
                    System.Console.WriteLine("{0} FAILED!", filename);
                    failures.SafeAdd(filename);
                    return;
                }

                bool is_open = false;
                bool loops_failed = false;
                bool is_empty = true;
                foreach ( DMesh3 mesh in builder.Meshes ) {
                    if (mesh.TriangleCount > 0)
                        is_empty = false;

                    if ( mesh.IsClosed() == false ) {
                        is_open = true;
                        try {
                            MeshBoundaryLoops loops = new MeshBoundaryLoops(mesh, false) {
                                SpanBehavior = MeshBoundaryLoops.SpanBehaviors.ThrowException,
                                FailureBehavior = MeshBoundaryLoops.FailureBehaviors.ThrowException
                            };
                            loops.Compute();
                        } catch {
                            loops_failed = true;
                        }
                    }
                }

                if (is_empty) {
                    empty.SafeAdd(filename);
                } else if (is_open) {
                    open.SafeAdd(filename);
                    if (loops_failed)
                        boundaries_failed.SafeAdd(filename);

                } else {
                    closed.SafeAdd(filename);
                }


            });


            foreach ( string failure in failures.Result ) {
                System.Console.WriteLine("FAIL: {0}", failure);
            }

            TestUtil.WriteTestOutputStrings(empty.List.ToArray(), "thingi10k_empty.txt");
            TestUtil.WriteTestOutputStrings(closed.List.ToArray(), "thingi10k_closed.txt");
            TestUtil.WriteTestOutputStrings(open.List.ToArray(), "thingi10k_open.txt");
            TestUtil.WriteTestOutputStrings(boundaries_failed.List.ToArray(), "thingi10k_boundaries_failed.txt");
        }





        // make sure format writers all minimally function, and properly close file when completed
        public static void test_write_formats()
        {
            string out_path = Program.TEST_OUTPUT_PATH + "format_test";

            DMesh3 mesh = StandardMeshReader.ReadMesh(Program.TEST_FILES_PATH + "bunny_solid.obj");
            StandardMeshWriter writer = new StandardMeshWriter();
            var list = new List<WriteMesh>() { new WriteMesh(mesh) };

            if (writer.Write(out_path + ".obj", list, WriteOptions.Defaults).code != IOCode.Ok)
                System.Console.WriteLine("test_write_formats: obj failed");
            if (writer.Write(out_path + ".stl", list, WriteOptions.Defaults).code != IOCode.Ok)
                System.Console.WriteLine("test_write_formats: stl failed");
            if (writer.Write(out_path + ".off", list, WriteOptions.Defaults).code != IOCode.Ok)
                System.Console.WriteLine("test_write_formats: off failed");
            if (writer.Write(out_path + ".g3mesh", list, WriteOptions.Defaults).code != IOCode.Ok)
                System.Console.WriteLine("test_write_formats: g3mesh failed");

            if (writer.Write(out_path + ".obj", list, WriteOptions.Defaults).code != IOCode.Ok)
                System.Console.WriteLine("test_write_formats: obj failed on second pass");
            if (writer.Write(out_path + ".stl", list, WriteOptions.Defaults).code != IOCode.Ok)
                System.Console.WriteLine("test_write_formats: stl failed on second pass");
            if (writer.Write(out_path + ".off", list, WriteOptions.Defaults).code != IOCode.Ok)
                System.Console.WriteLine("test_write_formats: off failed on second pass");
            if (writer.Write(out_path + ".g3mesh", list, WriteOptions.Defaults).code != IOCode.Ok)
                System.Console.WriteLine("test_write_formats: g3mesh failed on second pass");

            MemoryStream fileStream = new MemoryStream();
            MemoryStream mtlStream = new MemoryStream();
            writer.OpenStreamF = (filename) => {
                return filename.EndsWith(".mtl") ? mtlStream : fileStream;
            };
            writer.CloseStreamF = (stream) => { };

            WriteOptions opt = WriteOptions.Defaults; opt.bWriteMaterials = true; opt.MaterialFilePath = out_path + ".mtl";
            if (writer.Write(out_path + ".obj", list, opt).code != IOCode.Ok)
                System.Console.WriteLine("test_write_formats: write to memory stream failed");

            //string s = Encoding.ASCII.GetString(fileStream.ToArray());
            if (fileStream.Length == 0 )
                System.Console.WriteLine("test_write_formats: write to memory stream produced zero-length stream");
        }


        public static void test_points()
        {
            string filename = "c:\\scratch\\bunny_solid.obj";
            DMesh3 mesh = StandardMeshReader.ReadMesh(filename);

            PointSplatsGenerator pointgen = new PointSplatsGenerator() {
                PointIndices = IntSequence.Range(mesh.VertexCount),
                PointF = mesh.GetVertex,
                NormalF = (vid) => { return (Vector3d)mesh.GetVertexNormal(vid); },
                Radius = mesh.CachedBounds.DiagonalLength * 0.01
            };
            DMesh3 pointMesh = pointgen.Generate().MakeDMesh();
            StandardMeshWriter.WriteMesh("c:\\scratch\\POINTS.obj", pointMesh, WriteOptions.Defaults);
        }

    }



}
