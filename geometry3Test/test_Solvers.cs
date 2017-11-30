using System;
using System.Collections.Generic;
using System.Diagnostics;
using g3;

namespace geometry3Test 
{
	public static class test_Solvers {


        public static void test_Matrices()
        {
            int N = 10;
            DenseMatrix M1 = new DenseMatrix(N, N);
            SymmetricSparseMatrix M2 = new SymmetricSparseMatrix();
            for (int i = 0; i < N; ++i) {
                for (int j = i; j < N; ++j) {
                    if (i == j) {
                        M1.Set(i, i, 1);
                        M2.Set(i, i, 1);
                    } else if ( j % 2 != 0) {
                        double d = Math.Sqrt(i + j);
                        M1.Set(i, j, d);
                        M1.Set(j, i, d);
                        M2.Set(i, j, d);
                    }
                }
            }

            double[] X = new double[N], b1 = new double[N], b2 = new double[N];
            for (int i = 0; i < N; ++i)
                X[i] = (double)i / (double)N;

            M1.Multiply(X, b1);
            M2.Multiply(X, b2);

            for (int i = 0; i < N; ++i)
                Debug.Assert(MathUtil.EpsilonEqual(b1[i], b2[i]));
        }



        public static void test_SparseCG()
        {
            int N = 10;
            SymmetricSparseMatrix M = new SymmetricSparseMatrix();
            double[] B = new double[N];
	        for (int i = 0; i < N; ++i) {
		        for (int j = i; j < N; ++j) {
                    if (i == j)
                        M.Set(i, j, 1);
                    else
                        M.Set(i, j, (double)(i + j) / 100.0);
		        }
		        B[i] = i+1;
	        }

	        double[] X = new double[N];

            SparseSymmetricCG Solver = new SparseSymmetricCG() { B = B, MultiplyF = M.Multiply };
            Solver.Solve();
            string s = "";
            for (int i = 0; i < N; ++i)
                s += " " + Solver.X[i];
            System.Console.WriteLine(s);
        }





        // [RMS] this only tests some basic cases...
        public static void test_Laplacian_deformer()
        {
            // compact version
            DMesh3 mesh = new DMesh3(TestUtil.MakeRemeshedCappedCylinder(1.0), true);
            Debug.Assert(mesh.IsCompact);

            AxisAlignedBox3d bounds = mesh.GetBounds();

            TestUtil.WriteTestOutputMesh(mesh, "laplacian_deformer_before.obj");

            List<IMesh> result_meshes = new List<IMesh>();

            LaplacianMeshDeformer deformer = new LaplacianMeshDeformer(mesh);


            int ti = MeshQueries.FindNearestTriangle_LinearSearch(mesh, new Vector3d(2, 5, 2));
            int v_pin = mesh.GetTriangle(ti).a;
            List<int> constraints = new List<int>() { v_pin };
            double consPin = 10;
            double consBottom = 10;

            foreach (int vid in constraints)
                result_meshes.Add(TestUtil.MakeMarker(mesh.GetVertex(vid), (vid == 0) ? 0.2f : 0.1f, Colorf.Red));

            foreach ( int vid in mesh.VertexIndices() ) {
                Vector3d v = mesh.GetVertex(vid);
                bool bottom = (v.y - bounds.Min.y) < 0.01f;
                if ( constraints.Contains(vid) )
                    deformer.SetConstraint(vid, v + Vector3f.AxisY, consPin, false);
                if (bottom)
                    deformer.SetConstraint(vid, v, consBottom, false);

            }

            deformer.SolveAndUpdateMesh();

            result_meshes.Add(mesh);
            TestUtil.WriteTestOutputMeshes( result_meshes, "laplacian_deformer_after.obj");
        }


	}
}
