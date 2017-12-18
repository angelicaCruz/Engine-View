using System.Collections;
using UnityEngine;
using DataMesh.AR.Interactive;
using DataMesh.AR.Network;
using MEHoloClient.Entities;
using DataMesh.AR;
using System.Collections.Generic;
using DataMesh.AR.Utility;
using MEHoloClient.Proto;
using System;
using Newtonsoft.Json;
using System.IO;
using DataMesh.AR.UI;
//using System.Runtime.Serialization.Formatters.Binary;

public class MainApp : MonoBehaviour, IMessageHandler
{
    
    //modules
    private MultiInputManager inputManager;
    private CollaborationManager cm;
    private UIManager uiManager;
    private BlockMenu menu;

    //message variables
    private ShowObject showObject;
    private SceneObject roomData;

    //variables
    public GameObject mainCamera;
    private LineRenderer lineRenderer;
    private float lineDrawSpeed = 10f;
    public List<GameObject> objList; //the lsit of objects that will have the line renderer component
    private bool state; //for coroutine drawing
    private bool trash = false; //if true, takes the focused game object and removes the line renderer component
    private float counter = 0;
    private float dist; //distance between the user and the focused object;
    private GameObject focused;
    [HideInInspector]
    public List<Information> objInformation; //array of struct to be serialized
    [HideInInspector]
    //buttons
    public GameObject menuButton;

    // Use this for initialization
    void Start () {
        StartCoroutine(WaitForInit());
        lineRenderer = mainCamera.GetComponent<LineRenderer>();
        lineRenderer.SetPosition(0, mainCamera.transform.position);
        lineRenderer.SetWidth(.01f, .01f);
        lineRenderer.enabled = false;

        Debug.Log(Application.persistentDataPath);
    }

    /// <summary>
    /// used for initialization of variable and modules
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitForInit()
    {
        MEHoloEntrance entrance = MEHoloEntrance.Instance;

        while (!entrance.HasInit)
        {
            yield return null;
        }
        //instantiate modules
        inputManager = MultiInputManager.Instance;
        cm = CollaborationManager.Instance;
        uiManager = UIManager.Instance;

        // Todo: Begin your logic       
        inputManager.cbTap += OnTap;
        cm.AddMessageHandler(this);
        cm.cbEnterRoom = cbEnterRoom;

        string showId = "showId001";
        string obj_type = "name";

        string showId2 = "showId002";

        //creation of a message entry
        MsgEntry msg = new MsgEntry();
        msg.ShowId = showId;
        ObjectInfo info = new ObjectInfo();
        info.ObjType = obj_type;
        msg.Info = info;
        //add position of the main camera
        msg.Pr.Add(mainCamera.transform.position.x);
        msg.Pr.Add(mainCamera.transform.position.y);
        msg.Pr.Add(mainCamera.transform.position.z);
        //add three other elements to Pr to 
        //reserve position for the focused object
        msg.Pr.Add(0);
        msg.Pr.Add(0);
        msg.Pr.Add(0);
        showObject = new ShowObject(msg);
        roomData = new SceneObject();
        roomData.ShowObjectDic.Add(showObject.ShowId,showObject);

        cm.roomInitData = roomData;
        cm.TurnOn();

        //UI module        
        menu = uiManager.menuManager.GetMenu("Menu");
        menu.transform.position = mainCamera.transform.forward;
        uiManager.menuManager.cbMenuHide += OnMenuHide;
        //register action for the elements of the Menu
        menu.RegistButtonClick("button1.a", ShowAction);
        menu.RegistButtonClick("button1.b", HideAction);
        menu.RegistButtonClick("button2", DeleteLines);
        menu.RegistButtonClick("button2.b", DeleteJsonAction);
        menu.RegistButtonClick("button3", LoadAction);
        menu.RegistButtonClick("button4", SaveAction);


        inputManager.layerMask = LayerMask.GetMask("Engine") | LayerMask.GetMask("UI");
    }

    private void OnTap(int count)
    {
        //Debug.Log("I am back bitches!");
        if (inputManager.FocusedObject != null)
        {
            focused = inputManager.FocusedObject;
        }
        else
            focused = null;


        try
        {
            if (focused != null && !trash)
            {
                switch (focused.name)
                {
                    case "MenuButton":
                        //ShowHide(true);
                        menu.transform.position = mainCamera.transform.forward;
                        uiManager.menuManager.OpenMenu(menu);
                        inputManager.layerMask = LayerMask.GetMask("UI");
                        //menuButton.SetActive(false);
                        break;

                    case "ButtonContainer":
                        //does nothing beacuse the buttons have their own call back;
                        Debug.Log(focused.name);
                        break;

                    default:
                        dist = Vector3.Distance(mainCamera.transform.position, focused.transform.position);
                        lineRenderer.enabled = true;
                        lineRenderer.SetPosition(0, mainCamera.transform.position);
                        counter = 0;
                        state = true;
                        //CreateLineRenderer(focused);
                        //StartCoroutine(DrawLine());
                        CreateMessage();
                        inputManager.layerMask = LayerMask.GetMask("Engine") | LayerMask.GetMask("UI");
                        break;
                }
            }

            if (trash)
            {
                CreateTrashMessage();
            }
        }
        catch (Exception e)
        {
            Debug.Log("ERROR: " + e);
        }

       
    }

