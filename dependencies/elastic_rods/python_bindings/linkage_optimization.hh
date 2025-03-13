#ifndef LINKAGEOPTIMIZATIONBINDING_HH
#define LINKAGEOPTIMIZATIONBINDING_HH

#include <MeshFEM/Geometry.hh>

#include "../SurfaceAttractedLinkage.hh"
#include "../DesignOptimizationTerms.hh"
#include "../TargetSurfaceFitterMesh.hh"

#include "../RegularizationTerms.hh"
#include "../LinkageOptimization.hh"
#include "../WeavingOptimization.hh"
#include "../XShellOptimization.hh"

#include <pybind11/pybind11.h>
#include <pybind11/stl.h>
#include <pybind11/eigen.h>
#include <pybind11/iostream.h>
#include <pybind11/functional.h>
#include <sstream>
namespace py = pybind11;

template<typename Real_>
using SAL_T = SurfaceAttractedLinkage_T<Real_>;
using SAL   = SAL_T<Real>;

template<typename T>
std::string hexString(T val) {
    std::ostringstream ss;
    ss << std::hex << val;
    return ss.str();
}

template<template<typename> class Object>
struct LinkageOptimizationTrampoline : public LinkageOptimization<Object> {
    // Inherit the constructors.
    using LinkageOptimization<Object>::LinkageOptimization;

    Eigen::VectorXd gradp_J(const Eigen::Ref<const Eigen::VectorXd> &params, OptEnergyType opt_eType = OptEnergyType::Full) override {
        PYBIND11_OVERRIDE_PURE(
            Eigen::VectorXd, // Return type.
            LinkageOptimization<Object>, // Parent class.
            gradp_J, // Name of the function in C++.
            params, opt_eType// Arguments.
        );
    }

    Eigen::VectorXd apply_hess(const Eigen::Ref<const Eigen::VectorXd> &params, const Eigen::Ref<const Eigen::VectorXd> &delta_p, Real coeff_J, Real coeff_c = 0.0, Real coeff_angle_constraint = 0.0, OptEnergyType opt_eType = OptEnergyType::Full) override {
        PYBIND11_OVERRIDE_PURE(
            Eigen::VectorXd, // Return type.
            LinkageOptimization<Object>, // Parent class.
            apply_hess, // Name of the function in C++.
            params, delta_p, coeff_J, coeff_c, coeff_angle_constraint, opt_eType// Arguments.
        );
    }
    void setLinkageInterleavingType(InterleavingType new_type) override {
        PYBIND11_OVERRIDE_PURE(
            void, // Return type.
            LinkageOptimization<Object>, // Parent class.
            setLinkageInterleavingType, // Name of the function in C++.
            new_type// Arguments.
        );
    }
    void commitLinesearchLinkage() override {
        PYBIND11_OVERRIDE_PURE(
            void, // Return type.
            LinkageOptimization<Object>, // Parent class.
            commitLinesearchLinkage, // Name of the function in C++.
            // No Arguments.
        );
    }
    void setEquilibriumOptions(const NewtonOptimizerOptions &eopts) override {
        PYBIND11_OVERRIDE_PURE(
            void, // Return type.
            LinkageOptimization<Object>, // Parent class.
            setEquilibriumOptions, // Name of the function in C++.
            eopts// Arguments.
        );
    }
    NewtonOptimizerOptions getEquilibriumOptions() const override {
        PYBIND11_OVERRIDE_PURE(
            NewtonOptimizerOptions, // Return type.
            LinkageOptimization<Object>, // Parent class.
            getEquilibriumOptions, // Name of the function in C++.
            // No Arguments.
        );
    }
    void setGamma(Real val) override {
        PYBIND11_OVERRIDE_PURE(
            void, // Return type.
            LinkageOptimization<Object>, // Parent class.
            setGamma, // Name of the function in C++.
            val// Arguments.
        );
    }
    Real getGamma() const override {
        PYBIND11_OVERRIDE_PURE(
            Real, // Return type.
            LinkageOptimization<Object>, // Parent class.
            getGamma, // Name of the function in C++.
            // No Arguments.
        );
    }

