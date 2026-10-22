using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PixemToggleController : MonoBehaviour
{
    [System.Serializable]
    public class ToggleObjectPair
    {
        public Toggle toggle;
        public GameObject targetObject;
    }

    public ToggleGroup toggleGroup;
    public List<ToggleObjectPair> togglePairs;

    private void Start()
    {
        foreach (var pair in togglePairs)
        {
            pair.toggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    UpdateActiveObjects();
                }
            });
        }

        UpdateActiveObjects();
    }

    private void UpdateActiveObjects()
    {
        foreach (var pair in togglePairs)
        {
            bool shouldBeActive = pair.toggle.isOn && pair.toggle.group == toggleGroup;
            pair.targetObject.SetActive(shouldBeActive);
        }
    }

    private void OnDisable()
    {
        foreach (var pair in togglePairs)
        {
            pair.targetObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        for (int i = 0; i < togglePairs.Count; i++)
        {
            togglePairs[i].toggle.isOn = i == 0;
        }

        UpdateActiveObjects();
    }
}
