using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SimpleJSON;
using UnityEngine.Networking;

public class WeatherManager : MonoBehaviour
{
    public string apiKey;
    public string currentWeatherApi = "http://api.openweathermap.org/data/2.5/weather?lat=" + 28.474388 + "&lon=" + 77.503990 + "&appid=201306&appid=ebe9e3e5669b1fce654dff10f5023b78&units=metric";
    [Header("UI")]
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI location;
    public TextMeshProUGUI mainWeather;
    public TextMeshProUGUI description;
    public TextMeshProUGUI temp;
    public TextMeshProUGUI feels_like;
    public TextMeshProUGUI temp_min;
    public TextMeshProUGUI temp_max;
    public TextMeshProUGUI pressure;
    public TextMeshProUGUI humidity;
    public TextMeshProUGUI windspeed;
    private LocationInfo lastLocation;
    void Start()
    {
        StartCoroutine(FetchLocationData());
    }
    private IEnumerator FetchLocationData()
    {
        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser)
            yield break;
        // Start service before querying location
        Input.location.Start();
        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }
        // Service didn't initialize in 20 seconds
        if (maxWait < 1)
        {
            statusText.text = "Location Timed out";
            yield break;
        }
        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            statusText.text = "Unable to determine device location";
            yield break;
        }
        else
        {
            lastLocation = Input.location.lastData;
            UpdateWeatherData();
        }
        Input.location.Stop();
    }
    private void UpdateWeatherData()
    {
        StartCoroutine(FetchWeatherDataFromApi(lastLocation.latitude.ToString(), lastLocation.longitude.ToString()));
    }
    private IEnumerator FetchWeatherDataFromApi(string latitude, string longitude)
    {
        string url = currentWeatherApi + "lat=" + latitude + "&lon=" + longitude + "&appid=" + apiKey + "&units=metric";
        UnityWebRequest fetchWeatherRequest = UnityWebRequest.Get(url);
        yield return fetchWeatherRequest.SendWebRequest();
        if (fetchWeatherRequest.result == UnityWebRequest.Result.ConnectionError)
        {
            //Check and print error
            statusText.text = fetchWeatherRequest.error;
        }
        else
        {
            Debug.Log(fetchWeatherRequest.downloadHandler.text);
            var response = JSON.Parse(fetchWeatherRequest.downloadHandler.text);
            location.text = response["name"];
            mainWeather.text = response["weather"][0]["main"];
            description.text = response["weather"][0]["description"];
            temp.text = response["main"]["temp"] + " C";
            feels_like.text = "Feels like " + response["main"]["feels_like"] + " C";
            temp_min.text = "Min is " + response["main"]["temp_min"] + " C";
            temp_max.text = "Max is " + response["main"]["temp_max"] + " C";
            pressure.text = "Pressure is "  + response["main"]["pressure"] +  " Pa";
            humidity.text = response["main"]["humidity"] + " % Humidity";
            windspeed.text = "Windspeed is " + response["wind"]["speed"] + " Km/h";
        }
    }
}