    void m_forceEquilibriumUpdate() override {
        PYBIND11_OVERRIDE_PURE(
            void, // Return type.
            LinkageOptimization<Object>, // Parent class.
            m_forceEquilibriumUpdate, // Name of the function in C++.
            // No Arguments.
        );
    }
    bool m_updateEquilibria(const Eigen::Ref<const Eigen::VectorXd> &params) override {
        PYBIND11_OVERRIDE_PURE(
            bool, // Return type.
            LinkageOptimization<Object>, // Parent class.
            m_updateEquilibria, // Name of the function in C++.
            params// Arguments.
        );
    }
    void m_updateClosestPoints() override {
        PYBIND11_OVERRIDE_PURE(
            void, // Return type.
            LinkageOptimization<Object>, // Parent class.
            m_updateClosestPoints, // Name of the function in C++.
            // No Arguments.
        );
    }
    bool m_updateAdjointState(const Eigen::Ref<const Eigen::VectorXd> &params, const OptEnergyType /*opt_eType=OptEnergyType::Full*/) override {
        PYBIND11_OVERRIDE_PURE(
            bool, // Return type.
            LinkageOptimization<Object>, // Parent class.
            m_updateAdjointState, // Name of the function in C++.
            params// Arguments.
        );
    }

};

template<template<typename> class Object>
void bindLinkageOptimization(py::module &m, const std::string &typestr) {
    using LO = LinkageOptimization<Object>;
    using LTO = LinkageOptimizationTrampoline<Object>;
    std::string pyclass_name = std::string("LinkageOptimization_") + typestr;
    py::class_<LO, LTO>(m, pyclass_name.c_str())
    .def(py::init<Object<Real> &, const NewtonOptimizerOptions &, Real, Real, Real>(), py::arg("baseLinkage"), py::arg("equilibrium_options") = NewtonOptimizerOptions(), py::arg("E0") = 1.0, py::arg("l0") = 1.0, py::arg("rl0") = 1.0)
    .def("newPt",          &LO::newPt, py::arg("params"))
    .def("params",         &LO::params)
    .def("J",              py::overload_cast<const Eigen::Ref<const Eigen::VectorXd> &, OptEnergyType>(&LO::J),              py::arg("params"), py::arg("energyType") = OptEnergyType::Full)
    .def("J_target",       py::overload_cast<const Eigen::Ref<const Eigen::VectorXd> &>(&LO::J_target),       py::arg("params"))
    .def("J_regularization", &LO::J_regularization)
    .def("J_smoothing",      &LO::J_smoothing)
    .def("apply_hess_J",   &LO::apply_hess_J, py::arg("params"), py::arg("delta_p"), py::arg("energyType") = OptEnergyType::Full)
    .def("apply_hess_c",   &LO::apply_hess_J, py::arg("params"), py::arg("delta_p"), py::arg("energyType") = OptEnergyType::Full)
    .def("numParams",       &LO::numParams)
    .def("get_l0",          &LO::get_l0)
    .def("get_rl0",         &LO::get_rl0)
    .def("get_E0",          &LO::get_E0)
    .def("invalidateAdjointState",     &LO::invalidateAdjointState)
    .def("restKappaSmoothness", &LO::restKappaSmoothness)
    .def_readwrite("prediction_order", &LO::prediction_order)
    .def_property("beta",  &LO::getBeta , &LO::setBeta )
    .def_property("gamma", &LO::getGamma, &LO::setGamma)
    .def_property("rl_regularization_weight", &LO::getRestLengthMinimizationWeight, &LO::setRestLengthMinimizationWeight)
    .def_property("smoothing_weight",         &LO::getRestKappaSmoothingWeight,     &LO::setRestKappaSmoothingWeight)
    .def_property("contact_force_weight",     &LO::getContactForceWeight,           &LO::setContactForceWeight)
    .def_readonly("target_surface_fitter",    &LO::target_surface_fitter)
    .def("getTargetSurfaceVertices",          &LO::getTargetSurfaceVertices)
    .def("getTargetSurfaceFaces",             &LO::getTargetSurfaceFaces)
    .def("getTargetSurfaceNormals",           &LO::getTargetSurfaceNormals)
    .def_readwrite("objective", &LO::objective, py::return_value_policy::reference)
    ;
}

