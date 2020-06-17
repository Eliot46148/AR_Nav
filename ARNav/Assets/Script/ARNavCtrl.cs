namespace GoogleARCore
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;
    using UnityEditor;
    using System;
    using System.Linq;

#if UNITY_EDITOR
    // Set up touch input propagation while using Instant Preview in the editor.
    using Input = InstantPreviewInput;
#endif

    public class ARNavCtrl : MonoBehaviour
    {
        private ARNavModel model;
        private Dropdown pathDropdown;
        private Context context;
        private Text _text;
        private InputField inputNewField;
        public GameObject m_inputframe;
        public GameObject m_inputNewPath;
        public GameObject m_arrowObject;
        public GameObject DialogPanel;
        public GameObject AlertPanel;
        public DialogBox dialog;
        public AlertBox alert;
        public GameObject ModeText;
        public Text DebugText;
        public Text DebugText2;

        int testi = 0;


        /// <summary>
        /// The first-person camera being used to render the passthrough camera image (i.e. AR
        /// background).
        /// </summary>
        public Camera FirstPersonCamera;

        /// <summary>
        /// A prefab to place when a raycast from a user touch hits a vertical plane.
        /// </summary>
        public GameObject GameObjectVerticalPlanePrefab;

        /// <summary>
        /// A prefab to place when a raycast from a user touch hits a horizontal plane.
        /// </summary>
        public GameObject GameObjectHorizontalPlanePrefab;

        /// <summary>
        /// A prefab to place when a raycast from a user touch hits a feature point.
        /// </summary>
        public GameObject GameObjectPointPrefab;

        /// <summary>
        /// The rotation in degrees need to apply to prefab when it is placed.
        /// </summary>
        private const float k_PrefabRotation = 180.0f;

        /// <summary>
        /// True if the app is in the process of quitting due to an ARCore connection error,
        /// otherwise false.
        /// </summary>
        private bool m_IsQuitting = false;

        public void Awake()
        {
            Application.targetFrameRate = 60;
        }
        void Start()
        {
            pathDropdown = GameObject.Find("PathDropdown").GetComponent<Dropdown>();
            inputNewField = m_inputNewPath.GetComponent<InputField>();
            InitModel();
            DisplayCurrentRoute();
            DebugText.text = "test";
            dialog = new DialogBox(DialogPanel);
            alert = new AlertBox(AlertPanel);
            context = new Context(new UnityAction(UserWork), new UnityAction(ManagerWork));
        }

        void Update()
        {
            _UpdateApplicationLifecycle();
            DisplayDistance();
            context.Run();
        }

        /// <summary>
        /// Update handler in Manager mode.
        /// </summary>
        void ManagerWork()
        {
            // Debug.Log("Manager");
            // If the player has not touched the screen, we are done with this update.
            Touch touch;
            if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
            {
                return;
            }

            // Should not handle input if the player is pointing on UI.
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                return;
            }

            // Raycast against the location the player touched to search for planes.
            TrackableHit hit;
            TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
                TrackableHitFlags.FeaturePointWithSurfaceNormal;

            if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
            {
                // Use hit pose and camera pose to check if hittest is from the
                // back of the plane, if it is, no need to create the anchor.
                if ((hit.Trackable is DetectedPlane) &&
                    Vector3.Dot(FirstPersonCamera.transform.position - hit.Pose.position,
                        hit.Pose.rotation * Vector3.up) < 0)
                {
                    Debug.Log("Hit at back of the current DetectedPlane");
                }
                else
                {
                    // Choose the prefab based on the Trackable that got hit.
                    Debug.Log("hitted");
                    GameObject prefab;
                    if (hit.Trackable is FeaturePoint)
                    {
                        prefab = GameObjectPointPrefab;
                    }
                    else if (hit.Trackable is DetectedPlane)
                    {
                        DetectedPlane detectedPlane = hit.Trackable as DetectedPlane;
                        if (detectedPlane.PlaneType == DetectedPlaneType.Vertical)
                        {
                            prefab = GameObjectVerticalPlanePrefab;
                        }
                        else
                        {
                            prefab = GameObjectHorizontalPlanePrefab;
                        }
                    }
                    else
                    {
                        prefab = GameObjectHorizontalPlanePrefab;
                    }

                    // Instantiate prefab at the hit pose.
                    var AnchorGameObject = Instantiate(prefab, hit.Pose.position, hit.Pose.rotation);
                    AnchorGameObject.transform.parent = GameObject.Find("Root").transform.Find("Anchor");

                    // Compensate for the hitPose rotation facing away from the raycast (i.e.
                    // camera).
                    AnchorGameObject.transform.Rotate(0, k_PrefabRotation, 0, Space.Self);

                    // Create an anchor to allow ARCore to track the hitpoint as understanding of
                    // the physical world evolves.
                    var anchor = hit.Trackable.CreateAnchor(hit.Pose);

                    anchor.transform.parent = GameObject.Find("Root").transform.Find("Anchor");

                    // Make game object a child of the anchor.
                    AnchorGameObject.transform.parent = anchor.transform;
                    AnchorGameObject.transform.rotation *= Quaternion.Euler(0, 180, 0);

                    Vector3 temp;
                    if (model.GetAnchorsInCurrentRoute().Count == 0)
                        temp = anchor.transform.position;
                    else
                        temp = model.GetAnchorsInCurrentRoute().Last()._postion;
                    model.AddAnchorToCurrentRouter(anchor);
                    CreateArrow(temp, anchor.transform.position);

                    AnchorGameObject.GetComponentInChildren<TextMesh>().text = model.GetAnchorsInCurrentRoute().Count.ToString();//顯示編號                    
                }
            }
        }

        /// <summary>
        /// Update handler in User mode.
        /// </summary>
        void UserWork()
        {
            // Debug.Log("User");
            return;
        }

        public void ActiveInputNewPathField()
        {
            m_inputframe.SetActive(true);
        }

        private void InitModel()
        {
            model = new ARNavModel();

            // 從Json讀取路徑資料
            List<string> data = model.ReadFromJson();
            Debug.Log("Amount of routes: " + data.Count);

            // 初始化路徑選擇Dropdown
            foreach (string name in data)
            {
                pathDropdown.options.Add(new Dropdown.OptionData(name));
            }
            model.currentRouteIndex = data.Count - 1;
            pathDropdown.value = model.currentRouteIndex;
        }

        public void OnAddNewPathBtnClick()
        {
            string newPathName = inputNewField.text;
            pathDropdown.options.Add(new Dropdown.OptionData(newPathName));
            model.AddRoute(newPathName);
            model.currentRouteIndex = pathDropdown.options.Count;
            m_inputframe.gameObject.SetActive(false);
            pathDropdown.value = model.currentRouteIndex;
        }

        public void OnDeletePathBtnClick()
        {
            dialog.SetInfo("", "刪除後將無法復原\n確認刪除？");
            dialog.show(new UnityAction(OnDialogConfirmBtnClicked), new UnityAction(OnDialogCancelBtnClicked));
        }

        public void OnDialogConfirmBtnClicked()
        {
            if (model.mapData.Length > 1)
            {
                model.RemoveRouteByIndex(model.currentRouteIndex);
                pathDropdown.options.RemoveAt(model.currentRouteIndex);
                model.currentRouteIndex = 0;
                pathDropdown.value = model.currentRouteIndex;
                pathDropdown.RefreshShownValue();
                DisplayCurrentRoute();
            }
            else
            {
                alert.SetInfo("", "需要有兩條以上路徑才能進行刪除");
                alert.show();
            }
        }

        public void OnDialogCancelBtnClicked()
        {
            return;
        }

        public void OnRemoveAnchorBtnClicked()
        {
            if (model.GetCurrentRouteLength() > 0)
            {
                model.RemoveLastAnchor();
                RefreshView();
            }
            else
            {
                alert.SetInfo("", "路徑點數量為零");
                alert.show();
            }
            Debug.Log("Length of Current Route:" + model.GetCurrentRouteLength());
        }


        /// <summary>
        /// Triggered when changing the UI PathDropDown status
        /// </summary>
        public void OnPathDropDownChange()
        {
            model.currentRouteIndex = pathDropdown.value;
            Debug.Log("Now on route: " + model.currentRouteIndex);
            RefreshView();
        }

        public void OnSaveBtnClicked()
        {
            model.SaveToJSon();
            alert.SetInfo("", "儲存成功");
            alert.show();
        }


        /// <summary>
        /// Connect anchor newAnchor and lastAnchor with arrow
        /// </summary>
        /// <param name="newAnchorPostion">Transform of new anchor</param>
        /// <param name="lastAnchorPosition">Index of last anchor (to be connected></param>
        public void CreateArrow(Vector3 lastAnchorPosition, Vector3 newAnchorPostion)
        {
            float minArrowDistance = 0.1f;//錨點間能放入箭頭的最小距離
            if ((newAnchorPostion - lastAnchorPosition).magnitude <= minArrowDistance)//距離太短
            {
                return;
            }

            _text = GameObject.Find("DebugText").GetComponent<Text>();

            List<AnchorData> anchors = model.GetAnchorsInCurrentRoute();//錨點的list
            if (anchors.Count == 0)//routes還沒有錨點
            {
                _text.text = "No anchor";
                return;
            }
            else
            {
                _text.text = "Have anchors";

                Vector3 distance = newAnchorPostion - lastAnchorPosition;//最後一個錨點與新錨點的距離

                int stage = Convert.ToInt32(distance.magnitude / minArrowDistance);
                for (int i = 0; i < stage; i++)//根據距離放置箭頭
                {
                    //放置箭頭，每次的位置是「最後一個錨點的位置朝向新錨點」的方向+最小距離。箭頭的角度旋轉到這個方向
                    GameObject arrow = Instantiate(m_arrowObject, Vector3.zero, Quaternion.identity);
                    arrow.transform.parent = GameObject.Find("Root").transform.Find("Arrow");

                    GameObject temp = new GameObject();
                    temp.transform.position = new Vector3(lastAnchorPosition.x + distance.x / stage * i, lastAnchorPosition.y + distance.y / stage * i, lastAnchorPosition.z + distance.z / stage * i);
                    temp.transform.LookAt(newAnchorPostion);
                    arrow.transform.position = temp.transform.position;
                    arrow.transform.rotation = temp.transform.rotation * Quaternion.Euler(90, 90, 0);
                    GameObject.Destroy(temp);
                }
            }
        }

        /// <summary>
        /// Destroy all children of entered parent object.
        /// </summary>
        /// <param name="parent">Transform of parent object.</param>
        void DestroyAllChildren(Transform parent)
        {
            foreach (Transform child in parent)
            {
                // Debug.Log("Destroying " + child.name);
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// Remove all anchors on the AR plane.
        /// </summary>
        void DestroyAllAnchors()
        {
            DestroyAllChildren(GameObject.Find("Root").transform.Find("Anchor"));
            DestroyAllChildren(GameObject.Find("Root").transform.Find("Arrow"));
        }

        /// <summary>
        /// Re_render the anchors on the AR plane.
        /// </summary>
        void RefreshView()
        {
            DestroyAllAnchors();
            DisplayCurrentRoute();
        }


        /// <summary>
        /// Place the current route's anchors.
        /// </summary>
        void DisplayCurrentRoute()
        {
            List<AnchorData> anchors = model.GetAnchorsInCurrentRoute();

            if (anchors.Count <= 0) return;

            foreach (AnchorData anchor in anchors)
            {
                var newAnchor = Instantiate(GameObjectPointPrefab, anchor._postion, Quaternion.identity);
                newAnchor.transform.parent = GameObject.Find("Root").transform.Find("Anchor");

                newAnchor.GetComponentInChildren<TextMesh>().text = (anchors.IndexOf(anchor) + 1).ToString();//顯示編號

                Vector3 lastAnchorPosition;
                if (anchors.IndexOf(anchor) == 0)
                {
                    lastAnchorPosition = newAnchor.transform.position;
                }
                else
                {
                    lastAnchorPosition = anchors[anchors.IndexOf(anchor) - 1]._postion;
                }
                CreateArrow(lastAnchorPosition, newAnchor.transform.position);
            }
        }

        void DisplayDistance()
        {
            //管理者顯示目前路徑總長幾公尺
            //使用者顯示距離目的地還有幾公尺
            Text distanceText = GameObject.Find("Distance").GetComponent<Text>();

            List<AnchorData> anchors = model.GetAnchorsInCurrentRoute();
            float distance = 0;

            //if(manager)//管理方
            {
                /*for (int i = anchors.Count - 1; i > 0; i--)
                {
                    distance += (anchors[i]._postion - anchors[i - 1]._postion).magnitude;
                }

                debugText.text = "路徑總長:" + distance.ToString("F1") + "公尺";*/
            }
            //else if(user)//使用者
            {
                if (anchors.Count == 0)
                {
                    return;
                }

                int nearestAnchorIndex = FindNearestAnchorIndex();
                Vector3 nearestAnchorPosition = anchors[nearestAnchorIndex]._postion;

                if (nearestAnchorIndex == anchors.Count - 1)//最近的是最後一個點
                {
                    distance = (Camera.main.transform.position - anchors[nearestAnchorIndex]._postion).magnitude;
                }
                else
                {
                    distance = (Camera.main.transform.position - anchors[nearestAnchorIndex + 1]._postion).magnitude;
                    for (int i = anchors.Count - 1; i > nearestAnchorIndex + 1; i--)
                    {
                        distance += (anchors[i]._postion - anchors[i - 1]._postion).magnitude;
                    }
                }

                if (distance <= 2)
                {
                    distanceText.text = "距離目的地: < 2公尺";
                }
                else
                {
                    distanceText.text = "距離目的地:" + distance.ToString("F1") + "公尺";
                }
            }
        }
        int FindNearestAnchorIndex()//尋找最近的Anchor的Index
        {
            List<AnchorData> anchors = model.GetAnchorsInCurrentRoute();
            if (anchors.Count == 0)
            {
                return -1;
            }

            float min = (Camera.main.transform.position - anchors[0]._postion).magnitude;
            int index = 0;

            for (int i = 1; i < anchors.Count; i++)
            {
                float currentDistance = (Camera.main.transform.position - anchors[i]._postion).magnitude;
                if (currentDistance < min)
                {
                    min = currentDistance;
                    index = i;
                }
            }

            return index;
        }

        /// <summary>
        /// Check and update the application lifecycle.
        /// </summary>
        private void _UpdateApplicationLifecycle()
        {
            // Exit the app when the 'back' button is pressed.
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }

            // Only allow the screen to sleep when not tracking.
            if (Session.Status != SessionStatus.Tracking)
            {
                Screen.sleepTimeout = SleepTimeout.SystemSetting;
            }
            else
            {
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }

            if (m_IsQuitting)
            {
                return;
            }

            // Quit if ARCore was unable to connect and give Unity some time for the toast to
            // appear.
            if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
            {
                _ShowAndroidToastMessage("Camera permission is needed to run this application.");
                m_IsQuitting = true;
                Invoke("_DoQuit", 0.5f);
            }
            else if (Session.Status.IsError())
            {
                _ShowAndroidToastMessage(
                    "ARCore encountered a problem connecting.  Please start the app again.");
                m_IsQuitting = true;
                Invoke("_DoQuit", 0.5f);
            }
        }

        /// <summary>
        /// Actually quit the application.
        /// </summary>
        private void _DoQuit()
        {
            Application.Quit();
        }

        /// <summary>
        /// Show an Android toast message.
        /// </summary>
        /// <param name="message">Message string to show in the toast.</param>
        private void _ShowAndroidToastMessage(string message)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity =
                unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if (unityActivity != null)
            {
                AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
                unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    AndroidJavaObject toastObject =
                        toastClass.CallStatic<AndroidJavaObject>(
                            "makeText", unityActivity, message, 0);
                    toastObject.Call("show");
                }));
            }
        }

        public void Test()
        {
            if (testi % 2 != 0)
            {
                context.SetUserMode();
                ModeText.SetActive(false);
            }
            else
            {
                context.SetManagerMode();
                ModeText.SetActive(true);
            }
            context.Run();
            testi++;
        }
    }
}