using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using g3;

namespace geometry3Test
{
    public static class test_MathTypes
    {
        public static void all_tests()
        {
            test_AxisAlignedBox3();
            test_Matrix3d();
        }



        public static void test_AxisAlignedBox3()
        {
            Random r = new Random(31337);
            for ( int iter = 0; iter < 10000; ++iter) {
                Vector3d[] pts = TestUtil.RandomPoints3(100, r, Vector3d.Zero, 100);
                for ( int j = 0; j < pts.Length; j += 2) {
                    AxisAlignedBox3d box1 = new AxisAlignedBox3d(pts[j], 10.0);
                    AxisAlignedBox3d box2 = new AxisAlignedBox3d(pts[j + 1], 20);

                    double dist_sqr = box1.DistanceSquared(ref box2);
                    if (box1.Intersects(box2))
                        Util.gDevAssert(dist_sqr == 0);
                    else
                        Util.gDevAssert(dist_sqr > 0);

                    dist_sqr -= MathUtil.ZeroTolerance;  // numericals
                    for (int k = 0; k < 8; ++k) {
                        Vector3d p0 = box1.Corner(k);
                        Util.gDevAssert(dist_sqr < p0.DistanceSquared(box2.Center));
                        for (int i = 0; i < 8; ++i)
                            Util.gDevAssert(dist_sqr <= p0.DistanceSquared(box2.Corner(i)));
                    }

                }
            }

        }





