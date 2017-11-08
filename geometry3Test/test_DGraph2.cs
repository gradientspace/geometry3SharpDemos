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
                foreach (Vector2d v in poly)
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
