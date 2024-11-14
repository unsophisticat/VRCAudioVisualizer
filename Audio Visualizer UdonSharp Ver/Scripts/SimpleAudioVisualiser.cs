using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;

public class SimpleAudioVisualiser : UdonSharpBehaviour
{
    [Header("Frequency Band Analyser")]
    public AudioSource _AudioSource;
    public int _FreqBands = 32;
    public float _SmoothDownRate = 5;
    public float _Scalar = 0.1f;
    
    int _FrequencyBins = 512;
    float[] _Samples;
    float[] _SampleBuffer;

    private float[] _FreqBandsCustom;

    GameObject[] GameObjects;
    public GameObject _ObjectToSpawn;

    [Header("Object Array")]
    public bool LookAtCenter = true;
    public float radius = 5f; // Radius for the circular arrangement
    public float rotationSpeed = 0.1f; // Speed of rotation around the center
    public float ScaleStrength = 10f;

    private Vector3 _ScaleStrength;
    private Vector3 _BaseScale;
    private Vector3 displacement;

    private void Start()
    {
        GameObjects = new GameObject[_FreqBands];
        _BaseScale = _ObjectToSpawn.transform.localScale;
        displacement = this.transform.position;

        _ScaleStrength = new Vector3(0f, ScaleStrength, 0f);

        _Samples = new float[_FrequencyBins];
        _SampleBuffer = new float[_FrequencyBins];

        _FreqBandsCustom = new float[_FreqBands];

        // Circular arrangement of the objects
        float angleSpacing = (2f * Mathf.PI) / _FreqBands;
        for (int i = 0; i < GameObjects.Length; i++)
        {
            float angle = i * angleSpacing;
            float x = Mathf.Sin(angle) * radius;
            float z = Mathf.Cos(angle) * radius;

            GameObject newObject = VRCInstantiate(_ObjectToSpawn);
            newObject.transform.SetParent(transform);
            newObject.transform.localPosition = new Vector3(x, 0, z);

            // Rotate the object to face outward from the center
            newObject.transform.LookAt(transform.position);
            newObject.transform.localRotation *= Quaternion.Euler(-90, 0, 0);

            GameObjects[i] = newObject;
        }
    }

    void UpdateFreqBandsCustom()
    {
        int count = 0;
        int totalBins = _FrequencyBins; // Use the total number of samples
        int binsPerBand = totalBins / _FreqBands;

        for (int i = 0; i < _FreqBands; i++)
        {
            float average = 0;
            int sampleCount = binsPerBand; // Fixed number of samples per band

            if (i == _FreqBands - 1) // Last band gets any extra samples
            {
                sampleCount += totalBins % _FreqBands;
            }

            for (int j = 0; j < sampleCount; j++)
            {
                average += _Samples[count] * (count + 1); // Weighted sum
                count++;
            }

            average /= count; // Divide by count to get weighted average
            _FreqBandsCustom[i] = average * (1 + (i * _Scalar));
        }
    }

    // Update is called once per frame
    public void freq_update()
    {
        //---   POPULATE SAMPLES
        _AudioSource.GetSpectrumData(_SampleBuffer, 0, FFTWindow.BlackmanHarris);

        for (int i = 0; i < _Samples.Length; i++)
        {
            if (_SampleBuffer[i] > _Samples[i])
                _Samples[i] = _SampleBuffer[i];
            else
                _Samples[i] = Mathf.Lerp(_Samples[i], _SampleBuffer[i], Time.deltaTime * _SmoothDownRate);
        }

        UpdateFreqBandsCustom();
    }

    private void Update()
    {
        float time = Time.time * rotationSpeed; // Rotation over time
        freq_update();

        for (int i = 0; i < GameObjects.Length; i++)
        {
            // Calculate the current angle with rotation over time
            float angle = i * (2f * Mathf.PI / GameObjects.Length) + time;
            float x = Mathf.Sin(angle) * radius;
            float z = Mathf.Cos(angle) * radius;

            // Move the object in a circle while adjusting Y position based on FFT value
            Vector3 newPosition = new Vector3(x, (_ScaleStrength.y) * _FreqBandsCustom[i], z) + displacement;
            GameObjects[i].transform.position = newPosition;

            if (LookAtCenter) {
                GameObjects[i].transform.LookAt(displacement);
                GameObjects[i].transform.localRotation *= Quaternion.Euler(-90, 0, 0);
            }
        }
    }
}