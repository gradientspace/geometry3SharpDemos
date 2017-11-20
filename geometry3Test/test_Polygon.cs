using System;
using System.Collections.Generic;
using System.Linq;
using g3;

namespace geometry3Test
{
	public static class test_Polygon
	{


		public static void test_winding()
		{
			Random r = new Random(31337);
			int NPTS = 1000;

			double radius = 1;
			Polygon2d poly = Polygon2d.MakeCircle(radius, 777);
			Vector2d[] testPts = TestUtil.RandomPoints2(NPTS, r, Vector2d.Zero, radius);
			foreach(Vector2d v in testPts) {
				bool really_inside = (v.Length < radius);
				bool inside = poly.Contains(v);
				double winding0 = poly.WindingIntegral(v);
				bool inside_winding = ! (Math.Abs(winding0) < MathUtil.Epsilonf);
				if (really_inside != inside || really_inside != inside_winding) {
					System.Console.WriteLine("Failed! truth {0}  inside {1}   winding0 {2}",
										 really_inside, inside, winding0);
				}
			}

			// test random polygons
			int NPOLYS = 100;
			for (int k = 0; k < NPOLYS; ++k) {
				poly = new Polygon2d(TestUtil.RandomPoints2(30, r, Vector2d.Zero, radius));
				testPts = TestUtil.RandomPoints2(NPTS, r, Vector2d.Zero, radius);
				foreach (Vector2d v in testPts) {
					bool inside = poly.Contains(v);
					double winding0 = poly.WindingIntegral(v);
					bool inside_winding = !(Math.Abs(winding0) < MathUtil.Epsilonf);
					if (inside != inside_winding) {
						System.Console.WriteLine("Failed! inside {0}   winding0 {1}", inside, winding0);
					}
				}
			}
		}





		
		public static void test_svg()
		{
			Polygon2d poly = Polygon2d.MakeCircle(100.0f, 10);
			PolyLine2d pline = new PolyLine2d();
			pline.AppendVertex(Vector2d.Zero);
			pline.AppendVertex(200 * Vector2d.AxisX);
			pline.AppendVertex(200 * Vector2d.One);
			Circle2d circ = new Circle2d(33 * Vector2d.One, 25);
			Segment2d seg = new Segment2d(Vector2d.Zero, -50 * Vector2d.AxisY);

			SVGWriter writer = new SVGWriter();
			writer.AddPolygon(poly, SVGWriter.Style.Filled("lime", "black", 0.25f) );
			writer.AddPolyline(pline, SVGWriter.Style.Outline("orange", 2.0f));
			writer.AddCircle(circ, SVGWriter.Style.Filled("yellow", "red", 5.0f));
			writer.AddLine(seg, SVGWriter.Style.Outline("blue", 10.0f));

			writer.Write(TestUtil.GetTestOutputPath("test.svg"));
		}



		public static void test_tiling()
		{
			Vector2d origin = Vector2d.Zero;
			double radius = 22;
			Circle2d circ = new Circle2d(origin, radius);
			AxisAlignedBox2d elemBounds = circ.Bounds;
			//elemBounds.Max.x += radius / 2;

			AxisAlignedBox2d packBounds = new AxisAlignedBox2d(0, 0, 800, 400);
			double spacing = 5;
			Polygon2d boundsPoly = new Polygon2d();
			for (int i = 0; i < 4; ++i)
				boundsPoly.AppendVertex(packBounds.GetCorner(i));

			//List<Vector2d> packed = TilingUtil.BoundedRegularTiling2(elemBounds, packBounds, spacing);
			List<Vector2d> packed = TilingUtil.BoundedCircleTiling2(elemBounds, packBounds, spacing);

			System.Console.WriteLine("packed {0}", packed.Count);

			SVGWriter writer = new SVGWriter();
			foreach (Vector2d t in packed) {
				writer.AddCircle(new Circle2d(origin + t, radius), SVGWriter.Style.Outline("black", 1.0f));
			}
			writer.AddPolygon(boundsPoly, SVGWriter.Style.Outline("red", 2.0f));
			writer.Write(TestUtil.GetTestOutputPath("test.svg"));
		}






        public static void test_convex_hull_2()
        {
            Random r = new Random(31337);

            //LocalProfiler p = new LocalProfiler();
            //p.Start("Hulls");

            QueryNumberType[] modes = new QueryNumberType[] { QueryNumberType.QT_DOUBLE, QueryNumberType.QT_INT64 };

            foreach (var queryMode in modes) {

                for (int k = 0; k < 1000; ++k) {

                    int N = 2500;
                    double scale = (r.NextDouble() + 0.1) * 1024.0;

                    Vector2d[] pts = TestUtil.RandomPoints2(N, r, Vector2d.Zero, scale);

                    double eps = MathUtil.Epsilonf;

                    ConvexHull2 hull = new ConvexHull2(pts, eps, queryMode);
                    Polygon2d hullPoly = hull.GetHullPolygon();

                    foreach (Vector2d v in pts) {
                        if (hullPoly.Contains(v))
                            continue;
                        double d = hullPoly.DistanceSquared(v);
                        if (d < eps)
                            continue;
                        System.Console.WriteLine("test_convex_hull: Point {0} not contained!", v);
                    }
                }
            }

            //p.StopAll();
            //System.Console.WriteLine(p.AllTimes());

            //SVGWriter writer = new SVGWriter();
            //foreach (Vector2d v in pts) { 
            //    writer.AddCircle(new Circle2d(v, 3.0), SVGWriter.Style.Outline("black", 1.0f));
            //}
            //writer.AddPolygon(hullPoly, SVGWriter.Style.Outline("red", 2.0f));
            //writer.Write(TestUtil.GetTestOutputPath("test.svg"));
        }










