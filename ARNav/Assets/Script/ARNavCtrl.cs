namespace GoogleARCore.Examples.HelloAR
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using System;

#if UNITY_EDITOR
    // Set up touch input propagation while using Instant Preview in the editor.
    using Input = InstantPreviewInput;
#endif

    public class ARNavCtrl : MonoBehaviour
    {
        private ARNavModel model;

        private Dropdown pathDropdown;
        private Text _text;
        private InputField inputNewField;
        public GameObject m_inputNewPath;
        public GameObject m_arrowObject;

        private int currentRouteIndex;

        public Text DebugText;
        public Text DebugText2;

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
        }

        void Update()
        {
            _UpdateApplicationLifecycle();

            // If the player has not touched the screen, we are done with this update.
            Touch touch;
            if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
            {
                return;
            }

            // Should not handle input if the player is pointing on UI.
            /*if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                return;
            }*/

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
                    var gameObject = Instantiate(prefab, hit.Pose.position, hit.Pose.rotation);

                    // Compensate for the hitPose rotation facing away from the raycast (i.e.
                    // camera).
                    gameObject.transform.Rotate(0, k_PrefabRotation, 0, Space.Self);

                    // Create an anchor to allow ARCore to track the hitpoint as understanding of
                    // the physical world evolves.
                    var anchor = hit.Trackable.CreateAnchor(hit.Pose);

                    // Make game object a child of the anchor.
                    gameObject.transform.parent = anchor.transform;

                    CreateArrow(anchor.transform);
                    model.AddAnchorToCurrentRouter(anchor);
                    model.SaveToJSon();
                }
            }
        }

        public void ActiveInputNewPathField()
        {
            m_inputNewPath.SetActive(true);
        }

        private void InitModel()
        {
            model = new ARNavModel();

            // 從Json讀取路徑資料
            List<string> data = model.ReadFromJson();

            // 初始化路徑選擇Dropdown
            foreach (string name in data)
            {
                pathDropdown.options.Add(new Dropdown.OptionData(name));
            }
            currentRouteIndex = data.Count - 1;
        }

        public void OnAddNewPathBtnClick()
        {
            string newPathName = inputNewField.text;
            pathDropdown.options.Add(new Dropdown.OptionData(newPathName));
            model.AddRoute(newPathName);
            currentRouteIndex = pathDropdown.options.Count;
            m_inputNewPath.gameObject.SetActive(false);
        }

        public void OnDeletePathBtnClick()
        {
            if (currentRouteIndex > 0)
            {
                model.RemoveRouteByIndex(currentRouteIndex);
                pathDropdown.options.RemoveAt(currentRouteIndex);
                currentRouteIndex--;
                pathDropdown.value = currentRouteIndex;
            }
        }

        public void OnPathDropDownChange()
        {
            currentRouteIndex = pathDropdown.value;
            Debug.Log(currentRouteIndex);
            DisplayCurrentRoute();
        }

        public void CreateArrow(Transform newAnchorTransform)
        {
            _text = GameObject.Find("DebugText").GetComponent<Text>();
            float minArrowDistance = 0.1f;//錨點間能放入箭頭的最小距離

            List<AnchorData> anchors = model.GetAnchorsByRouteIndex(2);//錨點的list
            if (anchors.Count == 0)//空routes的時候
            {
                _text.text = "No anchor";
                return;
            }
            else
            {
                _text.text = "Have anchors";
                AnchorData lastAnchor = anchors[anchors.Count - 1];//最後一個錨點

                Vector3 distance = newAnchorTransform.position - lastAnchor._postion;//最後一個錨點與新錨點的距離

                if (distance.magnitude > minArrowDistance)//若距離大於最小值
                {
                    int stage = Convert.ToInt32(distance.magnitude / minArrowDistance);
                    for (int i = 0; i < stage; i++)//根據距離放置箭頭
                    {
                        //放置箭頭，每次的位置是「最後一個錨點的位置朝向新錨點」的方向+最小距離。箭頭的角度旋轉到這個方向
                        GameObject arrow = GameObject.Instantiate(m_arrowObject, Vector3.zero, Quaternion.identity);

                        GameObject temp = new GameObject();
                        temp.transform.position = new Vector3(lastAnchor._postion.x + distance.x / stage * i, lastAnchor._postion.y + distance.y / stage * i, lastAnchor._postion.z + distance.z / stage * i);
                        temp.transform.LookAt(newAnchorTransform);
                        arrow.transform.position = temp.transform.position;
                        arrow.transform.rotation = temp.transform.rotation * Quaternion.Euler(90, 90, 0);
                        GameObject.Destroy(temp);
                    }
                }
            }
        }
        void DisplayCurrentRoute(){
            List<AnchorData> anchors = model.GetAnchorsInCurrentRoute();
            foreach(AnchorData anchor in anchors){
                var newAnchor = Instantiate(GameObjectPointPrefab, anchor._postion, Quaternion.identity);
                CreateArrow(newAnchor.transform);
            }
            DebugText.text = "Read"+model.currentRouteIndex;
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
    }
}