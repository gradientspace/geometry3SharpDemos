using System;
using System.Collections.Generic;
using g3;

namespace geometry3Test 
{
	public static class test_Math {

		public static double ClampAngleDeg(double theta, double min, double max)
		{
			// convert interval to center/extent - [c-e,c+e]
			double c = (min+max)*0.5;
			double e = max-c;

			// get rid of extra rotations
			theta = theta % 360;

			// shift to origin, then convert theta to +- 180
			theta -= c;
			if ( theta < -180 )
				theta += 360;
			else if ( theta > 180 ) {
				theta -= 360;
			}

			// clamp to extent
			if ( theta < -e )
				theta = -e;
			else if ( theta > e )
				theta = e;

			// shift back
			return theta + c;
		}


		public static void test_AngleClamp() {
			List<Vector2d> intervals = new List<Vector2d>();
			intervals.Add( new Vector2d(0, 90) );
			intervals.Add( new Vector2d(0, 180) );
			intervals.Add( new Vector2d(0, 270) );
			intervals.Add( new Vector2d(0, 350) );
			intervals.Add( new Vector2d(180, 359) );
			intervals.Add( new Vector2d(-1, 45) );
			intervals.Add( new Vector2d(-1, 180) );
			intervals.Add( new Vector2d(-1, 270) );
			intervals.Add( new Vector2d(-90, 90) );
			intervals.Add( new Vector2d(-90, 180) );
			intervals.Add( new Vector2d(-90, 260) );
			intervals.Add( new Vector2d(-180, 90) );

			foreach ( Vector2d i in intervals ) {
				double c = (i.x + i.y) * 0.5;
				double e = i.y - c;

				double clamped = ClampAngleDeg(c+0.5*e, i.x, i.y);
				Util.gDevAssert(clamped == c+0.5*e);
				clamped = ClampAngleDeg(c-0.5*e, i.x, i.y);
				Util.gDevAssert(clamped == c-0.5*e);
				clamped = ClampAngleDeg(c, i.x, i.y);
				Util.gDevAssert(clamped == c);

				
				clamped = ClampAngleDeg(c+e+1, i.x,i.y);
				Util.gDevAssert(clamped == i.y);
				clamped = ClampAngleDeg(c-e-1, i.x, i.y);
				Util.gDevAssert(clamped == i.x);

				clamped = ClampAngleDeg(c+e+1+360, i.x,i.y);
				Util.gDevAssert(clamped == i.y);
				clamped = ClampAngleDeg(c-e-1+360, i.x, i.y);
				Util.gDevAssert(clamped == i.x);

				clamped = ClampAngleDeg(c+e+1-360, i.x,i.y);
				Util.gDevAssert(clamped == i.y);
				clamped = ClampAngleDeg(c-e-1-360, i.x, i.y);
				Util.gDevAssert(clamped == i.x);

				clamped = ClampAngleDeg(c+e+1+720, i.x,i.y);
				Util.gDevAssert(clamped == i.y);
				clamped = ClampAngleDeg(c-e-1+720, i.x, i.y);
				Util.gDevAssert(clamped == i.x);

				clamped = ClampAngleDeg(c+e+1-720, i.x,i.y);
				Util.gDevAssert(clamped == i.y);
				clamped = ClampAngleDeg(c-e-1-720, i.x, i.y);
				Util.gDevAssert(clamped == i.x);
			}
		}


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
