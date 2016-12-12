using System;
using System.Collections.Generic;
using g3;


namespace geometry3Test 
{
	public static class TestUtil 
	{
		public const string WRITE_PATH = "/Users/rms/scratch/";

		public static void write_mesh(IMesh mesh, string sfilename) 
		{
			OBJWriter writer = new OBJWriter();
			var s = new System.IO.StreamWriter(WRITE_PATH + sfilename, false);
			writer.Write(s, new List<IMesh> {mesh}, new WriteOptions() );
			s.Close();
		}


		public static DMesh3 MakeOpenCylinder(bool bNoSharedVertices) 
		{ 
			DMesh3 mesh = new DMesh3();
			OpenCylinderGenerator cylgen = new OpenCylinderGenerator() { NoSharedVertices = bNoSharedVertices };
			cylgen.Generate();
			cylgen.MakeMesh(mesh);
			return mesh;
		}

		public static DMesh3 MakeCappedCylinder(bool bNoSharedVertices) 
		{ 
			DMesh3 mesh = new DMesh3();
			CappedCylinderGenerator cylgen = new CappedCylinderGenerator() { NoSharedVertices = bNoSharedVertices };
			cylgen.Generate();
			cylgen.MakeMesh(mesh);
			return mesh;
		}
	}
}
