# Running an external process (exe) with Azure Functions

This sample shows running an exe from a functions project. It shows concepts such as redirecting output, and running the executable from a temporary location.

## Try it out

Run the Azure Functions app in the `ExternalProcessFunctions` folder on a Windows system. Make a request to `http://localhost:7071/api/RunExternalProcess?exitcode=2. You can use the the optional exitcode query string parameter to control the exit code that the external process returns when it runs. The value is passed to the external process as a command line argument. Setting it to 0 will simulate success, any other int will simulate failure, and a non-int value may crash the exe.

## Building and including the external app

This sample includes an app built using Visual C++. You can use anything to build your executable, but be careful with its dependencies. Ideally all dependencies should be bundled with your exe. Try not to rely on particular system components or runtimes being pre-installed on the host. If you do have dependencies, try to first [log in to Kudu](https://github.com/projectkudu/kudu/wiki/Accessing-the-kudu-service) on your Azure Function app in Azure, and check that those requirements can be met. You have very limited access to installing things, and performance on cold starts may suffer greatly if you do that. 

To include the exe in the bundle, I've set up the release configuration to output the exe to the `DependencyExe` folder in the functions project. These are included in my repo, you don't need to build them yourself. 

The `Copy to Output Directory` option in the file properties in Visual Studio is set to `Copy if newer`. This will ensure that the exe is copied to the binaries directory when our Functions app is built and packaged.

## CPU Architecture

Because c++ apps are compiled for specific CPU architectures, I've provided both x64 and x86 versions, but only for Windows. At runtime, the Functions app will detect whether the operating system is 32bit or 64bit, and run the appropriate exe. You can change the architecture of the Functions app in the Function app configuration in the Azure portal, if you've only got one particular exe. If you plan to deploy your app to Functions for Linux, you'll need to build appropriate Linux-compatible exe's. 

## Running in a temporary location

When deploying a Function app using the recommended RunFromPackage option, the file system where the app is running is read-only. This can prevent your executable from running in Azure, even though it works fine on your local machine. To fix this, the sample copies the contents of the dependency directory to a temporary location provided by the system. The executable is run from that location, which is also set as the working directory. You should also use that location for any files that the executable needs to read or write. This sample creates a subdirectory in the temporary path for each instantiation to prevent filesystem clashes, but you may not need to do that. 

## Redirecting output

Output is redirected so that it may be read using stream readers and reported in the ProcessResult. Depending on how the app performs stream io, this may prevent the app from working - it appears to hang when standard output is read while debugging. Set the `redirectIO` parameter in the `ExternalProcessManager` to false to disable it.
