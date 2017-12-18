# Engine-View
HoloLens application made with METool kit by DataMesh. 

**Modules Used**
```
Input 
Collaboration
UI and Block Menu Button
```

**Scenario**
- Users share a commone scene that features an Engine. 

- Each user can select a part of the engine and a line will be drawn from the user's position to the selected object's position. The lines will be then made available to other connected devices. 

- To see the lines, users can open their own Menu and select "Show Lines" to make line visibles and "Hide Lines" to hide them. 

- Users can also decide if they want to save the line they created for later use with "Save Lines" from the block menu. "Load Lines' can be used to make previously created lines appear. These two procedures uses a Json file. They are also subject to improvement as for now the Json file is device dependendent. 

**Note:**
- Before getting started, download [**METookit**](https://github.com/DataMesh-OpenSource/METoolkit "METoolkit Source"). 
- This project is created with the older version on METool kit, so make sure you're not using the METool kit 2017 version. 
- This folder is not a Unity project so you have to create an empty Unity project and then drag and drop all the elements above your project.

If you want to know more, visit [**DataMesh**](https://www.datamesh.com/ "DataMesh website").

### Get Started
Get started by integrating **METool kit** with your Unity project. This can be done in two different ways. 
1. Unzip the folder. Go to Unity, click **Open** and select the unzipped file. 
2. You can create a new project then drag and drop the METool kit folder in the **Project** section in Unity. 

To configure your project correctly, follow [**Configuration manual**](http://docs.datamesh.com/projects/me-live/en/latest/toolkit/toolkit-man-configure-your-project/ "Project Config"). 

Find **main** and have as you main scene. In the project panel, find **MEHoloEntrance** then drag and drop it in the Heirarchy. It is important that you have this in your scene as this manages all the modules of the METool kit: Cursors, Inputs and Collaboration in this scenario. Follow [**Integration manual**](http://docs.datamesh.com/projects/me-live/en/latest/toolkit/toolkit-man-integrated-METoolkit/ "Integration Manual") for a correct integration of the tool kit.  

### Build
- [**For HoloLens**](https://github.com/DataMesh-OpenSource/SolarSystemExplorer/blob/master/Docs/DiveDeeper/build-hololens-app.md "HoloLens build")

- [**For PC**](https://github.com/DataMesh-OpenSource/SolarSystemExplorer/blob/master/Docs/DiveDeeper/build-pc-app.md#build-pc-app "PC build"). Use the setting in this build, connect your Spectator view and run the application.

- [**For Surface**](https://github.com/DataMesh-OpenSource/SolarSystemExplorer/blob/master/Docs/DiveDeeper/build-surface-app.md "Surface build")

### NOTES
1. Make sure **MEConfigNetwork.ini** contains your own server ip. If you have MeshExpert Center, you can find it under **Service IP** in the Dash board. If not, you can use you **ipconfig** in you command line.
2. Check **Console** in Unity and see the value of **Delay**. If it is equals to zero even after executing the note above, try turning off your Firewall.
3. You can try to use this application with Spectatoe view. 

