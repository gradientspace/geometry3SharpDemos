using System;
using System.Collections.Generic;
using g3;

namespace geometry3Test
{
	public static class test_Polygon
	{
		
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


	}
}