    private void OnClickButton2()
    {
        Debug.Log(focused.name);
    }

    /// <summary>
    /// callback for when the menu disappears
    /// </summary>
    private void OnMenuHide()
    {
        inputManager.layerMask = LayerMask.GetMask("Engine") | LayerMask.GetMask("UI");
    }

    /// <summary>
    /// creates the message to be sent to the server
    /// </summary>
    void CreateMessage()
    {
        ObjectInfo info = new ObjectInfo();
        info.ObjType = focused.name;
        //create message entry
        MsgEntry entry = new MsgEntry();
        entry.OpType = MsgEntry.Types.OP_TYPE.Upd;
        entry.ShowId = showObject.ShowId;
        //add the camera's current position
        entry.Pr.Add(mainCamera.transform.position.x);
        entry.Pr.Add(mainCamera.transform.position.y);
        entry.Pr.Add(mainCamera.transform.position.z);
        //add the focused object position
        entry.Pr.Add(focused.transform.position.x);
        entry.Pr.Add(focused.transform.position.y);
        entry.Pr.Add(focused.transform.position.z);
        //add the name of the focusedObject
        entry.Info = info;
        //create sync message
        SyncMsg msg = new SyncMsg();
        msg.MsgEntry.Add(entry);
        //send message
        cm.SendMessage(msg);
    }

    /// <summary>
    /// creates a message that is bound to be use only
    /// when trash button is selected
    /// </summary>
    void CreateTrashMessage()
    {
        ObjectInfo info = new ObjectInfo();
        info.ObjType = focused.name;
        //create message entry
        MsgEntry entry = new MsgEntry();
        entry.OpType = MsgEntry.Types.OP_TYPE.Upd;
        entry.ShowId = "showId002";
        //add the name of the focused Object
        entry.Info = info;
        //create Sync message
        SyncMsg msg = new SyncMsg();
        msg.MsgEntry.Add(entry);
        cm.SendMessage(msg);
    }

    /// <summary>
    /// Callback function of EnterRoom
    /// </summary>
    private void cbEnterRoom()
    {
        Debug.Log("Enter Room Sucessfully");
    }

    // <summary>
    /// iterates the list object with line renderers
    /// shows all line renderers
    /// </summary>
    private void ShowAction()
    {
        lineRenderer.enabled = false;
        foreach (GameObject go in objList)
        {
            go.GetComponent<LineRenderer>().enabled = true;
        }

        inputManager.layerMask = LayerMask.GetMask("Engine") | LayerMask.GetMask("UI");
    }

    // <summary>
    /// iterates the list object with line renderers
    /// hides all line renderers
    /// </summary>
    private void HideAction()
    {
        lineRenderer.enabled = false;
        foreach (GameObject go in objList)
        {
            go.GetComponent<LineRenderer>().enabled = false;
        }

        inputManager.layerMask = LayerMask.GetMask("Engine") | LayerMask.GetMask("UI");
    }