template<template<typename> class Object>
void bindXShellOptimization(py::module &m, const std::string &typestr) {
    using XO = XShellOptimization<Object>;
    using LO = LinkageOptimization<Object>;
    std::string pyclass_name = std::string("XShellOptimization_") + typestr;
    py::class_<XO, LO>(m, pyclass_name.c_str())
    .def(py::init<Object<Real> &, Object<Real> &, const NewtonOptimizerOptions &, Real, int, bool, bool, bool>(), py::arg("flat_linkage"), py::arg("deployed_linkage"), py::arg("equilibrium_options") = NewtonOptimizerOptions(), py::arg("minAngleConstraint") = 0, py::arg("pinJoint") = -1, 
         py::arg("allowFlatActuation") = true, py::arg("optimizeTargetAngle") = true, py::arg("fixDeployedVars") = true)
    .def("J",                      py::overload_cast<const Eigen::Ref<const Eigen::VectorXd> &, OptEnergyType>(&XO::J),              py::arg("params"), py::arg("energyType") = OptEnergyType::Full)
    .def("J_target",               py::overload_cast<const Eigen::Ref<const Eigen::VectorXd> &>(&XO::J_target),                      py::arg("params"))
    .def("c",                      py::overload_cast<const Eigen::Ref<const Eigen::VectorXd> &>(&XO::c),                             py::arg("params"))
    .def("angle_constraint",       py::overload_cast<const Eigen::Ref<const Eigen::VectorXd> &>(&XO::angle_constraint),              py::arg("params"))
    .def("gradp_J",                py::overload_cast<const Eigen::Ref<const Eigen::VectorXd> &, OptEnergyType>(&XO::gradp_J),        py::arg("params"), py::arg("energyType") = OptEnergyType::Full)
    .def("gradp_J_target",         py::overload_cast<const Eigen::Ref<const Eigen::VectorXd> &>(&XO::gradp_J_target),                py::arg("params"))
    .def("gradp_angle_constraint", py::overload_cast<const Eigen::Ref<const Eigen::VectorXd> &>(&XO::gradp_angle_constraint),        py::arg("params"))
    .def("gradp_c",                &XO::gradp_c,        py::arg("params"))
    .def("get_w_x",                &XO::get_w_x)
    .def("get_w_lambda",           &XO::get_w_lambda)
    .def("get_y",                  &XO::get_y)

    .def("get_s_x",            &XO::get_s_x)
    .def("get_delta_x3d",      &XO::get_delta_x3d)
    .def("get_delta_x2d",      &XO::get_delta_x2d)
    .def("get_delta_w_x",      &XO::get_delta_w_x)
    .def("get_delta_w_lambda", &XO::get_delta_w_lambda)
    .def("get_delta_s_x",      &XO::get_delta_s_x)

    .def("pushforward",                  &XO::pushforward, py::arg("params"), py::arg("delta_p"))
    .def("apply_hess",                   &XO::apply_hess, py::arg("params"), py::arg("delta_p"), py::arg("coeff_J") = 1., py::arg("coeff_c") = 0., py::arg("coeff_angle_constraint") = 0., py::arg("energyType") = OptEnergyType::Full)
    .def("setTargetSurface",             &XO::setTargetSurface,           py::arg("V"), py::arg("F"))
    .def("loadTargetSurface",            &XO::loadTargetSurface,          py::arg("path"))
    .def("saveTargetSurface",            &XO::saveTargetSurface,          py::arg("path"))
    .def("scaleJointWeights",            &XO::scaleJointWeights, py::arg("jointPosWeight"), py::arg("featureMultiplier") = 1.0, py::arg("additional_feature_pts") = std::vector<size_t>())
    .def("numFullParams",                &XO::numFullParams)
    .def("getTargetAngle",               &XO::getTargetAngle)
    .def("setTargetAngle",               &XO::setTargetAngle, py::arg("alpha_t"))
    .def("getEquilibriumOptions",         &XO::getEquilibriumOptions)
    .def("getDeploymentOptions",          &XO::getDeploymentOptions)
    .def("getFixedFlatVars",              &XO::getFixedFlatVars)
    .def("getFixedDeployedVars",          &XO::getFixedDeployedVars)
    .def("getLinesearchBaseLinkage",      &XO::getLinesearchBaseLinkage,     py::return_value_policy::reference)
    .def("getLinesearchDeployedLinkage",  &XO::getLinesearchDeployedLinkage, py::return_value_policy::reference)
    .def("getLinesearchDesignParameters", &XO::getLinesearchDesignParameters)
    .def("getFullDesignParameters",       &XO::getFullDesignParameters)
    .def("getOptimizeTargetAngle",        &XO::getOptimizeTargetAngle)
    .def("setOptimizeTargetAngle",        &XO::setOptimizeTargetAngle,    py::arg("optimize"))
    .def("setHoldClosestPointsFixed",     &XO::setHoldClosestPointsFixed, py::arg("hold"))
    .def("getTargetJointsPosition",       &XO::getTargetJointsPosition)
    .def("setTargetJointsPosition",       &XO::setTargetJointsPosition, py::arg("jointPosition"))
    .def("getEpsMinAngleConstraint",      &XO::getEpsMinAngleConstraint)
    .def("constructTargetSurface", &XO::constructTargetSurface, py::arg("loop_subdivisions"), py::arg("num_extension_layers"), py::arg("scale_factors"))
    .def("XShellOptimize", &XO::optimize, py::arg("alg"), py::arg("num_steps"), py::arg("trust_region_scale"), py::arg("optimality_tol"), py::arg("update_viewer"), py::arg("minRestLen") = -1, py::arg("applyAngleConstraint") = true, py::arg("applyFlatnessConstraint") = true)
    ;
}

