//using UnityEngine;

//public class CollectPickup : MonoBehaviour, IInteractable
//{
//    public CollectItem Item;
//    public CollectCam collectycam;

//    [Space(5)]
//    public int CollectibleCameraLayer = 16;
//    public GameObject CollectibleCanvas;
//    public GameObject CollectibleEmpty;
//    public GameObject CollectibleModel;
//    public GameObject collectInstant;

//    private void Start()
//    {
//        if (SaveManager.HasCollected(Item.CollectibleCode))
//            gameObject.SetActive(false);
//    }

//    public void Interact()
//    {
//        SaveManager.Collect(Item);

//        PlayerPrefs.SetInt("CameraDisable", 1);
//        CollectibleCanvas.SetActive(true);
//        //CollectibleModel.layer = CollectibleCameraLayer;
//        collectInstant = Instantiate(CollectibleModel, CollectibleEmpty.transform);
//        Cursor.lockState = CursorLockMode.Confined;
//        collectycam.collectInstant = collectInstant;
//        collectycam.ItemAttributes = Item;
//        SetLayerRecursively(collectInstant, CollectibleCameraLayer);
//        gameObject.SetActive(false);
//    }

//    public string GetInteractionType()
//    {
//        return "Collectible";
//    }

//    void SetLayerRecursively(GameObject obj, int newLayer)
//    {
//        if (obj == null) return;
//        obj.layer = newLayer;

//        foreach (Transform child in obj.transform)
//        {
//            SetLayerRecursively(child.gameObject, newLayer);
//        }
//    }
//}
