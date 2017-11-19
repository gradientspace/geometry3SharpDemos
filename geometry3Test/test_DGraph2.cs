using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using g3;

namespace geometry3Test
{
    public static class test_DGraph2
    {



        public static void test_cells()
        {
            Polygon2d outer = Polygon2d.MakeCircle(1000, 17);
            Polygon2d hole = Polygon2d.MakeCircle(100, 32); hole.Reverse();
            GeneralPolygon2d gpoly = new GeneralPolygon2d(outer);
            gpoly.AddHole(hole);

            DGraph2 graph = new DGraph2();
            graph.AppendPolygon(gpoly);

            GraphSplitter2d splitter = new GraphSplitter2d(graph);
            splitter.InsideTestF = gpoly.Contains;
            for (int k = 0; k < outer.VertexCount; ++k) {
                Line2d line = new Line2d(outer[k], Vector2d.AxisY);
                splitter.InsertLine(line);
            }
            for (int k = 0; k < outer.VertexCount; ++k) {
                Line2d line = new Line2d(outer[k], Vector2d.AxisX);
                splitter.InsertLine(line);
            }
            for (int k = 0; k < outer.VertexCount; ++k) {
                Line2d line = new Line2d(outer[k], Vector2d.One.Normalized);
                splitter.InsertLine(line);
            }
            for (int k = 0; k < outer.VertexCount; ++k) {
                Line2d line = new Line2d(outer[k], new Vector2d(1, -1).Normalized);
                splitter.InsertLine(line);
            }

            GraphCells2d cells = new GraphCells2d(graph);
            cells.FindCells();

            List<Polygon2d> polys = cells.ContainedCells(gpoly);

            for (int k = 0; k < polys.Count; ++k) {
                double offset = polys[k].IsClockwise ? 4 : 20;
                polys[k].PolyOffset(offset);
            }


            PlanarComplex cp = new PlanarComplex();
            for (int k = 0; k < polys.Count; ++k)
                cp.Add(polys[k]);

            // convert back to solids
            var options = PlanarComplex.FindSolidsOptions.Default;
            options.WantCurveSolids = false;
            options.SimplifyDeviationTolerance = 0;
            var solids = cp.FindSolidRegions(options);

            SVGWriter svg = new SVGWriter();
            svg.AddGraph(graph, SVGWriter.Style.Outline("red", 5));
            for (int k = 0; k < polys.Count; ++k)
                svg.AddPolygon(polys[k], SVGWriter.Style.Outline("black", 1));

            svg.Write(TestUtil.GetTestOutputPath("cells_graph.svg"));
        }







        public static void test_splitter()
        {
            Polygon2d poly = Polygon2d.MakeCircle(1000, 16);
            Polygon2d hole = Polygon2d.MakeCircle(500, 32); hole.Reverse();
            GeneralPolygon2d gpoly = new GeneralPolygon2d(poly);
            gpoly.AddHole(hole);
            //Polygon2d poly = Polygon2d.MakeRectangle(Vector2d.Zero, 1000, 1000);

            DGraph2 graph = new DGraph2();
            graph.AppendPolygon(gpoly);

            System.Console.WriteLine("Stats before: verts {0} edges {1} ", graph.VertexCount, graph.EdgeCount);

            GraphSplitter2d splitter = new GraphSplitter2d(graph);
            splitter.InsideTestF = gpoly.Contains;

            for (int k = 0; k < poly.VertexCount; ++k) {
                Line2d line = new Line2d(poly[k], Vector2d.AxisY);
                splitter.InsertLine(line);
            }
            System.Console.WriteLine("Stats after 1: verts {0} edges {1} ", graph.VertexCount, graph.EdgeCount);
            for (int k = 0; k < poly.VertexCount; ++k) {
                Line2d line = new Line2d(poly[k], Vector2d.AxisX);
                splitter.InsertLine(line);
            }
            for (int k = 0; k < poly.VertexCount; ++k) {
                Line2d line = new Line2d(poly[k], Vector2d.One.Normalized);
                splitter.InsertLine(line);
            }
            for (int k = 0; k < poly.VertexCount; ++k) {
                Line2d line = new Line2d(poly[k], new Vector2d(1,-1).Normalized);
                splitter.InsertLine(line);
            }

            System.Console.WriteLine("Stats after: verts {0} edges {1} ", graph.VertexCount, graph.EdgeCount);


            Random r = new Random(31337);
            foreach (int vid in graph.VertexIndices()) {
                Vector2d v = graph.GetVertex(vid);
                v += TestUtil.RandomPoints(1, r, v, 25)[0];
                graph.SetVertex(vid, v);
            }



            SVGWriter svg = new SVGWriter();
            svg.AddGraph(graph);

            var vtx_style = SVGWriter.Style.Outline("red", 1.0f);
            foreach (int vid in graph.VertexIndices()) {
                Vector2d v = graph.GetVertex(vid);
                svg.AddCircle(new Circle2d(v, 10), vtx_style);
            }

            svg.Write(TestUtil.GetTestOutputPath("split_graph.svg"));
        }












