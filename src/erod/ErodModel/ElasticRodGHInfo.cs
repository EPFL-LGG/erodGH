using System;
using System.Drawing;
using Grasshopper;
using Grasshopper.Kernel;

namespace ElasticRodGH
{
    public class ElasticRodGHInfo : GH_AssemblyInfo
    {
        public override string Name => "ElasticRodGH Info";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("973FDE5C-9CA4-474D-8D54-1D6E0D076C8A");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}