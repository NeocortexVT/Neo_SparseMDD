using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VNyanInterface;

namespace Neo_SparseMDD
{
    public class Sparse_MDD : MonoBehaviour, VNyanInterface.IButtonClickedHandler, VNyanInterface.ITriggerHandler
    {
        public GameObject windowPrefab;

        private GameObject window;

        public InputManager inputManager;

        private InputField MDD_animObject_Input;
        private InputField MDD_keyFrameList_Input;
        private InputField MDD_nStepList_Input;
        private InputField MDD_nExitFrames_Input;
        private InputField MDD_blendShapeOffset_Input;

        // Settings
        public string MDD_parameterNameAnimObject = "MDD_animObject";
        public string MDD_animObject = "";
        
        public string MDD_parameterNameKeyFrameList = "MDD_keyFrameList";
        public List<int> MDD_keyFrameList = new List<int>();
        
        public string MDD_parameterNameNStepList = "MDD_nStepList";
        public List<int> MDD_nStepList = new List<int>();

        public string MDD_parameterNameExitFrames = "MDD_nExitFrames";
        public int MDD_nExitFrames = 0;

        public string MDD_parameterNameBlendShapeOffset = "MDD_blendShapeOffset";
        public int MDD_blendShapeOffset = 0;

        private bool MDD_runAnim = false;
        
        private string input_temp;
        private int MDD_shapeIdx = 0;
        private float MDD_shapeSize = 0;
        private float MDD_shapeMod = 0;
        private float MDD_stepSize = 0;
        private int MDD_blendShapeCount = 0;
        private int MDD_maxFrame = 0;
        private int MDD_listIdx = 0;
        private Mesh MDD_skinnedMesh;
        
        public GameObject MDD_selectObj;

        private string blendshapeName;

        public void Awake()
        {
            if (!(VNyanInterface.VNyanInterface.VNyanUI == null))
            {
                // Register button to plugins window
                VNyanInterface.VNyanInterface.VNyanUI.registerPluginButton("Sparse MDD Animations", this);
                // Register listener for trigger calls. Without this, triggerCalled() will not work.
                VNyanInterface.VNyanInterface.VNyanTrigger.registerTriggerListener(this);

                // Create a window that will show when the button in plugins window is clicked
                window = (GameObject)VNyanInterface.VNyanInterface.VNyanUI.instantiateUIPrefab(windowPrefab);
                inputManager = window.GetComponent<InputManager>();

                MDD_animObject_Input = inputManager.A;
                MDD_keyFrameList_Input = inputManager.B;
                MDD_nStepList_Input = inputManager.C;
                MDD_nExitFrames_Input = inputManager.D;
                MDD_blendShapeOffset_Input = inputManager.E;

                // Load settings
                loadPluginSettings();
            }

            // Hide the window by default
            if (window != null)
            {
                window.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                window.SetActive(false);

                // Set ui component callbacks and loaded values
                MDD_animObject_Input.onEndEdit.AddListener((v) => { MDD_animObject = v;
                    VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterString(MDD_parameterNameAnimObject, MDD_animObject); });

                MDD_keyFrameList_Input.onEndEdit.AddListener((v) => {
                    input_temp = v;
                    MDD_keyFrameList = input_temp?.Split(',')?.Select(Int32.Parse)?.ToList();
                    VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterString(MDD_parameterNameKeyFrameList, input_temp); });

                MDD_nStepList_Input.onEndEdit.AddListener((v) => {
                    input_temp = v;
                    MDD_nStepList = input_temp?.Split(',')?.Select(Int32.Parse)?.ToList();
                    VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterString(MDD_parameterNameNStepList, input_temp); });

                MDD_nExitFrames_Input.onEndEdit.AddListener((v) => {
                    input_temp = v;
                    int.TryParse(input_temp, NumberStyles.Any, CultureInfo.InvariantCulture, out MDD_nExitFrames);
                    VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(MDD_parameterNameExitFrames, MDD_nExitFrames); });

                MDD_blendShapeOffset_Input.onEndEdit.AddListener((v) => {
                    input_temp = v;
                    int.TryParse(input_temp, NumberStyles.Any, CultureInfo.InvariantCulture, out MDD_blendShapeOffset);
                    VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(MDD_parameterNameBlendShapeOffset, MDD_blendShapeOffset); });

            }


        }

