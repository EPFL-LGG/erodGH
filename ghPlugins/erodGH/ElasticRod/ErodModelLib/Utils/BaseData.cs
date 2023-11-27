using System;
using System.Collections.Generic;
using ErodModelLib.Types;
using Rhino.Geometry;
using Speckle.Core.Models;

namespace ErodModelLib.Utils
{
    public class BaseData : Base
    {
        public bool ContainsFlatLinkage { get; set; }
        public bool ContainsDeployLinkage { get; set; }
        public bool ContainsEditedSurfaces { get; set; }

        [DetachProperty]
        public BaseLinkage FlatLinkage { get; set; }
        [DetachProperty]
        public BaseLinkage DeployLinkage { get; set; }

        public Dictionary<string, BaseTargetSurface> EditedSurfaces { get; private set; }

        public BaseData() : base() {}

        public BaseData(BaseCurveNetwork network, BaseLinkage flat, BaseLinkage deploy, Dictionary<string, BaseTargetSurface> editedMeshes=null)
        {
            this["CurveNetwork"] = network;
            ContainsFlatLinkage = false;
            ContainsDeployLinkage = false;
            ContainsEditedSurfaces = false;
            FlatLinkage = new BaseLinkage();
            DeployLinkage = new BaseLinkage();

            if (flat != null)
            {
                FlatLinkage = flat;
                this["ContainsFlatLinkage"] = true;
            }
            if (deploy != null)
            {
                DeployLinkage = deploy;
                ContainsDeployLinkage = true;
            }

            if (editedMeshes != null)
            {
                this["EditedSurfaces"] = editedMeshes;
                ContainsEditedSurfaces = true;
            }
        }

        public void AddEditedSurfaces(Dictionary<string, BaseTargetSurface> editedMeshes) {
            EditedSurfaces = editedMeshes;
            ContainsEditedSurfaces = true;
        }

        public override string ToString()
        {
            if (ContainsEditedSurfaces) return "BaseLinkageWithEdits";
            else return "BaseLinkage";
        }
    }
}