template<template<typename> class Object>
void bindWeavingOptimization(py::module &m, const std::string &typestr) {
    using WO = WeavingOptimization<Object>;
    using LO = LinkageOptimization<Object>;
    std::string pyclass_name = std::string("WeavingOptimization_") + typestr;
    py::class_<WO, LO>(m, pyclass_name.c_str())
    .def(py::init<Object<Real> &, const std::string, bool, const NewtonOptimizerOptions &, int, bool, const std::vector<size_t>>(), py::arg("weaver"), py::arg("input_surface_path"), py::arg("useCenterline"), py::arg("equilibrium_options") = NewtonOptimizerOptions(), py::arg("pinJoint") = -1, py::arg("useFixedJoint") = true, py::arg("fixedVars") = std::vector<size_t>())
    .def("gradp_J",        py::overload_cast<const Eigen::Ref<const Eigen::VectorXd> &, OptEnergyType>(&WO::gradp_J),        py::arg("params"), py::arg("energyType") = OptEnergyType::Full)

    .def("WeavingOptimize", &WO::optimize, py::arg("alg"), py::arg("num_steps"), py::arg("trust_region_scale"), py::arg("optimality_tol"), py::arg("update_viewer"), py::arg("minRestLen") = -1, py::arg("applyAngleConstraint") = false, py::arg("applyFlatnessConstraint") = false)
    .def("get_w_x",         &WO::get_w_x)

    .def("get_delta_x",   &WO::get_delta_x)
    .def("get_delta_w_x", &WO::get_delta_w_x)

    .def("getLinesearchWeaverLinkage", &WO::getLinesearchWeaverLinkage, py::return_value_policy::reference)
    .def("setWeavingOptimization",     &WO::setWeavingOptimization)
    .def("setUseCenterline",           &WO::setUseCenterline, py::arg("useCenterline"), py::arg("jointPosWeight"), py::arg("jointPosValence2Multiplier"))

    .def("loadTargetSurface",          &WO::loadTargetSurface,          py::arg("path"))
    .def("set_target_joint_position",  &WO::set_target_joint_position,  py::arg("input_target_joint_pos"))
    .def("set_holdClosestPointsFixed", &WO::set_holdClosestPointsFixed, py::arg("holdClosestPointsFixed"))
    .def("scaleJointWeights",          &WO::scaleJointWeights, py::arg("jointPosWeight"), py::arg("featureMultiplier") = 1.0, py::arg("additional_feature_pts") = std::vector<size_t>())
    .def("get_holdClosestPointsFixed", &WO::get_holdClosestPointsFixed)
    .def("setLinkageAttractionWeight", &WO::setLinkageAttractionWeight, py::arg("attraction_weight"))
    .def("constructTargetSurface", &WO::constructTargetSurface, py::arg("loop_subdivisions"), py::arg("scale_factors"))
    .def("equilibriumOptions",  &WO::equilibriumOptions,  py::return_value_policy::reference)
    ;
}

