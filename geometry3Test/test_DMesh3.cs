using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using g3;

namespace geometry3Test
{
    public class test_DMesh3
    {
        public static void basic_tests()
        {
            System.Console.WriteLine("DMesh3:basic_tests() starting");

            DMesh3 tmp = new DMesh3();
            CappedCylinderGenerator cylgen = new CappedCylinderGenerator();
            cylgen.Generate();
            cylgen.MakeMesh(tmp);

            tmp.CheckValidity();

            System.Console.WriteLine("cylinder ok");

        }


		public static void split_tests(bool bTestBoundary, int N = 100) {
			System.Console.WriteLine("DMesh3:split_tests() starting");

			DMesh3 mesh = new DMesh3();
			CappedCylinderGenerator cylgen = new CappedCylinderGenerator() { NoSharedVertices = bTestBoundary };
			cylgen.Generate();
			cylgen.MakeMesh(mesh);
			mesh.CheckValidity();

			Random r = new Random(31377);
			for ( int k = 0; k < N; ++k ) {
				int eid = r.Next() % mesh.EdgeCount;
				if ( ! mesh.IsEdge(eid) )
					continue;

				DMesh3.EdgeSplitInfo splitInfo; 
				MeshResult result = mesh.SplitEdge(eid, out splitInfo);
				Debug.Assert(result == MeshResult.Ok);
				mesh.CheckValidity();
			}

			System.Console.WriteLine("splits ok");
		}


		public static void flip_tests(bool bTestBoundary, int N = 100) {
			System.Console.WriteLine("DMesh3:flip_tests() starting");

			DMesh3 mesh = new DMesh3();
			CappedCylinderGenerator cylgen = new CappedCylinderGenerator() { NoSharedVertices = bTestBoundary };
			cylgen.Generate();
			cylgen.MakeMesh(mesh);
			mesh.CheckValidity();

			Random r = new Random(31377);
			for ( int k = 0; k < N; ++k ) {
				int eid = r.Next() % mesh.EdgeCount;
				if ( ! mesh.IsEdge(eid) )
					continue;
				bool bBoundary = mesh.edge_is_boundary(eid);

				DMesh3.EdgeFlipInfo flipInfo; 
				MeshResult result = mesh.FlipEdge(eid, out flipInfo);
				if ( bBoundary )
					Debug.Assert(result == MeshResult.Failed_IsBoundaryEdge);
				else
					Debug.Assert(result == MeshResult.Ok || result == MeshResult.Failed_FlippedEdgeExists);
				mesh.CheckValidity();
			}

			System.Console.WriteLine("flips ok");
		}


    }
}
