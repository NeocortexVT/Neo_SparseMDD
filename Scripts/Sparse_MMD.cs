using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VNyanInterface;

namespace Neo_SparseMMD
{
    public class Sparse_MMD : MonoBehaviour, VNyanInterface.IButtonClickedHandler, VNyanInterface.ITriggerHandler
    {
        public GameObject windowPrefab;

        private GameObject window;

        public InputManager inputManager;

        private InputField MMD_animObject_Input;
        private InputField MMD_keyFrameList_Input;
        private InputField MMD_nStepList_Input;
        private InputField MMD_nExitFrames_Input;
        private InputField MMD_blendShapeOffset_Input;

        // Settings
        public string MMD_parameterNameAnimObject = "MMD_animObject";
        public string MMD_animObject = "";
        
        public string MMD_parameterNameKeyFrameList = "MMD_keyFrameList";
        public List<int> MMD_keyFrameList = new List<int>();
        
        public string MMD_parameterNameNStepList = "MMD_nStepList";
        public List<int> MMD_nStepList = new List<int>();

        public string MMD_parameterNameExitFrames = "MMD_nExitFrames";
        public int MMD_nExitFrames = 0;

        public string MMD_parameterNameBlendShapeOffset = "MMD_blendShapeOffset";
        public int MMD_blendShapeOffset = 0;

        private bool MMD_runAnim = false;
        
        private string input_temp;
        private int MMD_shapeIdx = 0;
        private float MMD_shapeSize = 0;
        private float MMD_shapeMod = 0;
        private float MMD_stepSize = 0;
        private int MMD_blendShapeCount = 0;
        private int MMD_maxFrame = 0;
        private int MMD_listIdx = 0;
        private Mesh MMD_skinnedMesh;
        
        public GameObject MMD_selectObj;

        private string blendshapeName;

        public void Awake()
        {
            if (!(VNyanInterface.VNyanInterface.VNyanUI == null))
            {
                // Register button to plugins window
                VNyanInterface.VNyanInterface.VNyanUI.registerPluginButton("Sparse MMD Animations", this);
                // Register listener for trigger calls. Without this, triggerCalled() will not work.
                VNyanInterface.VNyanInterface.VNyanTrigger.registerTriggerListener(this);

                // Create a window that will show when the button in plugins window is clicked
                window = (GameObject)VNyanInterface.VNyanInterface.VNyanUI.instantiateUIPrefab(windowPrefab);
                inputManager = window.GetComponent<InputManager>();

                MMD_animObject_Input = inputManager.A;
                MMD_keyFrameList_Input = inputManager.B;
                MMD_nStepList_Input = inputManager.C;
                MMD_nExitFrames_Input = inputManager.D;
                MMD_blendShapeOffset_Input = inputManager.E;

                // Load settings
                loadPluginSettings();
            }

            // Hide the window by default
            if (window != null)
            {
                window.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                window.SetActive(false);

                // Set ui component callbacks and loaded values
                MMD_animObject_Input.onEndEdit.AddListener((v) => { MMD_animObject = v;
                    VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterString(MMD_parameterNameAnimObject, MMD_animObject); });

                MMD_keyFrameList_Input.onEndEdit.AddListener((v) => {
                    input_temp = v;
                    MMD_keyFrameList = input_temp?.Split(',')?.Select(Int32.Parse)?.ToList();
                    VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterString(MMD_parameterNameKeyFrameList, input_temp); });

                MMD_nStepList_Input.onEndEdit.AddListener((v) => {
                    input_temp = v;
                    MMD_nStepList = input_temp?.Split(',')?.Select(Int32.Parse)?.ToList();
                    VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterString(MMD_parameterNameNStepList, input_temp); });

                MMD_nExitFrames_Input.onEndEdit.AddListener((v) => {
                    input_temp = v;
                    int.TryParse(input_temp, NumberStyles.Any, CultureInfo.InvariantCulture, out MMD_nExitFrames);
                    VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(MMD_parameterNameExitFrames, MMD_nExitFrames); });

                MMD_blendShapeOffset_Input.onEndEdit.AddListener((v) => {
                    input_temp = v;
                    int.TryParse(input_temp, NumberStyles.Any, CultureInfo.InvariantCulture, out MMD_blendShapeOffset);
                    VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(MMD_parameterNameBlendShapeOffset, MMD_blendShapeOffset); });

            }


        }

