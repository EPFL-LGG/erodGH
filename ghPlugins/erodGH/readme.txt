Installing ErodGH for Rhino 7 (MAC intel):

1. Clone recursively the branch grasshopper from elastic_rods git repo

2. build 

3. Install Visual Studio 2019 (2022 not yet tested) from
https://visualstudio.microsoft.com/vs/mac/

4. Check if Mono Framework is installed with Visual Studio. If not download and install Mono from:
https://www.mono-project.com/download/stable/

5. Download the latest Rhino/Grasshopper extension (.mpack file) for Visual Studio Mac from:
https://github.com/mcneel/RhinoCommonXamarinStudioAddin/releases

6. Launch Visual Studio for Mac => Navigate to Visual Studio>Extensions.. => Click "Install from file" => Select the .mpack file.

7. Quit and Restart Visual Studio for Mac => Navigate to Extensions Studio>Add-ins..>Installed tab => Verify that RhinoCommon Plugin Support exists under the Debugging category.

8. Open .sln project from elastic_rods/erodGH/ElasticRod/ in Visual studio Mac and Build it. This will copy all the .dll and .gha files (plugin files) in elastic_rods/erodGH/ElasticRod/bin. The bin folder already contains a .dylib file with all the wrapped functions from elastic_rods.

9. After compiling the plugin, open Rhino. Enter "GrasshopperDeveloperSettings" into the Command console and the bin folder into the Library Folders.

10. Restart Rhino


Installing ErodGH for Rhino 7 (MAC arm64):
IMPORTANT: Building the plugin for Rhino 7 using an arm64 processor requires some special steps. 
Rhino 7 is an intel based application that can run on an Apple silicon machine via Rosetta 2. Because of this, the c++ library (liberod.dylib) used by the plugin needs to be compiled for an x86_64 architecture otherwise Rhino 7 will throw an error when loading the plugin.  
On the contrary, Rhino 8 is a universal application in which case the c++ libary can be compiled for an arm64 architecture. However, Rhino 8 is still under development and unstable (not recommended). 
To compile the c++ library, we need to install boost and suitesparse for x86-64 architecture using Homebrew for Intel apps.
The steps to follow are:
 
1. Clone recursively the branch grasshopper from elastic_rods git repo

2._ Install Homebrew for Intel apps. 
    - Create a directory named homebrew in Downloads
    - Download and extract Homebrew in ~/Downloads/homebrew directory
        curl -L https://github.com/Homebrew/brew/tarball/master | tar xz --strip 1 -C homebrew
    - Move the homebrew directory to /usr/local/homebrew (Homebrew for apple silicon machines is installed in /opt/homebrew/bin/brew)
        sudo mv homebrew /usr/local/homebrew
    - Add an alias and path in the ~/.zshrc file
        # If you come from bash you might have to change your $PATH.
        # need this for x86_64 brew
        export PATH=$HOME/bin:/usr/local/bin:$PATH
        # for intel x86_64 brew
        alias axbrew='arch -x86_64 /usr/local/homebrew/bin/brew'  
    - Install boost and suitesparse for x86_64 architecture
        axbrew install boost suitesparse glew libpng

3._ Set the correct paths to find boost and suitesparse before building
    - Create a new file to store the required environment variables
        touch ~/.rhino_elastic_rod_env
    - Copy and paste the following variables in .rhino_elastic_rod_env
        # Environment variables for compiling elastic rods for X86_64 on m1
        export SUITESPARSE_ROOT=/usr/local/homebrew/opt/suitesparse
        export SUITESPARSE_INC=/usr/local/homebrew/include/
        export CHOLMODDIR=/usr/local/homebrew/include/
        export Boost_INCLUDE_DIR=/usr/local/homebrew/include/boost
        export BOOST_ROOT=/usr/local/homebrew/opt/boost
        export GLEW_ROOT=/usr/local/homebrew/opt/glew
        export GLEW_INC=/usr/local/homebrew/include
        export PNG_ROOT=/usr/local/homebrew/opt/libpng
        export PNG_INC=/usr/local/homebrew/include
    - Before building, always update the PATH (.rhino_elastic_rod_env is not updated frequently).
        source ~/.rhino_elastic_rod_env

2. Go to the elastic_rods directory and build  

3. Install Visual Studio 2019 (2022 not yet tested) from
https://visualstudio.microsoft.com/vs/mac/

4. Check if Mono Framework is installed with Visual Studio. If not download and install Mono from:
https://www.mono-project.com/download/stable/

5. Download the latest Rhino/Grasshopper extension (.mpack file) for Visual Studio Mac from:
https://github.com/mcneel/RhinoCommonXamarinStudioAddin/releases

6. Launch Visual Studio for Mac => Navigate to Visual Studio>Extensions.. => Click "Install from file" => Select the .mpack file.

7. Quit and Restart Visual Studio for Mac => Navigate to Extensions Studio>Add-ins..>Installed tab => Verify that RhinoCommon Plugin Support exists under the Debugging category.

8. Open .sln project from elastic_rods/erodGH/ElasticRod/ in Visual studio Mac and Build it. This will copy all the .dll and .gha files (plugin files) in elastic_rods/erodGH/ElasticRod/bin. The bin folder already contains a .dylib file with all the wrapped functions from elastic_rods.

9. After compiling the plugin, open Rhino. Enter "GrasshopperDeveloperSettings" into the Command console and the bin folder into the Library Folders.

10. Restart Rhino


