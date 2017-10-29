using System;
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
	}
}
