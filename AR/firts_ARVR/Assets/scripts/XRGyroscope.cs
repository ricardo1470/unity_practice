using UnityEngine;

public class XRGyroscope : MonoBehaviour
{
    public bool GyroEnabled { get { return _gyroEnabled; } }
    private bool _gyroEnabled;

    private Gyroscope _gyro;
    private GameObject _container;
    private Quaternion _rotation;
    private Quaternion _rotationBase;

    private void Start()
    {
        CreateContainer();

        _gyroEnabled = EnableGyro();
    }

    private void CreateContainer()
    {
        _container = new GameObject("[XRGyroscope] Container");
        _container.transform.position = transform.position;

        if (transform.parent != null)_container.transform.SetParent(transform.parent);

        transform.SetParent(_container.transform);
    }

    private bool EnableGyro()
    {
        if (SystemInfo.supportsGyroscope)
        {
            _gyro = Input.gyro;
            _gyro.enabled = true;

            _container.transform.localRotation = Quaternion.Euler(90f, 90f, 0);
            _rotationBase = new Quaternion(0, 0, 1, 0);

            return true;
        }
        else
        {
            Debug.LogWarning($"<color=yellow><b>[WARNING]</b></color> Gyro is not supported on this device");

            return false;
        }
    }

    private void Update()
    {
        if (_gyroEnabled)transform.localRotation = _gyro.attitude * _rotationBase;
    }
}