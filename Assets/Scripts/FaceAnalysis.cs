﻿using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class FaceAnalysis : MonoBehaviour {
    /// <summary>
    /// Allows this class to behave like a singleton
    /// </summary>
    public static FaceAnalysis Instance;

    /// <summary>
    /// The analysis result text
    /// </summary>
    private TextMesh labelText;

    /// <summary>
    /// Bytes of the image captured with camera
    /// </summary>
    internal byte[] imageBytes;

    /// <summary>
    /// Path of the image captured with camera
    /// </summary>
    internal string imagePath;

    /// <summary>
    /// Base endpoint of Face Recognition Service
    /// </summary>
    const string baseEndpoint = "https://westus.api.cognitive.microsoft.com/face/v1.0/";

    /// <summary>
    /// Auth key of Face Recognition Service
    /// </summary>
    private const string key = "";

    /// <summary>
    /// Id (name) of the created person group 
    /// </summary>
    private const string personGroupId = "";

    //People
    GameObject Andrew;
    GameObject Tanmoy;
    GameObject Jon;

    //Model & Load
    GameObject modelA;
    GameObject modelT;
    GameObject modelJ;
    GameObject star;
    /// <summary>
    /// Initialises this class
    /// </summary>
    private void Awake()
    {
        // Allows this instance to behave like a singleton
        Instance = this;

        // Add the ImageCapture Class to this Game Object
        gameObject.AddComponent<ImageCapture>();

        // Create the text label in the scene
        CreateLabel();

        //Set everyone's profiles to false
        Andrew = GameObject.Find("Andrew Chang");
        Andrew.SetActive(false);
        Tanmoy = GameObject.Find("Tanmoy Panigrahi");
        Tanmoy.SetActive(false);
        Jon = GameObject.Find("Jon Womack");
        Jon.SetActive(false);
        modelA = GameObject.Find("Model2_0");
        modelA.SetActive(false);
        modelT = GameObject.Find("model3_0");
        modelT.SetActive(false);
        modelJ = GameObject.Find("model4_0");
        modelJ.SetActive(false);
        //star = GameObject.Find("Star Generator Crazy 1");
        //star.SetActive(false);
    }

    /// <summary>
    /// Spawns cursor for the Main Camera
    /// </summary>
    private void CreateLabel()
    {
        // Create a sphere as new cursor
        GameObject newLabel = new GameObject();

        // Attach the label to the Main Camera
        newLabel.transform.parent = gameObject.transform;

        // Resize and position the new cursor
        newLabel.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        newLabel.transform.position = new Vector3(0f, 3f, 60f);

        // Creating the text of the Label
        labelText = newLabel.AddComponent<TextMesh>();
        labelText.anchor = TextAnchor.MiddleCenter;
        labelText.alignment = TextAlignment.Center;
        labelText.tabSize = 4;
        labelText.fontSize = 50;
        labelText.text = ".";
    }

    /// <summary>
    /// Detect faces from a submitted image
    /// </summary>
    internal IEnumerator DetectFacesFromImage()
    {
        WWWForm webForm = new WWWForm();
        string detectFacesEndpoint = $"{baseEndpoint}detect";

        // Change the image into a bytes array
        imageBytes = GetImageAsByteArray(imagePath);

        using (UnityWebRequest www =
            UnityWebRequest.Post(detectFacesEndpoint, webForm))
        {
            www.SetRequestHeader("Ocp-Apim-Subscription-Key", key);
            www.SetRequestHeader("Content-Type", "application/octet-stream");
            www.uploadHandler.contentType = "application/octet-stream";
            www.uploadHandler = new UploadHandlerRaw(imageBytes);
            www.downloadHandler = new DownloadHandlerBuffer();

            yield return www.SendWebRequest();
            string jsonResponse = www.downloadHandler.text;
            Face_RootObject[] face_RootObject =
                JsonConvert.DeserializeObject<Face_RootObject[]>(jsonResponse);

            List<string> facesIdList = new List<string>();
            // Create a list with the face Ids of faces detected in image
            foreach (Face_RootObject faceRO in face_RootObject)
            {
                facesIdList.Add(faceRO.faceId);
                Debug.Log($"Detected face - Id: {faceRO.faceId}");
            }

            StartCoroutine(IdentifyFaces(facesIdList));
        }
    }

    /// <summary>
    /// Returns the contents of the specified file as a byte array.
    /// </summary>
    static byte[] GetImageAsByteArray(string imageFilePath)
    {
        Debug.Log(imageFilePath);
        FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
        BinaryReader binaryReader = new BinaryReader(fileStream);
        return binaryReader.ReadBytes((int)fileStream.Length);
    }

    /// <summary>
    /// Identify the faces found in the image within the person group
    /// </summary>
    internal IEnumerator IdentifyFaces(List<string> listOfFacesIdToIdentify)
    {
        if (listOfFacesIdToIdentify.Count != 0)
        {
            // Create the object hosting the faces to identify
            FacesToIdentify_RootObject facesToIdentify = new FacesToIdentify_RootObject();
            facesToIdentify.faceIds = new List<string>();
            facesToIdentify.personGroupId = personGroupId;
            foreach (string facesId in listOfFacesIdToIdentify)
            {
                facesToIdentify.faceIds.Add(facesId);
            }
            facesToIdentify.maxNumOfCandidatesReturned = 1;
            facesToIdentify.confidenceThreshold = 0.5;

            // Serialise to Json format
            string facesToIdentifyJson = JsonConvert.SerializeObject(facesToIdentify);
            // Change the object into a bytes array
            byte[] facesData = Encoding.UTF8.GetBytes(facesToIdentifyJson);
            Debug.Log(facesData);
            Debug.Log(facesToIdentifyJson);

            WWWForm webForm = new WWWForm();
            string detectFacesEndpoint = $"{baseEndpoint}identify";

            using (UnityWebRequest www = UnityWebRequest.Post(detectFacesEndpoint, webForm))
            {
                www.SetRequestHeader("Ocp-Apim-Subscription-Key", key);
                www.SetRequestHeader("Content-Type", "application/json");
                www.uploadHandler.contentType = "application/json";
                www.uploadHandler = new UploadHandlerRaw(facesData);
                www.downloadHandler = new DownloadHandlerBuffer();

                yield return www.SendWebRequest();
                string jsonResponse = www.downloadHandler.text;
                Debug.Log($"Get Person - jsonResponse: {jsonResponse}");
                Candidate_RootObject[] candidate_RootObject = JsonConvert.DeserializeObject<Candidate_RootObject[]>(jsonResponse);

                // For each face to identify that ahs been submitted, display its candidate
                foreach (Candidate_RootObject candidateRO in candidate_RootObject)
                {
                    StartCoroutine(GetPerson(candidateRO.candidates[0].personId));
                    Debug.Log("Get Person" + GetPerson(candidateRO.candidates[0].personId));

                    // Delay the next "GetPerson" call, so all faces candidate are displayed properly
                    yield return new WaitForSeconds(3);
                }
            }
        }
    }

    /// <summary>
    /// Provided a personId, retrieve the person name associated with it
    /// </summary>
    internal IEnumerator GetPerson(string personId)
    {
        string getGroupEndpoint = $"{baseEndpoint}persongroups/{personGroupId}/persons/{personId}?";
        WWWForm webForm = new WWWForm();

        using (UnityWebRequest www = UnityWebRequest.Get(getGroupEndpoint))
        {
            www.SetRequestHeader("Ocp-Apim-Subscription-Key", key);
            www.downloadHandler = new DownloadHandlerBuffer();
            yield return www.SendWebRequest();
            string jsonResponse = www.downloadHandler.text;

            Debug.Log($"Get Person - jsonResponse: {jsonResponse}");
            IdentifiedPerson_RootObject identifiedPerson_RootObject = JsonConvert.DeserializeObject<IdentifiedPerson_RootObject>(jsonResponse);
            //star.SetActive(false);
            // Display the name of the person in the UI
            if (identifiedPerson_RootObject.name.Equals("Andrew Chang"))
            {
                Tanmoy.SetActive(false);
                Jon.SetActive(false);
                modelT.SetActive(false);
                modelJ.SetActive(false);
                Andrew.SetActive(true);
                modelA.SetActive(true);

            } else if(identifiedPerson_RootObject.name.Equals("Tanmoy Panigrahi"))
            {
                Jon.SetActive(false);
                Andrew.SetActive(false);
                modelA.SetActive(false);
                modelJ.SetActive(false);
                Tanmoy.SetActive(true);
                modelT.SetActive(true);
            }
            else if (identifiedPerson_RootObject.name.Equals("Jon Womack"))
            {
                Andrew.SetActive(false);
                Tanmoy.SetActive(false);
                modelT.SetActive(false);
                modelA.SetActive(false);
                Jon.SetActive(true);
                modelJ.SetActive(true);
            }
            //labelText.text = identifiedPerson_RootObject.name;
        }
    }

}

/// <summary>
/// The Person Group object
/// </summary>
public class Group_RootObject
{
    public string personGroupId { get; set; }
    public string name { get; set; }
    public object userData { get; set; }
}

/// <summary>
/// The Person Face object
/// </summary>
public class Face_RootObject
{
    public string faceId { get; set; }
}

/// <summary>
/// Collection of faces that needs to be identified
/// </summary>
public class FacesToIdentify_RootObject
{
    public string personGroupId { get; set; }
    public List<string> faceIds { get; set; }
    public int maxNumOfCandidatesReturned { get; set; }
    public double confidenceThreshold { get; set; }
}

/// <summary>
/// Collection of Candidates for the face
/// </summary>
public class Candidate_RootObject
{
    public string faceId { get; set; }
    public List<Candidate> candidates { get; set; }
}

public class Candidate
{
    public string personId { get; set; }
    public double confidence { get; set; }
}

/// <summary>
/// Name and Id of the identified Person
/// </summary>
public class IdentifiedPerson_RootObject
{
    public string personId { get; set; }
    public string name { get; set; }
}
