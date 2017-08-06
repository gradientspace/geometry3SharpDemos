using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using g3;

namespace geometry3Test
{
    public static class test_Grids
    {

        public static void test_levelset_basic()
        {
            //DMesh3 mesh = TestUtil.MakeCappedCylinder(false);
            //MeshTransforms.Scale(mesh, 1, 3, 1);
            DMesh3 mesh = TestUtil.LoadTestMesh(Program.TEST_FILES_PATH + "bunny_open_base.obj");

            AxisAlignedBox3d bounds = mesh.CachedBounds;
            float cellSize = (float)bounds.MaxDim / 32.0f;

            NarrowBandLevelSet levelSet = new NarrowBandLevelSet(mesh, cellSize);
            levelSet.Compute();

            Vector3i dims = levelSet.Dimensions;
            int midx = dims.x / 2;
            int midy = dims.y / 2;
            int midz = dims.z / 2;
            //for ( int xi = 0; xi < dims.x; ++xi ) {
            //    System.Console.Write(levelSet[xi, yi, zi] + " ");
            //}
            for ( int yi = 0; yi < dims.y; ++yi ) {
                System.Console.Write(levelSet[midx, yi, midz] + " ");
            }
            System.Console.WriteLine();

            DMesh3 tmp = new DMesh3();
            MeshEditor editor = new MeshEditor(tmp);
            for ( int x = 0; x < dims.x; ++x ) {
                for ( int y = 0; y < dims.y; ++y ) {
                    for ( int z = 0; z < dims.z; ++z ) {
                        if (levelSet[x, y, z] < 0) {
                            Vector3f c = levelSet.CellCenter(x, y, z);
                            editor.AppendBox(new Frame3f(c), cellSize);
                        }
                    }
                }
            }
            TestUtil.WriteTestOutputMesh(tmp, "LevelSetInterior.obj");
            TestUtil.WriteTestOutputMesh(mesh, "LevelSetInterior_src.obj");
        }
    }
}