    /// <summary>
    /// take the focused object and
    /// adds the line renderer to that object. 
    /// if trash is true, removes the linerenderer from the object and
    /// remove the object from the list
    /// </summary>
    private void CreateLineRenderer(MsgEntry me)
    {
        Vector3 camera = new Vector3(me.Pr[0], me.Pr[1], me.Pr[2]);
        ObjectInfo info = me.Info;

        //populate serializable
        Information newInfo = new Information();
        newInfo.objectName = info.ObjType;
        newInfo.posx = me.Pr[0];
        newInfo.posy = me.Pr[1];
        newInfo.posz = me.Pr[2];
        objInformation.Add(newInfo);
        //end 

        //meObject obj = GameObject.Find(info.ObjType);
        try
        {
            GameObject obj = GameObject.Find(info.ObjType);
            //Debug.Log(obj);
            obj.AddComponent<LineRenderer>();
            LineRenderer lr = obj.GetComponent<LineRenderer>();
            lr.SetPosition(0, camera);
            lr.SetPosition(1, obj.transform.position);
            lr.material = lineRenderer.material;
            lr.SetWidth(.01f, .01f);
            lr.enabled = false;
            //add the object to the list og gameobjects
            objList.Add(obj);

        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    /// <summary>
    ///  callback for the button Delete Lines
    /// </summary>
    /// <param name="me"></param>
    private void DeleteLines()
    {       

        inputManager.layerMask = LayerMask.GetMask("Engine");
        trash = true;
        ShowAction();
        
    }

    /// <summary>
    /// receives the name of the focused object,
    /// searcesh the object in objList: remove linerenderer and removes object from the list
    /// </summary>
    /// <param name="me"></param>
    private void DeleteAction(MsgEntry me)
    {
        //extract from me the name of the focused object
        ObjectInfo info = me.Info;

        //iterate the list of objects with line renderer
        for (int i = objList.Count - 1; i >= 0; i--)
        {
            if (objList[i].name == info.ObjType)
            {
                LineRenderer lr = objList[i].GetComponent<LineRenderer>();
                //checl lr is not null the destroy it and remove object from the list
                if (lr != null)
                {
                    Destroy(lr);
                    objList.Remove(objList[i]);
                }
            }            
        }

        //finds the object in the list to be serialide. Removes the object and then
        //save the list through SaveAction()
        for (int i = objInformation.Count - 1; i >= 0; i--)
        {
            if (info.ObjType == objInformation[i].objectName)
            {
                objInformation.Remove(objInformation[i]);
            }
        }
        SaveAction();
        trash = false;
    }

    /// <summary>
    /// draws the line from the focused object to 
    /// the user's position frame by frame
    /// </summary>1    
    private IEnumerator DrawLine(MsgEntry me)
    {
        Vector3 camera = new Vector3(me.Pr[0], me.Pr[1], me.Pr[2]);
        Vector3 obj = new Vector3(me.Pr[3], me.Pr[4], me.Pr[5]);
 
        while (state)
        {
            yield return new WaitForEndOfFrame();
            if (counter < dist)
            {
                counter += .1f / lineDrawSpeed;
                float x = Mathf.Lerp(0, dist, counter);

                Vector3 pointA = camera;
                Vector3 pointB = obj;
                Vector3 pointAngeleLine = x * Vector3.Normalize(pointB - pointA) + pointA;

                lineRenderer.SetPosition(1, pointAngeleLine);
            }
            else
                state = false;
        }
    }

    void IMessageHandler.DealMessage(SyncProto proto)
    {
        this.DealMessage(proto);
    }

    void DealMessage(SyncProto proto)
    {
        Google.Protobuf.Collections.RepeatedField<MsgEntry> messages = 
            proto.SyncMsg.MsgEntry;
        //Debug.Log("deal message");
        if (messages == null)
            return;
        for (int i = 0; i < messages.Count; i++)
        {
            MsgEntry msgEntry = messages[i];
            if (msgEntry.ShowId == showObject.ShowId)
            {
                Debug.Log(msgEntry);
                CreateLineRenderer(msgEntry);
                StartCoroutine(DrawLine(msgEntry));                
            }
            if (msgEntry.ShowId == "showId002")
            {
                DeleteAction(msgEntry);
            }
        }
    }

    /// <summary>
    /// deletes the file first then create it again so it
    /// always contains the newest information
    /// serializes the list and writes a json file
    /// </summary>
    public void SaveAction()
    {
        try
        {
            //File.Delete(Application.persistentDataPath + "/Data.json");
            if (File.Exists(Application.persistentDataPath + "/Data.json"))
                File.Delete(Application.persistentDataPath + "/Data.json");

            File.WriteAllText(Application.persistentDataPath + "/Data.json", JsonConvert.SerializeObject(objInformation));
        }
        catch (Exception e)
        { Debug.Log(e);  }
    }

    /// <summary>
    /// deserialize a json file, extracts the content.
    /// each element in the list is is used in CreateMessage()
    /// and added to the objList of elements with Line renderer
    /// </summary>
    public void LoadAction()
    {
        try
        {
            objInformation = JsonConvert.DeserializeObject<List<Information>>(File.ReadAllText(Application.persistentDataPath + "/Data.json"));
            foreach (Information i in objInformation)
            {
                focused.name = i.objectName;
                mainCamera.transform.position = new Vector3(i.posx, i.posy, i.posz);
                CreateMessage();
            }

            inputManager.layerMask = LayerMask.GetMask("Engine") | LayerMask.GetMask("UI");
        }
        catch (Exception e)
        { Debug.Log(e); }

    }

    /// <summary>
    /// deletes the Json file created previously
    /// </summary>
    public void DeleteJsonAction()
    {
        File.Delete(Application.persistentDataPath + "/Data.json");
    }
}

[Serializable]
public struct Information
{
    public string objectName;
    public float posx;
    public float posy;
    public float posz;
}