// template<template<typename> class Object>
// void bindTerms(py::module &m, const std::string &typestr) {
//     using OEType = OptEnergyType;
//     using TSF  = TargetSurfaceFitter;
//     using RT   = RegularizationTerm<Object>;
//     using RCS  = RestCurvatureSmoothing<Object>;
//     using RLM  = RestLengthMinimization<Object>;
//     using DOT  = DesignOptimizationTerm<Object>;
//     using DOOT = DesignOptimizationObjectiveTerm<Object>;
//     using EEO  = ElasticEnergyObjective<Object>;
//     using CFO  = ContactForceObjective<Object>;
//     using TFO  = TargetFittingDOOT<Object>;
//     using RCSD = RegularizationTermDOOWrapper<Object, RestCurvatureSmoothing>;
//     using RLMD = RegularizationTermDOOWrapper<Object, RestLengthMinimization>;
//     using DOO  = DesignOptimizationObjective<Object, OEType>;
//     using TR   = DOO::TermRecord;
//     using TPtr = DOO::TermPtr;

//     std::string pyclass_name_TSF = std::string("TargetSurfaceFitter_") + typestr;
//     py::class_<TSF>(m, pyclass_name_TSF.c_str())
//         .def(py::init<>())
//         .def("loadTargetSurface", &TSF::loadTargetSurface, py::arg("linkage"), py::arg("surf_path"))
//         .def("objective",       &TSF::objective,             py::arg("linkage"))
//         .def("gradient",        &TSF::gradient,              py::arg("linkage"))
//         .def("numSamplePoints", &TSF::numSamplePoints<Real>, py::arg("linkage"))

//         .def("setTargetJointPosVsTargetSurfaceTradeoff", &TSF::setTargetJointPosVsTargetSurfaceTradeoff<Real>, py::arg("linkage"), py::arg("jointPosWeight"), py::arg("valence2Multiplier") = 1.0)
//         .def("scaleJointWeights", &TSF::scaleJointWeights<Real>,  py::arg("linkage"), py::arg("jointPosWeight"), py::arg("valence2Multiplier") = 1.0, py::arg("additional_feature_pts") = std::vector<size_t>())
//         .def_readwrite("holdClosestPointsFixed", &TSF::holdClosestPointsFixed)

//         .def_readonly("W_diag_joint_pos",                      &TSF::W_diag_joint_pos)
//         .def_readonly("Wsurf_diag_linkage_sample_pos",         &TSF::Wsurf_diag_linkage_sample_pos)
//         .def_readonly("joint_pos_tgt",                         &TSF::joint_pos_tgt)

//         .def_property_readonly("V", [](const TSF &tsf) { return tsf.getV(); })
//         .def_property_readonly("F", [](const TSF &tsf) { return tsf.getF(); })
//         .def_property_readonly("N", [](const TSF &tsf) { return tsf.getN(); })
//         .def_readonly("linkage_closest_surf_pts",              &TSF::linkage_closest_surf_pts)
//         .def_readonly("linkage_closest_surf_pt_sensitivities", &TSF::linkage_closest_surf_pt_sensitivities)
//         .def_readonly("linkage_closest_surf_tris",             &TSF::linkage_closest_surf_tris)
//         .def_readonly("holdClosestPointsFixed",                &TSF::holdClosestPointsFixed)
//         ;

