using System;
using System.Collections.Generic;
using System.Diagnostics;
using g3;

namespace geometry3Test 
{
	public static class test_Deformers {


        // [RMS] this only tests some basic cases...
        public static void test_LaplacianDeformation()
        {
            // compact version
            DMesh3 mesh = new DMesh3(TestUtil.MakeRemeshedCappedCylinder(1.0), true);
            Debug.Assert(mesh.IsCompact);
            AxisAlignedBox3d bounds = mesh.GetBounds();

            List<IMesh> result_meshes = new List<IMesh>();

            LaplacianMeshDeformer deformer = new LaplacianMeshDeformer(mesh);

            // constrain bottom points
            foreach (int vid in mesh.VertexIndices()) {
                Vector3d v = mesh.GetVertex(vid);
                bool bottom = (v.y - bounds.Min.y) < 0.01f;
                if (bottom)
                    deformer.SetConstraint(vid, v, 10);
            }

            // constrain one other vtx
            int ti = MeshQueries.FindNearestTriangle_LinearSearch(mesh, new Vector3d(2, 5, 2));
            int v_pin = mesh.GetTriangle(ti).a;
            Vector3d cons_pos = mesh.GetVertex(v_pin);
            cons_pos += new Vector3d(0.5,0.5,0.5);
            deformer.SetConstraint(v_pin, cons_pos, 10);
            result_meshes.Add(TestUtil.MakeMarker(mesh.GetVertex(v_pin), 0.2f, Colorf.Red));

            deformer.Initialize();
            Vector3d[] resultV = new Vector3d[mesh.MaxVertexID];
            deformer.Solve(resultV);

            foreach (int vid in mesh.VertexIndices()) {
                mesh.SetVertex(vid, resultV[vid]);
            }

            result_meshes.Add(mesh);
            TestUtil.WriteDebugMeshes(result_meshes, "___LAPLACIAN_result.obj");
        }


	}
}