        public static void test_arrangement_stress()
        {
            Random r = new Random(31337);

            Arrangement2d builder = new Arrangement2d(new AxisAlignedBox2d(1024.0));

            Polygon2d circ = Polygon2d.MakeCircle(512, 33);
            builder.Insert(circ);

            // crazy stress-test
            for ( int k = 0; k < 1000; ++k ) {
                var pts = TestUtil.RandomPoints(2, r, circ.Bounds.Center, 800);
                builder.Insert(new Segment2d(pts[0], pts[1]));
            }

            //TestUtil.WriteTestOutputGraph(builder.Graph, "graph_complex.svg");
        }



        public static void test_arrangement_demo()
        {
            DMesh3 mesh = TestUtil.LoadTestInputMesh("spheres_and_planes.obj");
            MeshTransforms.Scale(mesh, 8);
            AxisAlignedBox3d meshBounds = mesh.CachedBounds;
            Vector3d origin = meshBounds.Center;
            double simplify_thresh = 5.0;

            Frame3f plane = new Frame3f(origin, Vector3d.AxisY);
            MeshPlaneCut cut = new MeshPlaneCut(mesh, plane.Origin, plane.Z);
            cut.Cut();

            Arrangement2d builder = new Arrangement2d(new AxisAlignedBox2d(1024.0));

            // insert all cut edges
            HashSet<Vector2d> srcpts = new HashSet<Vector2d>();
            foreach (EdgeLoop loop in cut.CutLoops) {
                Polygon2d poly = new Polygon2d();
                foreach (int vid in loop.Vertices)
                    poly.AppendVertex(mesh.GetVertex(vid).xz);

                poly.Simplify(simplify_thresh, 0.01, true);
                foreach (Vector2d v in poly.Vertices)
                    srcpts.Add(v);

                builder.Insert(poly);
            }
            foreach (EdgeSpan span in cut.CutSpans) {
                PolyLine2d pline = new PolyLine2d();
                foreach (int vid in span.Vertices)
                    pline.AppendVertex(mesh.GetVertex(vid).xz);
                pline.Simplify(simplify_thresh, 0.01, true);
                foreach (Vector2d v in pline)
                    srcpts.Add(v);
                builder.Insert(pline);
            }

            SVGWriter svg = new SVGWriter();
            svg.AddGraph(builder.Graph);

            var vtx_style = SVGWriter.Style.Outline("red", 1.0f);
            foreach ( int vid in builder.Graph.VertexIndices()) {
                Vector2d v = builder.Graph.GetVertex(vid);
                if (srcpts.Contains(v) == false)
                    svg.AddCircle(new Circle2d(v, 2), vtx_style);
            }

            svg.Write(TestUtil.GetTestOutputPath("arrangement.svg"));
        }




    }
}