//     std::string pyclass_name_RT = std::string("RegularizationTerm_") + typestr;
//     py::class_<RT, std::shared_ptr<RT>>(m, pyclass_name_RT.c_str())
//         .def("energy", &RT::energy)
//         .def_readwrite("weight", &RT::weight)
//         ;

//     std::string pyclass_name_RCS = std::string("RestCurvatureSmoothing_") + typestr;
//     py::class_<RCS, RT, std::shared_ptr<RCS>>(m, pyclass_name_RCS.c_str())
//         .def(py::init<const Object &>(), py::arg("linkage")) //Might be wrong
//         ;

//     std::string pyclass_name_RLM = std::string("RestLengthMinimization_") + typestr;
//     py::class_<RLM, RT, std::shared_ptr<RLM>>(m, pyclass_name_RLM.c_str())
//         .def(py::init<const Object &>(), py::arg("linkage")) //Might be wrong
//         ;

//     std::string pyclass_name_DOT = std::string("DesignOptimizationTerm_") + typestr;
//     py::class_<DOT, std::shared_ptr<DOT>>(m, pyclass_name_DOT.c_str())
//         .def("value",  &DOT::value)
//         .def("update", &DOT::update)
//         .def("grad"  , &DOT::grad  )
//         .def("grad_x", &DOT::grad_x)
//         .def("grad_p", &DOT::grad_p)
//         .def("computeGrad",      &DOT::computeGrad)
//         .def("computeDeltaGrad", &DOT::computeDeltaGrad, py::arg("delta_xp"))
//         ;

//     std::string pyclass_name_DOOT = std::string("DesignOptimizationObjectiveTerm_") + typestr;
//     py::class_<DOOT, DOT, std::shared_ptr<DOOT>>(m, pyclass_name_DOOT.c_str())
//         .def_readwrite("weight", &DOOT::weight)
//         ;

//     std::string pyclass_name_EEO = std::string("ElasticEnergyObjective_") + typestr;
//     py::class_<EEO, DOOT, std::shared_ptr<EEO>>(m, pyclass_name_EEO.c_str())
//         .def(py::init<const Object &>(), py::arg("linkage"))
//         .def_property("useEnvelopeTheorem", &EEO::useEnvelopeTheorem, &EEO::setUseEnvelopeTheorem)
//         ;

//     std::string pyclass_name_CFO = std::string("ContactForceObjective_") + typestr;
//     py::class_<CFO, DOOT, std::shared_ptr<CFO>>(m, pyclass_name_CFO.c_str())
//         .def(py::init<const Object &>(), py::arg("linkage"))
//         .def_property(             "normalWeight", &CFO::getNormalWeight,              &CFO::setNormalWeight)
//         .def_property(         "tangentialWeight", &CFO::getTangentialWeight,          &CFO::setTangentialWeight)
//         .def_property(             "torqueWeight", &CFO::getTorqueWeight,          &CFO::setTorqueWeight)
//         .def_property(     "boundaryNormalWeight", &CFO::getBoundaryNormalWeight,      &CFO::setBoundaryNormalWeight)
//         .def_property( "boundaryTangentialWeight", &CFO::getBoundaryTangentialWeight,  &CFO::setBoundaryTangentialWeight)
//         .def_property(     "boundaryTorqueWeight", &CFO::getBoundaryTorqueWeight,  &CFO::setBoundaryTorqueWeight)
//         .def_property("normalActivationThreshold", &CFO::getNormalActivationThreshold, &CFO::setNormalActivationThreshold)
//         .def("jointForces", [](const CFO &cfo) { return cfo.jointForces(); })
//         ;

