using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using g3;

namespace geometry3Test
{
	static class Program
	{
		public static string TEST_FILES_PATH {
            get { return Util.IsRunningOnMono() ? "../../test_files/" : "..\\..\\test_files\\"; }
        }
        public static string TEST_OUTPUT_PATH {
			get { return Util.IsRunningOnMono() ? "../../test_output/" : "..\\..\\test_output\\"; }
		}


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
#if DEBUG
            Util.DebugBreakOnDevAssert = true;
#endif


            //test_VectorTypes.test_rcvector();
            //test_VectorTypes.test_bitarrays();
            //test_Math.test_VectorTanCot();
            //test_Math.test_AngleClamp();
            //test_Math.test_RayBoxIntersect();
            //test_MathTypes.all_tests();

            //test_MeshIO.test_simple_obj();
            //test_MeshIO.test_write_formats();
            //test_MeshIO.test_read_thingi10k();

			//test_VectorTypes.test_pq();
			//test_VectorTypes.profile_pq();

			//test_dmesh();
			//test_DMesh3.merge_test_closed_mesh();
			//test_DMesh3.copy_performance();
			//test_DMesh3.performance_grinder();

			//test_ntmesh();

			//test_Remesher.WriteDebugMeshes = true;
			//test_Remesher.test_basic_closed_remesh();
			//test_Remesher.test_remesh_smoothing();
			//test_Remesher.test_remesh_constraints_fixedverts();
			//test_Remesher.test_remesh_constraints_vertcurves();
			//test_Remesher.test_remesh_region();

			//test_Reducer.test_basic_closed_reduce();
			//test_Reducer.test_reduce_constraints_fixedverts();
			//test_Reducer.test_reduce_profiling();

			//test_Spatial.test_AABBTree_basic();
			//test_Spatial.test_AABBTree_TriDist();
			//test_Spatial.test_AABBTree_profile();
			//test_Spatial.test_AABBTree_RayHit();
			//test_Spatial.test_AABBTree_TriTriDist();
			//test_Spatial.test_AABBTree_TriTriIntr();
			//test_Spatial.test_Winding();

			test_MeshGen.test_basic_generators();
            //test_MeshGen.test_voxel_surface();
            //test_MeshGen.test_mesh_builders();
            //test_MeshGen.test_marching_cubes();
            //test_MeshGen.test_marching_cubes_levelset();
            //test_MeshGen.test_marching_cubes_topology();
            //test_MeshGen.test_marching_cubes_implicits();
            //test_MeshGen.test_marching_cubes_demos();

            //test_Solvers.test_Matrices();
            //test_Solvers.test_SparseCG();
            //test_Solvers.test_Laplacian_deformer();
            //test_Solvers.test_SparseCG_Precond();

            //test_Deformers.test_LaplacianDeformation();

            //test_MeshEdits.test_basic_fills();
            //test_MeshEdits.test_plane_cut();
            //test_MeshEdits.test_planar_fill();

            //test_MeshBoundary.test_mesh_boundary();
            //test_MeshRegionBoundary.test_region_boundary();

            //test_Grids.test_levelset_basic();

            //test_Dijkstra.test_dijkstra();
            //test_Dijkstra.profile_dijkstra_2b(500);
            //test_Dijkstra.profile_dijkstra_2b_reuse(500);

            //test_Dijkstra.test_local_param();
            //test_Dijkstra.test_uv_insert_segment();
            //test_Dijkstra.test_uv_insert_string();

            //test_Polygon.test_winding();
            //test_Polygon.profile_winding();
            //test_Polygon.test_svg();
            //test_Polygon.test_tiling();
            //test_Polygon.test_convex_hull_2();
            //test_Polygon.test_min_box_2();
            //test_Polygon.containment_demo_svg();
            //test_Polygon.test_chamfer();

            //test_DGraph2.test_arrangement_stress();
            //test_DGraph2.test_arrangement_demo();
            //test_DGraph2.test_splitter();
            //test_DGraph2.test_cells();

            //test_Debugging.test();

            System.Console.WriteLine("Done tests, press enter key to exit");
            System.Console.ReadLine();
        }


		static void test_dmesh() {
			test_DMesh3.basic_tests();
            test_DMesh3.test_normals();
            test_DMesh3.test_remove();

            test_DMesh3.test_insert();
            test_DMesh3.test_remove_change_apply();
            test_DMesh3.test_remove_change_construct();
            test_DMesh3.test_add_change();


            int split_rounds = 100;
			test_DMesh3.split_tests(true, split_rounds);
			test_DMesh3.split_tests(false, split_rounds);

			int flip_rounds = 100;
			test_DMesh3.flip_tests(true, flip_rounds);
			test_DMesh3.flip_tests(false, flip_rounds);

			int collapse_rounds = 1000;
			test_DMesh3.collapse_tests(true, collapse_rounds);
			test_DMesh3.collapse_tests(false, collapse_rounds);

			test_DMesh3.collapse_test_convergence_cyl_noshared();
			test_DMesh3.collapse_test_closed_mesh();
			test_DMesh3.collapse_test_convergence_opencyl();

            test_DMesh3.poke_test();

            test_DMesh3.test_compact_in_place();
        }




        static void test_ntmesh()
        {
            test_NTMesh3.basic_tests();
            test_NTMesh3.test_remove();
            test_NTMesh3.poke_test();

            test_NTMesh3.split_tests(true);
            test_NTMesh3.split_tests(false);
            test_NTMesh3.split_tests_nonmanifold();
        }

    }
}
