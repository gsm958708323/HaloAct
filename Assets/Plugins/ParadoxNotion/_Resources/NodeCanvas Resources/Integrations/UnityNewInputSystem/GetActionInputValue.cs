using UnityEngine;
using UnityEngine.InputSystem;
using ParadoxNotion.Design;
using NodeCanvas.Framework;

namespace NodeCanvas.Tasks.Actions
{

    [Category("Input (New System)")]
    [Description("Get an input action value and store it. Note: Not all input actions return a value.")]
    public class GetActionInputValue : NodeCanvas.Framework.ActionTask
    {
        [RequiredField] public InputActionAsset inputActionAsset;
        [ShowIf(nameof(hasValue), 1), BlackboardOnly]
        public Framework.Internal.BBObjectParameter saveValueAs;
        [SerializeField] private string selectedActionID;

        private bool hasValue => saveValueAs.varType != typeof(object);
        private InputAction action;

        protected override string info {
            get
            {
                if ( inputActionAsset == null ) { return "No InputActionAsset Assigned"; }
                if ( string.IsNullOrEmpty(selectedActionID) ) { return "No InputAction Selected"; }
                action = action != null ? action : inputActionAsset.FindAction(selectedActionID);
                if ( action == null ) { return "Action can't be found"; }
                return string.Format("{0} = Input {1}", saveValueAs, action.name);
            }
        }

        protected override string OnInit() {
            if ( string.IsNullOrEmpty(selectedActionID) ) { return "No InputAction Selected"; }
            action = inputActionAsset.FindAction(selectedActionID);
            if ( action != null ) { return null; }
            return "Input Action can not be resolved or is missing";
        }

        protected override void OnExecute() {
            action.Enable();
            action.performed += OnActionPerformed;
            action.canceled += OnActionCanceled;
        }

        protected override void OnStop() {
            action.performed -= OnActionPerformed;
            action.canceled -= OnActionCanceled;
        }

        void OnActionPerformed(InputAction.CallbackContext context) {
            saveValueAs.value = context.ReadValueAsObject();
        }

        void OnActionCanceled(InputAction.CallbackContext context) {
            saveValueAs.value = context.ReadValueAsObject();
        }

        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR
        protected override void OnTaskInspectorGUI() {
            base.OnTaskInspectorGUI();
            if ( inputActionAsset != null && GUILayout.Button("Select Input Action") ) {
                var menu = new UnityEditor.GenericMenu();
                InputAction current = null;
                if ( !string.IsNullOrEmpty(selectedActionID) ) { current = inputActionAsset.FindAction(selectedActionID); }
                foreach ( var map in inputActionAsset.actionMaps ) {
                    foreach ( var action in map.actions ) {
                        menu.AddItem(new GUIContent(map.name + "/" + action.name), current == action, () =>
                        {
                            UndoUtility.RecordObject(ownerSystem.contextObject, "Set Input Action");
                            selectedActionID = action.id.ToString();

                            if ( action.type == InputActionType.Value ) {
                                var controlType = ParadoxNotion.ReflectionTools.GetType(action.expectedControlType, true);
                                saveValueAs.SetType(controlType);
                            } else {
                                saveValueAs.SetType(typeof(object));
                            }

                            saveValueAs.name = null;
                            this.action = null;
                        });
                    }
                }
                menu.ShowAsContext();
            }
        }
#endif
        ///----------------------------------------------------------------------------------------------
    }
}