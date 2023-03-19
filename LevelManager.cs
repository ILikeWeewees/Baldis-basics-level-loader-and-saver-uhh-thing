using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;

public class SaveLoadPlanes : MonoBehaviour
{
    public Button saveButton;
    public Button loadButton;
    public InputField fileNameInput;
    public Dropdown fileDropdown;

    private List<Transform> planeTransforms = new List<Transform>();

    void Start()
    {
        saveButton.onClick.AddListener(SavePlanesToJson);
        loadButton.onClick.AddListener(LoadPlanesFromJson);

        // Populate the file dropdown with the available JSON files in the StreamingAssets folder
        string[] jsonFiles = Directory.GetFiles(Application.streamingAssetsPath, "*.json");
        List<string> fileNames = new List<string>();
        foreach (string jsonFile in jsonFiles)
        {
            fileNames.Add(Path.GetFileNameWithoutExtension(jsonFile));
        }
        fileDropdown.ClearOptions();
        fileDropdown.AddOptions(fileNames);
    }

    private void FindPlanes()
    {
        planeTransforms.Clear();
        MeshFilter[] meshFilters = FindObjectsOfType<MeshFilter>();
        foreach (MeshFilter meshFilter in meshFilters)
        {
            if (meshFilter.sharedMesh != null && meshFilter.sharedMesh.name == "Plane")
            {
                planeTransforms.Add(meshFilter.transform);
            }
        }
    }

    private void SavePlanesToJson()
    {
        FindPlanes();

        string fileName = fileNameInput.text + ".json";
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

        List<PlaneData> planeDataList = new List<PlaneData>();
        foreach (Transform planeTransform in planeTransforms)
        {
            PlaneData planeData = new PlaneData();
            planeData.name = planeTransform.name;
            planeData.position = planeTransform.position;
            planeData.rotation = planeTransform.rotation.eulerAngles;
            planeData.material = planeTransform.GetComponent<MeshRenderer>().sharedMaterial.name;
            planeData.scale = planeTransform.localScale;
            planeDataList.Add(planeData);
        }

        string jsonData = JsonUtility.ToJson(new PlaneList(planeDataList), true);

        using (StreamWriter streamWriter = File.CreateText(filePath))
        {
            streamWriter.Write(jsonData);
        }

        Debug.Log("Planes saved to " + filePath);

        // Refresh the file dropdown with the new file
        string[] jsonFiles = Directory.GetFiles(Application.streamingAssetsPath, "*.json");
        List<string> fileNames = new List<string>();
        foreach (string jsonFile in jsonFiles)
        {
            fileNames.Add(Path.GetFileNameWithoutExtension(jsonFile));
        }
        fileDropdown.ClearOptions();
        fileDropdown.AddOptions(fileNames);
    }

    private void LoadPlanesFromJson()
    {
        string fileName = fileDropdown.options[fileDropdown.value].text + ".json";
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

        using (StreamReader streamReader = File.OpenText(filePath))
        {
            string jsonData = streamReader.ReadToEnd();
            PlaneList planeList = JsonUtility.FromJson<PlaneList>(jsonData);
            foreach (PlaneData planeData in planeList.planes)
            {
                GameObject planeObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
                planeObject.name = planeData.name;
                planeObject.transform.position = planeData.position;
                planeObject.transform.rotation = Quaternion.Euler(planeData.rotation);
                planeObject.transform.localScale = planeData.scale;
                Material material = Resources.Load<Material>(planeData.material);
                if (material != null)
                {
                    planeObject.GetComponent<MeshRenderer>().sharedMaterial = material;
                }
            }
        }

        Debug.Log("Planes loaded from " + filePath);
    }
}

[System.Serializable]
public class PlaneData
{
    public string name;
    public Vector3 position;
    public Vector3 rotation;
    public string material;
    public Vector3 scale;
}

[System.Serializable]
public class PlaneList
{
    public List<PlaneData> planes;

    public PlaneList(List<PlaneData> planes)
    {
        this.planes = planes;
    }
}