        /// <summary>
        /// Load plugin settings
        /// </summary>
        private void loadPluginSettings()
        {
            // Get settings in dictionary
            Dictionary<string, string> settings = VNyanInterface.VNyanInterface.VNyanSettings.loadSettings("SparseMMD.cfg");
            if (settings != null)
            {
                // Read string value
                settings.TryGetValue(MMD_parameterNameAnimObject, out MMD_animObject);

                // Convert second value to list
                if (settings.TryGetValue(MMD_parameterNameKeyFrameList, out string s1))
                {
                    MMD_keyFrameList = s1?.Split(',').Select(e => Int32.Parse(e, CultureInfo.InvariantCulture)).ToList();
                }
		        if (settings.TryGetValue(MMD_parameterNameNStepList, out string s2))
                {
                    MMD_nStepList = s2?.Split(',').Select(e => Int32.Parse(e, CultureInfo.InvariantCulture)).ToList();
                }
                if (settings.TryGetValue(MMD_parameterNameExitFrames, out string s4))
                {
                    int.TryParse(s4, NumberStyles.Any, CultureInfo.InvariantCulture, out MMD_nExitFrames);
                }
                if (settings.TryGetValue(MMD_parameterNameBlendShapeOffset, out string s5))
                {
                    int.TryParse(s5, NumberStyles.Any, CultureInfo.InvariantCulture, out MMD_blendShapeOffset);
                }
            }
        }

        /// <summary>
        /// Called when VNyan is shutting down
        /// </summary>
        private void OnApplicationQuit()
        {
            // Save settings
            savePluginSettings();
        }

        private void savePluginSettings()
        {
            Dictionary<string, string> settings = new Dictionary<string, string>();
            settings[MMD_parameterNameAnimObject] = MMD_animObject;
            var MMD_keyFrameList_Formatted = MMD_keyFrameList.Select(d => d.ToString(CultureInfo.InvariantCulture));
            settings[MMD_parameterNameKeyFrameList] = string.Join(",", MMD_keyFrameList_Formatted);
            var MMD_nStepList_Formatted = MMD_nStepList.Select(d => d.ToString(CultureInfo.InvariantCulture));
            settings[MMD_parameterNameNStepList] = string.Join(",", MMD_nStepList_Formatted);
            settings[MMD_parameterNameExitFrames] = MMD_nExitFrames.ToString();
            settings[MMD_parameterNameBlendShapeOffset] = MMD_blendShapeOffset.ToString();

            if (!(VNyanInterface.VNyanInterface.VNyanSettings == null))
            {
                VNyanInterface.VNyanInterface.VNyanSettings.saveSettings("SparseMMD.cfg", settings);
            }
        }

        public void pluginButtonClicked()
        {
            // Flip the visibility of the window when plugin window button is clicked
            if (window != null)
            {
                window.SetActive(!window.activeSelf);
                if(window.activeSelf)
                    window.transform.SetAsLastSibling();
            }
                
        }

        public void triggerCalled(string name)
        {
            // Listen for a trigger named MMD_runAnimation
            if (name == "MMD_runAnimation")
            {
                if (!(MMD_runAnim))
                {
                    // Retrieve parameter MMD_animObject as variable.
                    MMD_animObject = VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterString(MMD_parameterNameAnimObject);
                    // Update InputField text to match MMD_animObject.
                    MMD_animObject_Input.SetTextWithoutNotify(MMD_animObject);


                    // Retrieve key frame and blend shape parameters and set as variables.
                    MMD_nExitFrames = (int)VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(MMD_parameterNameExitFrames);
                    MMD_nExitFrames_Input.SetTextWithoutNotify(MMD_nExitFrames.ToString());

                    MMD_blendShapeOffset = (int)VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(MMD_parameterNameBlendShapeOffset);
                    MMD_blendShapeOffset_Input.SetTextWithoutNotify(MMD_blendShapeOffset.ToString());

                    MMD_shapeIdx = MMD_blendShapeOffset;


                    // Retreive parameters as variables and update the inputfield.
                    MMD_keyFrameList = VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterString(MMD_parameterNameKeyFrameList)?.Split(',')?.Select(Int32.Parse)?.ToList();
                    MMD_keyFrameList_Input.SetTextWithoutNotify(string.Join(",", MMD_keyFrameList));

                    MMD_nStepList = VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterString(MMD_parameterNameNStepList)?.Split(',')?.Select(Int32.Parse)?.ToList();
                    MMD_nStepList_Input.SetTextWithoutNotify(string.Join(",", MMD_nStepList));


                    // Get the correct mesh variables (also in inactive ones).
                    MMD_skinnedMesh = FindInActiveObjectByName(MMD_animObject);
                    // Get blend shape count on the mesh.
                    MMD_blendShapeCount = MMD_skinnedMesh.blendShapeCount;
                    // Get first blend shape name.
                    blendshapeName = MMD_skinnedMesh.GetBlendShapeName(MMD_shapeIdx);


                    // Recalculate the necessary values for the animation.
                    MMD_maxFrame = MMD_keyFrameList.Max();
                    MMD_stepSize = 100 / MMD_nStepList[MMD_listIdx];
                    MMD_shapeMod = 100 % MMD_nStepList[MMD_listIdx];


                    // Set MMD_runAnim to true to start animation on next frame.
                    MMD_runAnim = true;
                }
            }
            
        }

