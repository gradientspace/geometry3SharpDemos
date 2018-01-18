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



    }



}
