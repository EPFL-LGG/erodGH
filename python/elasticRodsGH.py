
import sys
sys.path.append('../../python')

import numpy as np
import elastic_rods
from elastic_rods import InterleavingType
import json

class linkageData:
    def __init__(self, filename : str, consistent_angle : bool = True):
        
        # Import converted data
        with open(filename) as json_file:
            data = json.load(json_file)
        
        # Interleaving
        # InterleavingType { xshell=0, weaving=1, noOffset=2, triaxialWeave=3 };
        itype = data['Interleaving']
        interleaving = InterleavingType.xshell
        if(itype==1):
            interleaving = InterleavingType.weaving
        elif(itype==2):
            interleaving = InterleavingType.noOffset
        elif(itype==3):
            interleaving = InterleavingType.triaxialWeave

        # Vertices data
        vertices = data['Vertices']
        num_vertices = len(vertices)
        self.vertices = np.ndarray(shape=(num_vertices, 3))
        self.normals = np.ndarray(shape=(num_vertices, 3))
        for i in range(num_vertices):
            j = vertices[i]
            pos = j['Location']
            self.vertices[i] = [float(pos['X']),float(pos['Y']),float(pos['Z'])]
            norm = j['Normal']
            self.normals[i] = [float(norm['X']),float(norm['Y']), float(norm['Z'])]
            
        # Edges data
        edges = data['Segments']
        num_edges = len(edges)
        self.edges = np.ndarray(shape=(num_edges, 2))
        self.rlengths = np.ndarray(shape=(num_edges,))
        self.subdivision = 0
        for i in range(num_edges):
            e = edges[i]
            idx = e['Indexes']
            self.edges[i] = [int(idx[0]),int(idx[1])] 
            self.subdivision = e['Subdivision']

        # Init linkage
        self.linkage = elastic_rods.RodLinkage(self.vertices, self.edges, self.subdivision, initConsistentAngle = consistent_angle, 
                                               rod_interleaving_type = interleaving,input_joint_normals = self.normals)

        # Material Data (after initialization)
        # CrossSectionType { rectangle=0, ellipse=1, I=2, L=3, cross=4 };
        # StiffAxis { tangent=0, normal=1 };
        materials = data['MaterialData']
        num_materials = len(materials)
        mat = []
        for i in range(num_materials):
            m = materials[i]

            section = 'rectangle'
            type = m['CrossSectionType']
            if(type==1):
                section = 'ellipse'
            elif(type==2):
                section = 'I'
            elif(type==3):
                section = 'L'
            elif(type==4):
                section = '+'

            axis = elastic_rods.StiffAxis.D1
            if(m['Orientation'] == 1):
                axis = elastic_rods.StiffAxis.D2

            mat.append(elastic_rods.RodMaterial(section, m['E'], m['PoisonsRatio'], [m['Width'],m['Height']], stiffAxis=axis))
        
        if(num_materials==1):
            self.linkage.setMaterial(mat[0])
        elif(num_materials>1):
            self.linkage.setJointMaterials(mat)
        
        # Support data
        anchors = data['Supports']
        num_anchors = len(anchors)
        anchors = []
        if num_anchors > 0:
            for i in range(num_anchors):
                a = anchors[i]
                idx = self.linkage.dofOffsetForJoint(a['Indexes'][0])
                idx_dof = []
                dof = a['LockedDOF']
                for offset in dof:
                    idx_dof.append(idx + offset)
                anchors.extend(idx_dof)
        else:
            driver  = self.linkage.centralJoint()
            idx = self.linkage.dofOffsetForJoint(driver)
            idx_dof = list(range(idx, idx + 6)) # fix rigid motion for a single joint
            anchors.extend(idx_dof)
        self.supports = anchors
        
        # Force data
        forces = data['Forces']
        num_forces = len(forces)
        forces = []
        if num_forces > 0:
            forces = np.linspace(0,0,len(self.linkage.gradient()))
            for i in range(num_forces):
                f = forces[i]
                idx = self.linkage.dofOffsetForJoint(f['Indexes'][0])
                vec = f['Vector']
                for j in range(3):
                    forces[idx+j] = vec[j]
        self.forces = forces
    
    
def clip_alpha(a):
    if a > 1:
        return 1
    if a < 0:
        return 0
    return a    
