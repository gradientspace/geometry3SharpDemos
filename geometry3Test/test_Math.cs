using System;
using System.Diagnostics;
using g3;

namespace geometry3Test 
{
	public static class test_Math {

		// compare vector-cot and vector-tan to regular cot/tan
		public static void test_VectorTanCot() {

			Vector3d a = Vector3d.AxisX;

			double angleStep = 0.001;
			int nSteps = (int)(Math.PI/angleStep);
			for ( int k = 0; k <= nSteps; ++k ) {
				double angle = angleStep * k;

				Vector3d b = new Vector3d(Math.Cos(angle), Math.Sin(angle), 0);
				b.Normalize();

				double vtanAB = MathUtil.VectorTan(a,b);
				double tanAngle = Math.Tan(angle);
				double vcotAB = MathUtil.VectorCot(a,b);

				double tanErr = Math.Abs(vtanAB-tanAngle);
				double cotErr = Math.Abs(vcotAB-(1/tanAngle));

				bool bErr = tanErr > 0.000001 || cotErr > 0.00001;

				if (bErr) 
					System.Console.WriteLine(" angle: {6}  tan {0}  vtan {1} err {2}     cot {3} vcot {4} coterr {5}",
					                         tanAngle, vtanAB, tanErr, 1/tanAngle, vcotAB, cotErr, angle);
			}
		}





        static void assert_same_hit(AxisAlignedBox3d aabox, Box3d obox, Ray3d ray, bool bIsAlwaysHit)
        {
            IntrRay3Box3 ohit = new IntrRay3Box3(ray, obox);
            ohit.Find();
            if ( bIsAlwaysHit )
                Debug.Assert(ohit.Find() == true);
            IntrRay3AxisAlignedBox3 aabbhit = new IntrRay3AxisAlignedBox3(ray, aabox);
            aabbhit.Find();
            if ( bIsAlwaysHit )
                Debug.Assert(aabbhit.Find() == true);

            Debug.Assert(ohit.Find() == aabbhit.Find());

            Debug.Assert(ohit.Test() == ohit.Find());
            Debug.Assert(aabbhit.Test() == aabbhit.Find());

            if (ohit.Find()) {
                Debug.Assert(MathUtil.EpsilonEqual(ohit.RayParam0, aabbhit.RayParam0, MathUtil.ZeroTolerance));
            }
        }


        // [RMS] this only tests some basic cases...
        public static void test_RayBoxIntersect()
        {
            Random rand = new Random(316136327);

            // check that box hit works 
            for ( int ii = 0; ii < 1000; ++ii ) {
                // generate random triangle
                Triangle3d t = new Triangle3d(rand.PointInRange(10), rand.PointInRange(10), rand.PointInRange(10));
                AxisAlignedBox3d bounds = new AxisAlignedBox3d(t.V0);
                bounds.Contain(t.V1);
                bounds.Contain(t.V2);
                Vector3d c = (t.V0 + t.V1 + t.V2) / 3.0;
                for ( int jj = 0; jj < 1000; ++jj) {
                    Vector3d d = rand.Direction();
                    Ray3d ray = new Ray3d(c - 100 * d, d);
                    IntrRay3AxisAlignedBox3 bhit = new IntrRay3AxisAlignedBox3(ray, bounds);
                    Debug.Assert(bhit.Find());
                    IntrRay3Triangle3 thit = new IntrRay3Triangle3(ray, t);
                    Debug.Assert(thit.Find());
                    Debug.Assert(bhit.RayParam0 < thit.RayParameter);
                }
            }

            int N = 100;
            for (int ii = 0; ii < N; ++ii) {

                // generate random boxes
                Vector3d c = rand.PointInRange(10);
                Vector3d e = rand.PositivePoint();
                AxisAlignedBox3d aabox = new AxisAlignedBox3d(c - e, c + e);
                Box3d obox = new Box3d(c, Vector3d.AxisX, Vector3d.AxisY, Vector3d.AxisZ, e);
                double r = aabox.DiagonalLength;

                // center-out tests
                for (int jj = 0; jj < N; ++jj) {
                    Ray3d ray = new Ray3d(c, rand.Direction());
                    assert_same_hit(aabox, obox, ray, true);
                }

                // outside-in tests
                for (int jj = 0; jj < N; ++jj) {
                    Vector3d p = c + 2 * r * rand.Direction();
                    Ray3d ray = new Ray3d(p, (c - p).Normalized);
                    assert_same_hit(aabox, obox, ray, true);
                }
            }



            // random rays
            int hits = 0;
            int InnerN = 1000;
            for (int ii = 0; ii < N; ++ii) {

                // generate random boxe
                Vector3d c = rand.PointInRange(10);
                Vector3d e = rand.PositivePoint();

                // every tenth box, set an axis to degenerate
                if (ii % 10 == 0)
                    e[rand.Next() % 3] = 0;


                AxisAlignedBox3d aabox = new AxisAlignedBox3d(c - e, c + e);
                Box3d obox = new Box3d(c, Vector3d.AxisX, Vector3d.AxisY, Vector3d.AxisZ, e);
                double r = aabox.DiagonalLength;


                TrivialBox3Generator boxgen = new TrivialBox3Generator() { Box = obox };
                boxgen.Generate();
                DMesh3 mesh = new DMesh3();
                boxgen.MakeMesh(mesh);

                for (int i = 0; i < InnerN; ++i) {
                    Vector3d target = c + rand.PointInRange(r);
                    Vector3d o = c + rand.PointInRange(10 * r);
                    Ray3d ray = new Ray3d(o, (target - o).Normalized);
                    assert_same_hit(aabox, obox, ray, false);

                    int hitT = MeshQueries.FindHitTriangle_LinearSearch(mesh, ray);
                    bool bMeshHit = (hitT != DMesh3.InvalidID);
                    if (bMeshHit)
                        ++hits;
                    IntrRay3AxisAlignedBox3 aabbhit = new IntrRay3AxisAlignedBox3(ray, aabox);
                    Debug.Assert(aabbhit.Find() == bMeshHit);
                    Debug.Assert(aabbhit.Test() == bMeshHit);
                }
            }

            System.Console.WriteLine("hit {0} of {1} rays", hits, N * InnerN);
        }


	}
}