//     std::string pyclass_name_TFO = std::string("TargetFittingDOOT_") + typestr;
//     py::class_<TFO, DOOT, std::shared_ptr<TFO>>(m, pyclass_name_TFO.c_str())
//         .def(py::init<const Object &, TargetSurfaceFitter &>(), py::arg("linkage"), py::arg("targetSurfaceFitter"))
//         ;

//     std::string pyclass_name_RCSD = std::string("RestCurvatureSmoothingDOOT_") + typestr;
//     py::class_<RCSD, DOT, std::shared_ptr<RCSD>>(m, pyclass_name_RCSD.c_str())
//         .def(py::init<const Object &>(),          py::arg("linkage"))
//         .def(py::init<std::shared_ptr<RCS>>(), py::arg("restCurvatureRegTerm"))
//         .def_property("weight", [](const RCSD &r) { return r.weight; }, [](RCSD &r, Real w) { r.weight = w; }) // Needed since we inherit from DOT instead of DOOT (to share weight with RestCurvatureSmoothing)
//         ;

//     std::string pyclass_name_RLMD = std::string("RestLengthMinimizationDOOT_") + typestr;
//     py::class_<RLMD, DOT, std::shared_ptr<RLMD>>(m, pyclass_name_RLMD.c_str())
//         .def(py::init<const Object &>(),          py::arg("linkage"))
//         .def(py::init<std::shared_ptr<RLM>>(), py::arg("restLengthMinimizationTerm"))
//         .def_property("weight", [](const RLMD &r) { return r.weight; }, [](RLMD &r, Real w) { r.weight = w; }) // Needed since we inherit from DOT instead of DOOT (to share weight with RestCurvatureSmoothing)
//         ;

//     std::string pyclass_name_DOO = std::string("DesignOptimizationObjective_") + typestr;
//     py::class_<DOO> doo(m, pyclass_name_DOO.c_str())

//     std::string pyclass_name_TR = std::string("DesignOptimizationObjectiveTermRecord_") + typestr;
//     py::class_<TR>(doo, pyclass_name_TR.c_str())
//     .def(py::init<const std::string &, OEType, std::shared_ptr<DOT>>(),  py::arg("name"), py::arg("type"), py::arg("term"))
//     .def_readwrite("name", &TR::name)
//     .def_readwrite("type", &TR::type)
//     .def_readwrite("term", &TR::term)
//     .def("__repr__", [](const TR *trp) {
//             const auto &tr = *trp;
//             return "TermRecord " + tr.name + " at " + hexString(trp) + " with weight " + std::to_string(tr.term->getWeight()) + " and value " + std::to_string(tr.term->unweightedValue());
//     })
//     ;

//     doo.def(py::init<>())
//     .def("update",         &DOO::update)
//     .def("grad",           &DOO::grad, py::arg("type") = OEType::Full)
//     .def("values",         &DOO::values)
//     .def("weightedValues", &DOO::weightedValues)
//     .def("value",  py::overload_cast<OEType>(&DOO::value, py::const_), py::arg("type") = OEType::Full)
//     .def("computeGrad",     &DOO::computeGrad, py::arg("type") = OEType::Full)
//     .def("computeDeltaGrad", &DOO::computeDeltaGrad, py::arg("delta_xp"), py::arg("type") = OEType::Full)
//     .def_readwrite("terms",  &DOO::terms, py::return_value_policy::reference)
//     .def("add", py::overload_cast<const std::string &, OEType, TPtr      >(&DOO::add),  py::arg("name"), py::arg("type"), py::arg("term"))
//     .def("add", py::overload_cast<const std::string &, OEType, TPtr, Real>(&DOO::add),  py::arg("name"), py::arg("type"), py::arg("term"), py::arg("weight"))
//     // More convenient interface for adding multiple terms at once
//     .def("add", [](DOO &o, const std::list<std::tuple<std::string, OEType, TPtr>> &terms) {
//                 for (const auto &t : terms)
//                     o.add(std::get<0>(t), std::get<1>(t), std::get<2>(t));
//             })
//     ;
// }
#endif /* end of include guard: LINKAGEOPTIMIZATIONBINDING_HH */
