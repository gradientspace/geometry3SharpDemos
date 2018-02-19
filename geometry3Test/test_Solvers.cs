using System;
using System.Collections.Generic;
using System.Diagnostics;
using g3;

namespace geometry3Test 
{
	public static class test_Solvers {


        public static void test_Matrices()
        {
            int N = 200;
            //int N = 2500;
            DenseMatrix M1 = new DenseMatrix(N, N);
            SymmetricSparseMatrix M2 = new SymmetricSparseMatrix();
            for (int i = 0; i < N; ++i) {
                for (int j = i; j < N; ++j) {
                    if (i == j) {
                        M1.Set(i, i, N);
                        M2.Set(i, i, N);
                    } else if ( j % 2 != 0) {
                        double d = 1.0 / Math.Sqrt(i + j);
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

            Debug.Assert(M1.IsSymmetric());
            Debug.Assert(M1.IsPositiveDefinite());

            // test parallel cholesky decomposition

            LocalProfiler p = new LocalProfiler();
			p.Start("chol");
			CholeskyDecomposition decompM = new CholeskyDecomposition(M1);
			decompM.ComputeParallel();
			p.Stop("chol");
			//System.Console.WriteLine(p.AllTimes());

            DenseMatrix LLT_M1 = decompM.L.Multiply(decompM.L.Transpose());
            if (LLT_M1.EpsilonEquals(M1) == false)
                System.Console.WriteLine("FAIL  choleskyM1 did not reproduce input");

            // test cholesky-decomp backsubstitution

            Random r = new Random(31337);
            double[] RealX = TestUtil.RandomScalars(N, r, new Interval1d(-10, 10));
            double[] B = new double[N], SolvedX = new double[N], TmpY = new double[N];
            M1.Multiply(RealX, B);
            decompM.Solve(B, SolvedX, TmpY);
            if (BufferUtil.DistanceSquared(RealX, SolvedX) > MathUtil.ZeroTolerance)
                System.Console.WriteLine("FAIL choleskyM1 backsubstution did not reproduce input vector");


            // test case from: https://people.cs.kuleuven.be/~karl.meerbergen/didactiek/h03g1a/ilu.pdf
            //DenseMatrix tmp = new DenseMatrix(6, 6);
            //tmp.Set(new double[] {
            //    3,0,-1,-1,0,-1,
            //    0,2,0,-1,0,0,
            //    -1,0,3,0,-1,0,
            //    -1,-1,0,2,0,-1,
            //    0,0,-1,0,3,-1,
            //    -1,0,0,-1,-1,4});
            //CholeskyDecomposition decompDense = new CholeskyDecomposition(tmp);
            //decompDense.Compute();
            //PackedSparseMatrix M1_sparse = PackedSparseMatrix.FromDense(tmp, true);
            //M1_sparse.Sort();
            //SparseCholeskyDecomposition decompM1_sparse = new SparseCholeskyDecomposition(M1_sparse);
            //decompM1_sparse.ComputeIncomplete();


            // cholesky decomposition known-result test
            DenseMatrix MSym3x3 = new DenseMatrix(3, 3);
			MSym3x3.Set(new double[]{25,15,-5,  15,18,0,  -5,0,11});
			DenseMatrix MSym3x3_Chol = new DenseMatrix(3, 3);
			MSym3x3_Chol.Set(new double[] { 5, 0, 0, 3, 3, 0, -1, 1, 3 });
			CholeskyDecomposition decomp3x3 = new CholeskyDecomposition(MSym3x3);
			decomp3x3.Compute();
			if (decomp3x3.L.EpsilonEquals(MSym3x3_Chol) == false)
				System.Console.WriteLine("FAIL  cholesky3x3 incorrect result");
			if ( decomp3x3.L.Multiply(decomp3x3.L.Transpose()).EpsilonEquals(MSym3x3) == false )
				System.Console.WriteLine("FAIL  cholesky3x3 did not reproduce input");

            // cholesky decomposition known-result test
            DenseMatrix MSym4x4 = new DenseMatrix(4, 4);
			MSym4x4.Set(new double[] {
				18, 22, 54, 42,  22, 70, 86, 62,
				54,  86,  174,  134, 42,  62,  134,  106 });
			DenseMatrix MSym4x4_Chol = new DenseMatrix(4, 4);
			MSym4x4_Chol.Set(new double[] {
				4.24264,0,0,0,  5.18545,6.56591, 0,0,
				12.72792, 3.04604, 1.64974, 0, 9.89949,1.62455,1.84971,1.39262 });
			CholeskyDecomposition decomp4x4 = new CholeskyDecomposition(MSym4x4);
			decomp4x4.Compute();
			if (decomp4x4.L.EpsilonEquals(MSym4x4_Chol, 0.0001) == false)
				System.Console.WriteLine("FAIL  cholesky4x4 incorrect result");
			if ( decomp4x4.L.Multiply(decomp4x4.L.Transpose()).EpsilonEquals(MSym4x4) == false )
				System.Console.WriteLine("FAIL  cholesky4x4 did not reproduce input");

        }



        public static void test_SparseCG()
        {
            Random r = new Random(31337);

            int N = 100;
            var pts = TestUtil.RandomScalars(N, r, new Interval1d(1, 10));
            SymmetricSparseMatrix M = new SymmetricSparseMatrix();
            double[] B = new double[N];
	        for (int i = 0; i < N; ++i) {
		        for (int j = i; j < N; ++j) {
                    if (i == j)
                        M.Set(i, j, pts[i]);
                    else
                        M.Set(i, j, (double)(i + j) / 10.0);
		        }
		        B[i] = i+1;
	        }

            SparseSymmetricCG Solver = new SparseSymmetricCG() { B = B, MultiplyF = M.Multiply };
            Solver.Solve();
            double[] BTest = new double[N];
            M.Multiply(Solver.X, BTest);
            double diff = BufferUtil.DistanceSquared(B, BTest);
            if (diff > MathUtil.ZeroTolerance)
                System.Console.WriteLine("test_SparseCG: initial solve failed!");

            PackedSparseMatrix PackedM = new PackedSparseMatrix(M);
            PackedM.Sort();
            SparseSymmetricCG Solver_PackedM = new SparseSymmetricCG() { B = B, MultiplyF = PackedM.Multiply };
            Solver_PackedM.Solve();
            PackedM.Multiply(Solver_PackedM.X, BTest);
            double diff_packed = BufferUtil.DistanceSquared(B, BTest);
            if (diff_packed > MathUtil.ZeroTolerance)
                System.Console.WriteLine("test_SparseCG: Packed solve failed!");

#if false
            SparseCholeskyDecomposition decomp = new SparseCholeskyDecomposition(PackedM);
            decomp.ComputeIncomplete();
            PackedSparseMatrix choleskyPrecond = decomp.L.Square();

            SymmetricSparseMatrix diagPrecond = new SymmetricSparseMatrix(N);
            for (int k = 0; k < N; ++k)
                diagPrecond[k, k] = 1.0 / M[k, k];

            SparseSymmetricCG Solver_Precond = new SparseSymmetricCG() { B = B, MultiplyF = PackedM.Multiply, PreconditionMultiplyF = diagPrecond.Multiply };
            Solver_Precond.SolvePreconditioned();
            PackedM.Multiply(Solver_Precond.X, BTest);
            double diff_precond = BufferUtil.DistanceSquared(B, BTest);
            if (diff_precond > MathUtil.ZeroTolerance)
                System.Console.WriteLine("test_SparseCG: cholesky-preconditioned solve failed!");

            System.Console.WriteLine("Iterations regular {0}  precond {1}", Solver_PackedM.Iterations, Solver_Precond.Iterations);
            System.Console.WriteLine("Tol regular {0}  precond {1}", diff_packed, diff_precond);
#endif

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
