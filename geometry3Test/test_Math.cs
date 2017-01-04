using System;
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


	}
}
