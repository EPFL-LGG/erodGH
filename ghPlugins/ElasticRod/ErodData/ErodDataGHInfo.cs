using System;
using System.Drawing;
using Grasshopper;
using Grasshopper.Kernel;

namespace ErodDataGH
{
    public class ErodDataGHInfo : GH_AssemblyInfo
    {
        public override string Name => "ErodDataGH Info";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("0E998D77-3066-41E4-9682-3400532AB6EB");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}
