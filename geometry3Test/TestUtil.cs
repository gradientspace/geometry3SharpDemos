using System;
using System.Collections.Generic;
using g3;


namespace geometry3Test 
{
	public static class TestUtil 
	{
		public static string WRITE_PATH {
            get {
                if (Util.IsRunningOnMono())
                    return "/Users/rms/scratch/";
                else
                    return "c:\\scratch\\";
            }
        }



        public static void WriteDebugMesh(IMesh mesh, string sfilename)
        {
            OBJWriter writer = new OBJWriter();
            var s = new System.IO.StreamWriter(WRITE_PATH + sfilename, false);
            writer.Write(s, new List<IMesh> { mesh }, new WriteOptions() { bWriteGroups = true } );
			s.Close();
		}


		public static DMesh3 MakeOpenCylinder(bool bNoSharedVertices, int nSlices = 16) 
		{ 
			DMesh3 mesh = new DMesh3();
			OpenCylinderGenerator cylgen = new OpenCylinderGenerator() {
                NoSharedVertices = bNoSharedVertices, Slices = nSlices };
			cylgen.Generate();
			cylgen.MakeMesh(mesh);
			mesh.ReverseOrientation();
			return mesh;
		}

		public static DMesh3 MakeCappedCylinder(bool bNoSharedVertices, int nSlices = 16) 
		{ 
			DMesh3 mesh = new DMesh3(true, false, false, true);
			CappedCylinderGenerator cylgen = new CappedCylinderGenerator() {
                NoSharedVertices = bNoSharedVertices, Slices = nSlices };
			cylgen.Generate();
			cylgen.MakeMesh(mesh);
			mesh.ReverseOrientation();
			return mesh;
		}
	}
}
