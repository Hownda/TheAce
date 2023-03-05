using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RebindUI : MonoBehaviour
{
    [SerializeField]
    private InputActionReference inputActionReference;

    [SerializeField]
    private bool excludeMouse = true;

    [Range(0, 10)]
    [SerializeField]
    private int selectedBinding;

    [SerializeField]
    private InputBinding.DisplayStringOptions displayStringOptions;

    [Header("Binding Info: Readonly")]
    [SerializeField]
    private InputBinding inputBinding;
    private int bindingIndex;

    private string actionName;

    [Header("UI Fields")]
    [SerializeField]
    private Text actionText;
    [SerializeField]
    private Button rebindButton;
    [SerializeField]
    private Text rebindText;
    [SerializeField]
    private Button resetButton;

    private void OnEnable()
    {
        rebindButton.onClick.AddListener(() => DoRebind());
        resetButton.onClick.AddListener(() => ResetBinding());

        if (inputActionReference != null)
        {
            LoadOverride();
            GetBindingInfo();
            UpdateUI();
        }

        KeybindManager.rebindComplete += UpdateUI;
    }

    private IEnumerator LoadOverride()
    {
        yield return new WaitForSeconds(0.2f);
        KeybindManager.LoadBindingOverride(actionName);
    }

    private void OnDisable()
    {
        KeybindManager.rebindComplete -= UpdateUI;
    }
    private void OnValidate()
    {
        if (inputActionReference != null)
        {
            GetBindingInfo();
            UpdateUI();
        }
    }

    private void GetBindingInfo()
    {
        if (inputActionReference.action != null)
        {
            actionName = inputActionReference.action.name;
        }
        if (inputActionReference.action.bindings.Count > selectedBinding)
        {
            inputBinding = inputActionReference.action.bindings[selectedBinding];
            bindingIndex = selectedBinding;
        }
    }

    private void UpdateUI()
    {
        if (actionText != null)
        {
            actionText.text = actionName;
        }
        if (rebindText != null)
        {
            if (Application.isPlaying)
            {
                rebindText.text = KeybindManager.GetBindingName(actionName, bindingIndex);
            }
            else
            {
                rebindText.text = inputActionReference.action.GetBindingDisplayString(bindingIndex);
            }
        }
    }

    private void DoRebind()
    {
        KeybindManager.StartRebind(actionName, bindingIndex, rebindText);
    }

    private void ResetBinding()
    {
        KeybindManager.ResetBinding(actionName, bindingIndex);
        UpdateUI();
    }


}
