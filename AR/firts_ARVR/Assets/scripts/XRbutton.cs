using UnityEngine;
using UnityEngine.UI;

public class XRButton : MonoBehaviour
{
    public GameObject content;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    private void Start()
    {
        _button.onClick.AddListener(() => XRController.Instance.XREnter(content));
    }

}