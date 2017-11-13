using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using g3;

namespace geometry3Test
{
	public static class test_MeshEdits
	{

		public static DMesh3 MakeEditTestMesh(int n, out string name)
		{
			name = "unknown";
			if (n == 0) {
				name = "bunny_open_base";
				return TestUtil.LoadTestMesh(Program.TEST_FILES_PATH + "bunny_open_base.obj");
			} else if (n == 1) {
				name = "n_holed_bunny";
				return TestUtil.LoadTestMesh(Program.TEST_FILES_PATH + "n_holed_bunny.obj");
			} else if (n == 2) {
				name = "bunny_bowties";
				return TestUtil.LoadTestMesh(Program.TEST_FILES_PATH + "bunny_bowties.obj");
            } else if (n == 3) {
                name = "open_sheets";
                return TestUtil.LoadTestMesh(Program.TEST_FILES_PATH + "open_sheets.obj");
            }
            throw new Exception("test_Spatial.MakeEditTestMesh: unknown mesh case");
		}
		public static int NumTestCases { get { return 1; } }






		public static void test_basic_fills()
		{
			// test trivial hole-fill

			//List<int> tests = new List<int>() { 0, 1 };
			List<int> tests = new List<int>() { 2 };

			foreach (int num_test in tests) {
				string name;
				DMesh3 mesh = MakeEditTestMesh(num_test, out name);
				mesh.EnableTriangleGroups();

				MeshBoundaryLoops loops;
				try {
					loops = new MeshBoundaryLoops(mesh);
					Debug.Assert(loops.Loops.Count > 0);
				} catch (Exception) {
					System.Console.WriteLine("failed to extract boundary loops for " + name);
					continue;
				}

				System.Console.WriteLine("Closing " + name + " - {0} holes", loops.Loops.Count);

				bool bOK = true;
				foreach (EdgeLoop loop in loops.Loops) {
					SimpleHoleFiller filler = new SimpleHoleFiller(mesh, loop);
					Debug.Assert(filler.Validate() == ValidationStatus.Ok);
					bOK = bOK && filler.Fill(1);
					Debug.Assert(bOK);
				}
				System.Console.WriteLine("{0}", (bOK) ? "Ok" : "Failed");
				if (bOK)
					Debug.Assert(mesh.CachedIsClosed);
				TestUtil.WriteTestOutputMesh(mesh, name + "_filled" + ".obj");
			}

		} // test_basic_fills




		public static void test_plane_cut()
		{
			List<int> tests = new List<int>() { 1 };

			bool DO_EXHAUSTIVE_TESTS = false;

			foreach (int num_test in tests) {
				string name;
				DMesh3 orig_mesh = MakeEditTestMesh(num_test, out name);

				DMesh3 mesh = new DMesh3(orig_mesh);

				AxisAlignedBox3d bounds = mesh.CachedBounds;

				MeshPlaneCut cut = new MeshPlaneCut(mesh, bounds.Center, Vector3d.AxisY);
				Debug.Assert(cut.Validate() == ValidationStatus.Ok);
				bool bCutOK = cut.Cut();

                System.Console.Write("cut: {0}   loops: {1}  spans: {2}", ((bCutOK) ? "Ok" : "Failed"),
                    cut.CutLoops.Count, cut.CutSpans.Count);

                foreach (var loop in cut.CutLoops)
                    loop.CheckValidity(FailMode.gDevAssert);
                foreach (var span in cut.CutSpans)
                    span.CheckValidity(FailMode.gDevAssert);

                bool bFillOK = cut.FillHoles(-1);
                System.Console.WriteLine("   fill: {0}", ((bFillOK) ? "Ok" : "Failed"));

				TestUtil.WriteTestOutputMesh(mesh, name + "_cut" + ".obj");


				if (DO_EXHAUSTIVE_TESTS == false)
					continue;


				// grinder: cut through each vtx
				int VertexIncrement = 1;
				for (int vid = 0; vid < orig_mesh.MaxVertexID; vid += VertexIncrement) {
					if (orig_mesh.IsVertex(vid) == false)
						continue;
					if (vid % 100 == 0)
						System.Console.WriteLine("{0} / {1}", vid, orig_mesh.MaxVertexID);
					Vector3d v = orig_mesh.GetVertex(vid);
					mesh = new DMesh3(orig_mesh);
					cut = new MeshPlaneCut(mesh, v, Vector3d.AxisY);
					bCutOK = cut.Cut();
					Debug.Assert(bCutOK);
				}



			} // test_plane_cut


		}
	}
}