        public static void test_min_box_2()
        {
            Random r = new Random(31337);

            bool write_svg = false;
            int contained_circles_N = 100;
            int test_iters = 1000;

            //LocalProfiler p = new LocalProfiler();
            //p.Start("Hulls");

            QueryNumberType[] modes = new QueryNumberType[] { QueryNumberType.QT_DOUBLE, QueryNumberType.QT_INT64 };
            //QueryNumberType[] modes = new QueryNumberType[] { QueryNumberType.QT_DOUBLE };

            foreach (var queryMode in modes) {

                for (int k = 0; k < test_iters; ++k) {

                    int N = contained_circles_N;
                    double scale = (r.NextDouble() + 0.1) * 1024.0;
                    Interval1d radRange = new Interval1d(10, 100);

                    Vector2d[] pts = TestUtil.RandomPoints2(N, r, Vector2d.Zero, scale);
                    double[] radius = TestUtil.RandomScalars(N, r, new Interval1d(radRange));

                    double eps = MathUtil.Epsilonf;

                    SVGWriter svg = (write_svg) ? new SVGWriter() : null;

                    List<Vector2d> accumPts = new List<Vector2d>();
                    for ( int i = 0; i < pts.Length; ++i ) {
                        Polygon2d circ = Polygon2d.MakeCircle(radius[i], 16, radius[i]);
                        circ.Translate(pts[i]);
                        accumPts.AddRange(circ.Vertices);

                        if ( svg != null )
                            svg.AddPolygon(circ, SVGWriter.Style.Outline("black", 1.0f));
                    }

                    ContMinBox2 contbox = new ContMinBox2(accumPts, 0.001, queryMode, false);
                    Box2d box = contbox.MinBox;

                    if (svg != null) {
                        svg.AddPolygon(new Polygon2d(box.ComputeVertices()), SVGWriter.Style.Outline("red", 2.0f));
                        svg.Write(TestUtil.GetTestOutputPath("contbox.svg"));
                    }

                    foreach (Vector2d v in accumPts) {
                        if (box.Contains(v))
                            continue;
                        double d = box.DistanceSquared(v);
                        if (d < eps)
                            continue;
                        System.Console.WriteLine("test_min_box_2: Point {0} not contained!", v);
                    }
                }
            }

            //p.StopAll();
            //System.Console.WriteLine(p.AllTimes());
        }





        // exports svg w/ different containments of point set (created by slicing mesh)
        public static void containment_demo_svg()
        {
            DMesh3 mesh = TestUtil.LoadTestInputMesh("bunny_solid.obj");
            MeshTransforms.Scale(mesh, 4);

            AxisAlignedBox3d meshBounds = mesh.CachedBounds;
            Vector3d origin = meshBounds.Center;
            origin -= 0.2 * meshBounds.Height * Vector3d.AxisY;
            Frame3f plane = new Frame3f(origin, new Vector3d(1,3,0).Normalized);
            MeshPlaneCut cut = new MeshPlaneCut(mesh, plane.Origin, plane.Z);
            cut.Cut();

            AxisAlignedBox2d polyBounds = AxisAlignedBox2d.Empty;
            List<Polygon2d> polys = new List<Polygon2d>();
            foreach (EdgeLoop loop in cut.CutLoops) {
                Polygon2d poly = new Polygon2d();
                foreach (int vid in loop.Vertices)
                    poly.AppendVertex(mesh.GetVertex(vid).xz);
                poly.Rotate(new Matrix2d(90,true), Vector2d.Zero);
                polys.Add(poly);
                polyBounds.Contain(poly.Bounds);
            }

            SVGWriter svg = new SVGWriter();
            var polyStyle = SVGWriter.Style.Outline("red", 1.0f);
            var contStyle = SVGWriter.Style.Outline("black", 1.0f);

            for ( int k = 0; k < 3; ++k ) {
                double shift = (k == 2) ? 1.4f : 1.1f;
                Vector2d tx = (k-1) * (polyBounds.Width * shift) * Vector2d.AxisX;
                List<Vector2d> pts = new List<Vector2d>();
                foreach (Polygon2d poly in polys) {
                    var p2 = new Polygon2d(poly).Translate(tx);
                    pts.AddRange(p2.Vertices);
                    svg.AddPolygon(p2, polyStyle);
                }

                if ( k == 0 ) {
                    ConvexHull2 hull = new ConvexHull2(pts, 0.001, QueryNumberType.QT_DOUBLE);
                    svg.AddPolygon(hull.GetHullPolygon(), contStyle);
                } else if ( k == 1 ) {
                    ContMinBox2 contbox = new ContMinBox2(pts, 0.001, QueryNumberType.QT_DOUBLE, false);
                    svg.AddPolygon(new Polygon2d(contbox.MinBox.ComputeVertices()), contStyle);
                } else if (k == 2) {
                    ContMinCircle2 contcirc = new ContMinCircle2(pts);
                    svg.AddCircle(contcirc.Result, contStyle);
                }

            }


            svg.Write(TestUtil.GetTestOutputPath("contain_demos.svg"));
        }


    }
}