        public static void test_Matrix3d()
        {
            {
                Matrix3d Identity = new Matrix3d(true);
                for (int r = 0; r < 3; ++r) {
                    for (int c = 0; c < 3; ++c)
                        Util.gDevAssert((r == c) ? Identity[r, c] == 1 : Identity[r, c] == 0);
                }
            }

            {
                double[] buffer = new double[9] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                Matrix3d M = new Matrix3d(buffer);
                for (int r = 0; r < 3; ++r) {
                    for (int c = 0; c < 3; ++c)
                        Util.gDevAssert(M[r, c] == buffer[r * 3 + c]);
                }
                for (int k = 0; k < 9; ++k)
                    Util.gDevAssert(M[k] == buffer[k]);

                Matrix3d M2 = new Matrix3d(new Vector3d(1, 2, 3), new Vector3d(4, 5, 6), new Vector3d(7, 8, 9), true);
                Util.gDevAssert(M.EpsilonEqual(M2, 0));
                Matrix3d M3 = new Matrix3d(new Vector3d(1, 4, 7), new Vector3d(2, 5, 8), new Vector3d(3, 6, 9), false);
                Util.gDevAssert(M.EpsilonEqual(M3, 0));

                Util.gDevAssert(M.Transpose().Transpose().EpsilonEqual(M, 0));
                Util.gDevAssert(M.EpsilonEqual(new Matrix3d(M.ToBuffer()), 0));

                Matrix3d Det = new Matrix3d(6, 1, 1, 4, -2, 5, 2, 8, 7);
                Util.gDevAssert(Det.Determinant == -306);

                Matrix3d Mult = M * M;
                Matrix3d MultResult = new Matrix3d(30, 36, 42, 66, 81, 96, 102, 126, 150);
                Util.gDevAssert(Mult.EpsilonEqual(MultResult, 0));

                Matrix3d Mult2 = new Matrix3d(M * M.Column(0), M * M.Column(1), M * M.Column(2), false);
                Util.gDevAssert(Mult2.EpsilonEqual(MultResult, 0));
            }


            {
                Random r = new Random(31337);
                Vector3d[] axes = TestUtil.RandomPoints3(100, r, Vector3d.Zero);
                double[] angles = TestUtil.RandomScalars(100, r, new Interval1d(-180, 180));
                Vector3d[] testPts = TestUtil.RandomPoints3(100, r, Vector3d.Zero, 10);

                for (int k = 0; k < angles.Length; ++k) {
                    Vector3d axis = axes[k].Normalized;
                    double angle = angles[k];
                    Vector3d pt = testPts[k];
                    Matrix3d MRotAxis = Matrix3d.AxisAngleD(axis, angle);
                    Quaterniond QRotAxis = Quaterniond.AxisAngleD(axis, angle);
                    Util.gDevAssert(
                        (MRotAxis * pt).EpsilonEqual(QRotAxis * pt, 100 * MathUtil.Epsilon));

                    Matrix3d QRotAxisM = QRotAxis.ToRotationMatrix();
                    Util.gDevAssert(MRotAxis.EpsilonEqual(QRotAxisM, 10*MathUtil.Epsilon));

                    Quaterniond QuatCons = new Quaterniond(MRotAxis);
                    Util.gDevAssert(
                        (QuatCons * pt).EpsilonEqual(QRotAxis * pt, 100*MathUtil.Epsilon));


                    // test 3x3 SVD

                    Matrix3d tmpM = MRotAxis;
                    tmpM[0, 0] += 0.0001; tmpM[1, 1] -= 0.0001; tmpM[0, 2] += 0.1;

                    SingularValueDecomposition fullsvd = new SingularValueDecomposition(3, 3, 999);
                    uint result = fullsvd.Solve(tmpM.ToBuffer(), -1);
                    double[] U = new double[9], V = new double[9], S = new double[3];
                    fullsvd.GetU(U); fullsvd.GetV(V);
                    fullsvd.GetSingularValues(S);
                    Matrix3d MU = new Matrix3d(U), MV = new Matrix3d(V);
                    Matrix3d Sdiag = new Matrix3d(S[0], S[1], S[2]);

                    // U is eigenvectors of ATA, V is eigenvectors of AAT
                    Vector3d rRight = Vector3d.Zero, rLeft = Vector3d.Zero;
                    Matrix3d ATA = tmpM.Transpose() * tmpM, AAT = tmpM * tmpM.Transpose();
                    for (int j = 0; j < 3; ++j) {
                        double eval = Sdiag[j, j] * Sdiag[j, j];
                        Vector3d right_evec = MV.Column(j);
                        rRight[j] = (ATA * right_evec - eval * right_evec).Length;
                        Vector3d left_evec = MU.Column(j);
                        rLeft[j] = (AAT * left_evec - eval * left_evec).Length;
                    }
                    Util.gDevAssert(rRight.Length < MathUtil.ZeroTolerancef && rLeft.Length < MathUtil.ZeroTolerancef);

                    // [RMS] if U or V contains a reflection, we need to get rid of it
                    if ( MU.Determinant < 0 ) {
                        MU *= -1;
                        Sdiag *= -1;
                    }
                    if ( MV.Determinant < 0 ) {
                        MV *= -1;
                        Sdiag *= -1;
                    }

                    Matrix3d MRecons = MU * Sdiag * MV.Transpose();
                    Util.gDevAssert((tmpM * pt).EpsilonEqual(MRecons * pt, 1000 * MathUtil.Epsilon));

                    Quaterniond qU = new Quaterniond(MU);
                    Util.gDevAssert((MU * pt).EpsilonEqual(qU * pt, 100 * MathUtil.Epsilon));
                    Quaterniond qV = new Quaterniond(MV.Transpose());
                    Util.gDevAssert((MV.Transpose() * pt).EpsilonEqual(qV * pt, 100 * MathUtil.Epsilon));
                    Vector3d ptQ =  qU * (Sdiag * (qV * pt));
                    Util.gDevAssert((tmpM * pt).EpsilonEqual(ptQ, 1000 * MathUtil.Epsilon));

                    Matrix3d MQRecons = qU.ToRotationMatrix() * Sdiag * qV.ToRotationMatrix();
                    Util.gDevAssert((tmpM * pt).EpsilonEqual(MQRecons * pt, 1000 * MathUtil.Epsilon));


                    // U is eigenvectors of ATA, V is eigenvectors of AAT
                    Vector3d qRight = Vector3d.Zero, qLeft = Vector3d.Zero;
                    for (int j = 0; j < 3; ++j) {
                        double eval = Sdiag[j, j] * Sdiag[j, j];
                        Vector3d right_evec = qV.Conjugate().ToRotationMatrix().Column(j);
                        qRight[j] = (ATA * right_evec - eval * right_evec).Length;
                        Vector3d left_evec = qU.ToRotationMatrix().Column(j);
                        qLeft[j] = (AAT * left_evec - eval * left_evec).Length;
                    }
                    Util.gDevAssert(qRight.Length < MathUtil.ZeroTolerancef && qLeft.Length < MathUtil.ZeroTolerancef);


                    double fast_eps = 0.001;

                    FastQuaternionSVD svd = new FastQuaternionSVD(tmpM, MathUtil.Epsilon, 4);
                    Matrix3d QQRecons = svd.ReconstructMatrix();
                    Util.gDevAssert((tmpM * pt).EpsilonEqual(QQRecons * pt, fast_eps));

                    Matrix3d svdS = new Matrix3d(svd.S[0], svd.S[1], svd.S[2]);

                    Matrix3d QQRecons2 = 
                        svd.U.ToRotationMatrix() * svdS * svd.V.Conjugate().ToRotationMatrix();
                    Util.gDevAssert((tmpM * pt).EpsilonEqual(QQRecons2 * pt, fast_eps));

                    Vector3d ptQSVD = svd.U * (svdS * (svd.V.Conjugate() * pt));
                    Util.gDevAssert((tmpM * pt).EpsilonEqual(ptQSVD, fast_eps));

                    // U is eigenvectors of ATA, V is eigenvectors of AAT
                    Vector3d qqRight = Vector3d.Zero, qqLeft = Vector3d.Zero;
                    for (int j = 0; j < 3; ++j) {
                        double eval = svdS[j, j] * svdS[j, j];
                        Vector3d right_evec = svd.V.ToRotationMatrix().Column(j);
                        qqRight[j] = (ATA * right_evec - eval * right_evec).Length;
                        Vector3d left_evec = svd.U.ToRotationMatrix().Column(j);
                        qqLeft[j] = (AAT * left_evec - eval * left_evec).Length;
                    }
                    Util.gDevAssert(qqRight.Length < fast_eps && qqLeft.Length < fast_eps);
                }
            }



        }


    }
}