        /// <summary>
        /// Load plugin settings
        /// </summary>
        private void loadPluginSettings()
        {
            // Get settings in dictionary
            Dictionary<string, string> settings = VNyanInterface.VNyanInterface.VNyanSettings.loadSettings("SparseMDD.cfg");
            if (settings != null)
            {
                // Read string value
                settings.TryGetValue(MDD_parameterNameAnimObject, out MDD_animObject);

                // Convert second value to list
                if (settings.TryGetValue(MDD_parameterNameKeyFrameList, out string s1))
                {
                    MDD_keyFrameList = s1?.Split(',').Select(e => Int32.Parse(e, CultureInfo.InvariantCulture)).ToList();
                }
		        if (settings.TryGetValue(MDD_parameterNameNStepList, out string s2))
                {
                    MDD_nStepList = s2?.Split(',').Select(e => Int32.Parse(e, CultureInfo.InvariantCulture)).ToList();
                }
                if (settings.TryGetValue(MDD_parameterNameExitFrames, out string s4))
                {
                    int.TryParse(s4, NumberStyles.Any, CultureInfo.InvariantCulture, out MDD_nExitFrames);
                }
                if (settings.TryGetValue(MDD_parameterNameBlendShapeOffset, out string s5))
                {
                    int.TryParse(s5, NumberStyles.Any, CultureInfo.InvariantCulture, out MDD_blendShapeOffset);
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
            settings[MDD_parameterNameAnimObject] = MDD_animObject;
            var MDD_keyFrameList_Formatted = MDD_keyFrameList.Select(d => d.ToString(CultureInfo.InvariantCulture));
            settings[MDD_parameterNameKeyFrameList] = string.Join(",", MDD_keyFrameList_Formatted);
            var MDD_nStepList_Formatted = MDD_nStepList.Select(d => d.ToString(CultureInfo.InvariantCulture));
            settings[MDD_parameterNameNStepList] = string.Join(",", MDD_nStepList_Formatted);
            settings[MDD_parameterNameExitFrames] = MDD_nExitFrames.ToString();
            settings[MDD_parameterNameBlendShapeOffset] = MDD_blendShapeOffset.ToString();

            if (!(VNyanInterface.VNyanInterface.VNyanSettings == null))
            {
                VNyanInterface.VNyanInterface.VNyanSettings.saveSettings("SparseMDD.cfg", settings);
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
            // Listen for a trigger named MDD_runAnimation
            if (name == "MDD_runAnimation")
            {
                if (!(MDD_runAnim))
                {
                    // Retrieve parameter MDD_animObject as variable.
                    MDD_animObject = VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterString(MDD_parameterNameAnimObject);
                    // Update InputField text to match MDD_animObject.
                    MDD_animObject_Input.SetTextWithoutNotify(MDD_animObject);


                    // Retrieve key frame and blend shape parameters and set as variables.
                    MDD_nExitFrames = (int)VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(MDD_parameterNameExitFrames);
                    MDD_nExitFrames_Input.SetTextWithoutNotify(MDD_nExitFrames.ToString());

                    MDD_blendShapeOffset = (int)VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(MDD_parameterNameBlendShapeOffset);
                    MDD_blendShapeOffset_Input.SetTextWithoutNotify(MDD_blendShapeOffset.ToString());

                    MDD_shapeIdx = MDD_blendShapeOffset;


                    // Retreive parameters as variables and update the inputfield.
                    MDD_keyFrameList = VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterString(MDD_parameterNameKeyFrameList)?.Split(',')?.Select(Int32.Parse)?.ToList();
                    MDD_keyFrameList_Input.SetTextWithoutNotify(string.Join(",", MDD_keyFrameList));

                    MDD_nStepList = VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterString(MDD_parameterNameNStepList)?.Split(',')?.Select(Int32.Parse)?.ToList();
                    MDD_nStepList_Input.SetTextWithoutNotify(string.Join(",", MDD_nStepList));


                    // Get the correct mesh variables (also in inactive ones).
                    MDD_skinnedMesh = FindInActiveObjectByName(MDD_animObject);
                    // Get blend shape count on the mesh.
                    MDD_blendShapeCount = MDD_skinnedMesh.blendShapeCount;
                    // Get first blend shape name.
                    blendshapeName = MDD_skinnedMesh.GetBlendShapeName(MDD_shapeIdx);


                    // Recalculate the necessary values for the animation.
                    MDD_maxFrame = MDD_keyFrameList.Max();
                    MDD_stepSize = 100 / MDD_nStepList[MDD_listIdx];
                    MDD_shapeMod = 100 % MDD_nStepList[MDD_listIdx];


                    // Set MDD_runAnim to true to start animation on next frame.
                    MDD_runAnim = true;
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
                // Set MDD_animObject as VNyan parameter
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterString(MDD_parameterNameAnimObject, MDD_animObject); 
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterString(MDD_parameterNameKeyFrameList, string.Join(",", MDD_keyFrameList)); 
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterString(MDD_parameterNameNStepList, string.Join(",", MDD_nStepList));
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(MDD_parameterNameExitFrames, MDD_nExitFrames);
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(MDD_parameterNameBlendShapeOffset, MDD_blendShapeOffset);
            }

            // Set input field text to the defined variables.
            MDD_animObject_Input.SetTextWithoutNotify(MDD_animObject);
            MDD_keyFrameList_Input.SetTextWithoutNotify(string.Join(",", MDD_keyFrameList));
            MDD_nStepList_Input.SetTextWithoutNotify(string.Join(",", MDD_nStepList));
            MDD_nExitFrames_Input.SetTextWithoutNotify(MDD_nExitFrames.ToString());
            MDD_blendShapeOffset_Input.SetTextWithoutNotify(MDD_blendShapeOffset.ToString());

        }

        // Update is called once per frame
        public void Update()
        {
            // If the animation is set to run running.
            if (MDD_runAnim)
            {
                // If the animation has not yet completed the final blendshape as listed or as available on the mesh.
                if ((MDD_shapeIdx < MDD_maxFrame) & (MDD_shapeIdx < MDD_blendShapeCount))
                {
                    // Increase the current blendshape value by the step size.
                    MDD_shapeSize += MDD_stepSize;
                    VNyanInterface.VNyanInterface.VNyanAvatar.setBlendshapeOverride(blendshapeName, (MDD_shapeSize + MDD_shapeMod)/100);
                    
                    // If the blendshape has been maxed out.
                    if (MDD_shapeSize+MDD_shapeMod == 100)
                    {
                        // Move on to the next blendshape.
                        MDD_shapeIdx++;
                        // Reset the total blendshape size.
		                MDD_shapeSize = 0;
                        // Retreive the name of the next blendshape.
                        blendshapeName = MDD_skinnedMesh.GetBlendShapeName(MDD_shapeIdx);

                        // If the animation has reached the last blendshape of the current step size.
                        if (MDD_shapeIdx == MDD_keyFrameList[MDD_listIdx])
		                {
                            // Get the next step size values.
		                    MDD_listIdx++;
		                    MDD_stepSize = 100 / MDD_nStepList[MDD_listIdx];
                            MDD_shapeMod  = 100 % MDD_nStepList[MDD_listIdx];
		                }
                    }
                    
                }
                // Run extra frames if MDD_nExitFrames is larger than 0.
                else if ((MDD_shapeIdx < MDD_maxFrame + MDD_nExitFrames) & (MDD_shapeIdx < MDD_blendShapeCount + MDD_nExitFrames))
                {
                    MDD_shapeIdx++;
                }

                else
                {
                    // Turn off the animation when it is done running.
                    MDD_runAnim = false;
                    // Reset the starting point of the animation to 0.
                    MDD_shapeIdx = 0;
                    MDD_listIdx = 0;

                    // Reset all the blendshapes.
                    for (int ii = MDD_blendShapeOffset; ii < MDD_maxFrame; ii++)
                    {
                        blendshapeName = MDD_skinnedMesh.GetBlendShapeName(ii);
                        VNyanInterface.VNyanInterface.VNyanAvatar.clearBlendshapeOverride(blendshapeName);
                    }

                    VNyanInterface.VNyanInterface.VNyanTrigger.callTrigger("MDD_exitAnimation");
                 
                }
            }
        }
    }
}
