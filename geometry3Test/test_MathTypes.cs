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

    }
}
