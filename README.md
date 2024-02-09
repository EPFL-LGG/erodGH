ElasticRod Pluging for Grasshopper
===========

# Getting Started

## Building C++ Libraries
The C++ code relies on `boost` and `cholmod/umfpack`. Some parts of the code (actuator sparsification, design optimization) also depend on the commercial optimization package `knitro`; these will be omitted from the build if knitro is not found.

### macOS
You can install all the mandatory dependencies on macOS with [MacPorts](https://www.macports.org).

```bash
# Build/version control tools, C++ code dependencies
sudo port install cmake boost suitesparse ninja
# Dependencies for jupyterlab/notebooks
sudo port install py37-pip
sudo port select --set python python37
sudo port select --set pip3 pip37
sudo port install npm6
```

## Obtaining and Building

Clone this repository *recursively* so that its submodules are also downloaded:

```bash
git clone --recursive git@github.com:EPFL-LGG/erodGH.git
```

Build the C++ code and its Python bindings using `cmake` and your favorite
build system. For example, with [`ninja`](https://ninja-build.org):

```bash
cd erodGH
mkdir build && cd build
cmake .. -GNinja
ninja
```

## Running the Jupyter Notebooks
The preferred way to interact with the inflatables code is in a Jupyter notebook,
using the Python bindings.
We recommend that you install the Python dependencies and JupyterLab itself in a
virtual environment (e.g., with [venv](https://docs.python.org/3/library/venv.html)).

```bash
pip3 install wheel # Needed if installing in a virtual environment
pip3 install jupyterlab ipykernel==5.5.5 # Use a slightly older version of ipykernel to avoid cluttering notebook with stdout content.
# If necessary, follow the instructions in the warnings to add the Python user
# bin directory (containing the 'jupyter' binary) to your PATH...

git clone https://github.com/jpanetta/pythreejs
cd pythreejs
pip3 install -e .
cd js
jupyter labextension install .

pip3 install matplotlib scipy
```

## Building C# Plugin
The C# plugin is compatible only with Rhino 7 and 8 for Mac with Intel processors, as well as Rhino 8 with ARM processors.

### Visual Studio for Mac (if not installed)
Download and install [Visual Studio 2022](https://visualstudio.microsoft.com/vs/mac/)

Check if `Mono` is installed with Visual Studio.
Open Visual Studio for Mac and click on 'Visual Studio' in the top menu bar. 
Select 'About Visual Studio' from the dropdown menu and, in the dialog that opens, you should see version information and installed components.
If `Mono` is not listed, download and install [Mono](https://www.mono-project.com/download/stable/)

Download the latest [RhinoVisualStudioExtensions](https://github.com/mcneel/RhinoCommonXamarinStudioAddin/releases).
Launch Visual Studio => Navigate to Visual Studio>Extensions.. => Click "Install from file" => Select the .mpack file.

Quit and Restart Visual Studio => Navigate to Extensions Studio>Add-ins..>Installed tab => Verify that RhinoCommon Plugin Support exists under the Debugging category.

## Building 
Open .sln project from `erodGH/src/erod/` in Visual Studio and build it. This will copy all the .dll and .gha files (plugin files) in `erodGH/bin/erod`. The bin folder already contains the C++ library (.dylib file).

If the 'bin' folder is not referenced in Grasshopper, open Rhino, enter `GrasshopperDeveloperSettings` into the Command console, and add the path to the 'bin' folder to the Library Folders. Restart Rhino