        public Mesh FindInActiveObjectByName(string name)
        {
            Debug.Log(name);
            SkinnedMeshRenderer[] objs = Resources.FindObjectsOfTypeAll<SkinnedMeshRenderer>() as SkinnedMeshRenderer[];
            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i].hideFlags == HideFlags.None)
                {
                    Debug.Log(objs[i].name);
                    if (objs[i].name == name)
                    {
                        return objs[i].sharedMesh;
                    }
                }
            }
            return null;
        }

        public void Start()
        {
            if (!(VNyanInterface.VNyanInterface.VNyanParameter == null))
            {
                // Set MMD_animObject as VNyan parameter
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterString(MMD_parameterNameAnimObject, MMD_animObject); 
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterString(MMD_parameterNameKeyFrameList, string.Join(",", MMD_keyFrameList)); 
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterString(MMD_parameterNameNStepList, string.Join(",", MMD_nStepList));
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(MMD_parameterNameExitFrames, MMD_nExitFrames);
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(MMD_parameterNameBlendShapeOffset, MMD_blendShapeOffset);
            }

            // Set input field text to the defined variables.
            MMD_animObject_Input.SetTextWithoutNotify(MMD_animObject);
            MMD_keyFrameList_Input.SetTextWithoutNotify(string.Join(",", MMD_keyFrameList));
            MMD_nStepList_Input.SetTextWithoutNotify(string.Join(",", MMD_nStepList));
            MMD_nExitFrames_Input.SetTextWithoutNotify(MMD_nExitFrames.ToString());
            MMD_blendShapeOffset_Input.SetTextWithoutNotify(MMD_blendShapeOffset.ToString());

        }

        // Update is called once per frame
        public void Update()
        {
            // If the animation is set to run running.
            if (MMD_runAnim)
            {
                // If the animation has not yet completed the final blendshape as listed or as available on the mesh.
                if ((MMD_shapeIdx < MMD_maxFrame) & (MMD_shapeIdx < MMD_blendShapeCount))
                {
                    // Increase the current blendshape value by the step size.
                    MMD_shapeSize += MMD_stepSize;
                    VNyanInterface.VNyanInterface.VNyanAvatar.setBlendshapeOverride(blendshapeName, (MMD_shapeSize + MMD_shapeMod)/100);
                    
                    // If the blendshape has been maxed out.
                    if (MMD_shapeSize+MMD_shapeMod == 100)
                    {
                        // Move on to the next blendshape.
                        MMD_shapeIdx++;
                        // Reset the total blendshape size.
		                MMD_shapeSize = 0;
                        // Retreive the name of the next blendshape.
                        blendshapeName = MMD_skinnedMesh.GetBlendShapeName(MMD_shapeIdx);

                        // If the animation has reached the last blendshape of the current step size.
                        if (MMD_shapeIdx == MMD_keyFrameList[MMD_listIdx])
		                {
                            // Get the next step size values.
		                    MMD_listIdx++;
		                    MMD_stepSize = 100 / MMD_nStepList[MMD_listIdx];
                            MMD_shapeMod  = 100 % MMD_nStepList[MMD_listIdx];
		                }
                    }
                    
                }
                // Run extra frames if MMD_nExitFrames is larger than 0.
                else if ((MMD_shapeIdx < MMD_maxFrame + MMD_nExitFrames) & (MMD_shapeIdx < MMD_blendShapeCount + MMD_nExitFrames))
                {
                    MMD_shapeIdx++;
                }

                else
                {
                    // Turn off the animation when it is done running.
                    MMD_runAnim = false;
                    // Reset the starting point of the animation to 0.
                    MMD_shapeIdx = 0;
                    MMD_listIdx = 0;

                    // Reset all the blendshapes.
                    for (int ii = MMD_blendShapeOffset; ii < MMD_maxFrame; ii++)
                    {
                        blendshapeName = MMD_skinnedMesh.GetBlendShapeName(ii);
                        VNyanInterface.VNyanInterface.VNyanAvatar.clearBlendshapeOverride(blendshapeName);
                    }

                    VNyanInterface.VNyanInterface.VNyanTrigger.callTrigger("MMD_exitAnimation");
                 
                }
            }
        }
    }
}
