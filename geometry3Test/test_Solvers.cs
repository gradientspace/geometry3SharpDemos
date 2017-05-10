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
        public static void test_Laplacian()
        {
            // compact version
            DMesh3 mesh = new DMesh3(TestUtil.MakeRemeshedCappedCylinder(1.0), true);
            Debug.Assert(mesh.IsCompact);

            AxisAlignedBox3d bounds = mesh.GetBounds();

            TestUtil.WriteDebugMesh(mesh, "___CG_before.obj");

            List<IMesh> result_meshes = new List<IMesh>();

            // make uniform laplacian matrix
            int N = mesh.VertexCount;
            SymmetricSparseMatrix M = new SymmetricSparseMatrix();
            //DenseMatrix M = new DenseMatrix(N, N);
            double[] Px = new double[N], Py = new double[N], Pz = new double[N];

            int[] nbr_counts = new int[N];
            for (int vid = 0; vid < N; ++vid)
                nbr_counts[vid] = mesh.GetVtxEdgeCount(vid);

            int ti = MeshQueries.FindNearestTriangle_LinearSearch(mesh, new Vector3d(2, 5, 2));
            int v_pin = mesh.GetTriangle(ti).a;
            List<int> constraints = new List<int>() { v_pin };
            double consW = 10;
            double consBottom = 10;

            foreach (int vid in constraints)
                result_meshes.Add(TestUtil.MakeMarker(mesh.GetVertex(vid), (vid == 0) ? 0.2f : 0.1f, Colorf.Red));

            for (int vid = 0; vid < N; ++vid) {
                int n = nbr_counts[vid];
                Vector3d v = mesh.GetVertex(vid), c = Vector3d.Zero;

                Px[vid] = v.x; Py[vid] = v.y; Pz[vid] = v.z;

                bool bottom = (v.y - bounds.Min.y) < 0.01f;

                double sum_w = 0;
                foreach ( int nbrvid in mesh.VtxVerticesItr(vid) ) {
                    int n2 = nbr_counts[nbrvid];

                    // weight options
                    //double w = -1;
                    double w = -1.0 / Math.Sqrt(n + n2);
                    //double w = -1.0 / n;

                    M.Set(vid, nbrvid, w);

                    c += w*mesh.GetVertex(nbrvid);
                    sum_w += w;
                }
                sum_w = -sum_w;

                M.Set(vid, vid, sum_w);

                // add soft constraints
                if ( constraints.Contains(vid) ) {
                    M.Set(vid, vid, sum_w + consW);
                } else if ( bottom ) {
                    M.Set(vid, vid, sum_w + consBottom);
                }
            }

            // compute laplacians
            double[] MLx = new double[N], MLy = new double[N], MLz = new double[N];
            M.Multiply(Px, MLx);
            M.Multiply(Py, MLy);
            M.Multiply(Pz, MLz);


            DiagonalMatrix Preconditioner = new DiagonalMatrix(N);
            for ( int i = 0; i < N; i++ ) {
                Preconditioner.Set(i, i, 1.0 / M[i, i]);
            }


            MLy[v_pin] += consW*0.5f;
            MLx[v_pin] += consW*0.5f;
            MLz[v_pin] += consW*0.5f;

            bool useXAsGuess = true;
            // preconditioned
            SparseSymmetricCG SolverX = new SparseSymmetricCG() { B = MLx, X = Px, MultiplyF = M.Multiply, PreconditionMultiplyF = Preconditioner.Multiply, UseXAsInitialGuess = useXAsGuess };
            // initial solution
            SparseSymmetricCG SolverY = new SparseSymmetricCG() { B = MLy, X = Py, MultiplyF = M.Multiply, UseXAsInitialGuess = useXAsGuess };
            // neither of those
            SparseSymmetricCG SolverZ = new SparseSymmetricCG() { B = MLz, MultiplyF = M.Multiply };

            bool bx = SolverX.Solve();
            bool by = SolverY.Solve();
            bool bz = SolverZ.Solve();

            for ( int vid = 0; vid < mesh.VertexCount; ++vid ) {
                Vector3d newV = new Vector3d(SolverX.X[vid], SolverY.X[vid], SolverZ.X[vid]);
                mesh.SetVertex(vid, newV);
            }

            result_meshes.Add(mesh);
            TestUtil.WriteDebugMeshes(result_meshes, "___CG_result.obj");
        }


	}
}
