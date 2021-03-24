using System.Collections;
using UnityEngine;

public class XRController : MonoBehaviour
{
    public static XRController Instance;

    public GameObject cameraAR;
    public GameObject cameraVR;
    [Space]
    public Material fadeMaterial;
    public Canvas canvasVR;

    private GameObject _currentContent;
    private bool _isEnterVR;
    private bool _isFading;
    private float _fadeValue;
    private Color _color;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        _color = fadeMaterial.color;

        _fadeValue = 0;
        UpdateValue();
    }

    public void XREnter(GameObject content) 
    {
        if (!_isFading)
        {
            _isFading = true;
            _isEnterVR = true;
            _currentContent = content;

            StartCoroutine(Fade());
        }
    }

    public void XRExit()
    {
        if (!_isFading)
        {
            _isFading = true;
            _isEnterVR = false;

            StartCoroutine(Fade());
        }
    }

    private IEnumerator Fade()
    {
        while (_fadeValue < 1)
        {
            _fadeValue += 0.05f;
            UpdateValue();
            yield return new WaitForSeconds(0.05f);;
        }

        cameraVR.SetActive(_isEnterVR);
        cameraAR.SetActive(!_isEnterVR);
        _currentContent.SetActive(_isEnterVR);
        canvasVR.enabled = _isEnterVR;

        while (_fadeValue > 0)
        {
            _fadeValue -= 0.05f;
            UpdateValue();
            yield return new WaitForSeconds(0.05f);;
        }

        _isFading = false;
    }

    private void UpdateValue()
    {
        _color.a = _fadeValue;
        fadeMaterial.color = _color;
    }

}