package com.companyname.weather.services

import android.annotation.SuppressLint
import android.location.Location
import android.os.Handler
import androidx.lifecycle.LifecycleService
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.Observer
import com.companyname.weather.services.nws.NWSGridPointsForecast
import com.companyname.weather.services.nws.NWSPoints
import com.companyname.weather.services.nws.NWSPointsStations
import com.companyname.weather.services.nws.NWSStationsObservations
import com.google.gson.Gson
import kotlinx.coroutines.GlobalScope
import kotlinx.coroutines.launch
import kotlinx.coroutines.sync.Mutex
import kotlinx.coroutines.sync.withLock
import java.lang.Exception
import java.net.URL
import java.text.SimpleDateFormat
import java.util.*

class NWSService : LifecycleService() {

    companion object {
        var instance: NWSService? = null

        var location: MutableLiveData<String> = MutableLiveData()
        var conditions: MutableLiveData<Conditions> = MutableLiveData()
        var forecasts: MutableLiveData<List<Forecast>> = MutableLiveData()

        private var timestamp: Long = 0
    }

    data class Conditions(
        val dewPoint: Double? = null,
        val relativeHumidity: Double? = null,
        val temperature: Double? = null,
        val windDirection: Double? = null,
        val windSpeed: Double? = null,
        val windGust: Double? = null,
        val icon: String? = null,
        val textDescription: String? = null,
        val barometricPressure: Double? = null,
        val visibility: Double? = null,
        val windChill: Double? = null,
        val heatIndex: Double? = null,
        val timestamp: Date? = null
    )

    data class Forecast(
        val name: String? = null,
        val icon: String? = null,
        val shortForecast: String? = null,
        val detailedForecast: String? = null,
        val temperature: Double? = null,
        val windSpeed: String? = null,
        val windDirection: String? = null,
        val temperatureTrend: String? = null,
        val isDaytime: Boolean = false
    )

    private val refreshInterval: Long = 1000 * 60 * 15

    //    private val refreshInterval: Long = 1000 * 15
    private val mutex = Mutex()
    private var lastLocation: Location = Location("")
    private var handler: Handler? = null

    private var stationId: String? = null
    private var gridX: Int? = null
    private var gridY: Int? = null
    private var gridWFO: String? = null
    private var baseURL = "https://api.weather.gov"

    override fun onCreate() {
        super.onCreate()
        instance = this

        LocationService.location.observe(this, Observer { setLocation(it) })

        handler = Handler()
        var runnable = Runnable { }
        runnable = Runnable {
            GlobalScope.launch { refresh() }
            handler?.postDelayed(runnable, refreshInterval)
        }
        handler?.postDelayed(runnable, refreshInterval)
    }

    override fun onDestroy() {
        super.onDestroy()
        instance = null
        handler?.removeCallbacks(null)
    }

    private fun setLocation(location: Location) {
        if (lastLocation.distanceTo(location) > 100) {
            lastLocation = location
            stationId = null
            gridX = null
            gridY = null
            gridWFO = null
            timestamp = 0
            GlobalScope.launch { refresh() }
        }
    }

    private suspend fun refresh() {
        mutex.withLock {
            val duration = Date().time - timestamp
            if (duration > refreshInterval) {
                timestamp = Date().time
                if ((lastLocation.latitude == 0.0 && lastLocation.longitude == 0.0)) {
                    location.postValue(null)
                    conditions.postValue(null)
                    forecasts.postValue(null)
                } else {
                    getStation()
                    getConditions()
                    getForecast()
                }
            }
        }
    }

    private fun getStation() {
        try {
            val latitude = lastLocation.latitude
            val longitude = lastLocation.longitude
            val url = URL("$baseURL/points/$latitude,$longitude/stations")
            val response = url.readText()
            val points = Gson().fromJson(response, NWSPointsStations.Root::class.java)
            location.postValue(points.features[0].properties.name)
            stationId = points.features[0].properties.stationIdentifier
        } catch (ex: Exception) {
        }
    }

    @SuppressLint("SimpleDateFormat")
    private fun getConditions() {
        stationId ?: return

        try {
            val url = URL("$baseURL/stations/$stationId/observations/latest?require_qc=false")
            val response = url.readText()
            val stations = Gson().fromJson(response, NWSStationsObservations.Root::class.java)

            conditions.postValue(
                Conditions(
                    dewPoint = stations.properties.dewpoint.value,
                    relativeHumidity = stations.properties.relativeHumidity.value,
                    temperature = stations.properties.temperature.value,
                    windDirection = stations.properties.windDirection.value,
                    windSpeed = stations.properties.windSpeed.value,
                    windGust = stations.properties.windGust.value,
                    icon = stations.properties.icon,
                    textDescription = stations.properties.textDescription,
                    barometricPressure = stations.properties.barometricPressure.value,
                    visibility = stations.properties.visibility.value,
                    windChill = stations.properties.windChill.value,
                    heatIndex = stations.properties.heatIndex.value,
                    timestamp = SimpleDateFormat("yyyy-MM-dd'T'HH:mm:sszzzzz").parse(stations.properties.timestamp)
                )
            )
        } catch (ex: Exception) {
        }
    }

    private fun getForecast() {
        if (listOf(gridX, gridY, gridWFO).any { it == null })
            getGridPoint()

        gridX ?: return
        gridY ?: return
        gridWFO ?: return

        try {
            val url = URL("$baseURL/gridpoints/$gridWFO/$gridX,$gridY/forecast")
            val response = url.readText()
            val forecast = Gson().fromJson(response, NWSGridPointsForecast.Root::class.java)

            forecasts.postValue(forecast.properties.periods.map {
                Forecast(
                    name = it.name,
                    icon = it.icon,
                    shortForecast = it.shortForecast,
                    detailedForecast = it.detailedForecast,
                    temperature = it.temperature,
                    windSpeed = it.windSpeed,
                    windDirection = it.windDirection,
                    temperatureTrend = it.temperatureTrend,
                    isDaytime = it.isDaytime
                )
            })
        } catch (ex: Exception) {
        }
    }

    private fun getGridPoint() {
        try {
            val latitude = lastLocation.latitude
            val longitude = lastLocation.longitude
            val url = URL("$baseURL/points/$latitude,$longitude")
            val response = url.readText()
            val points = Gson().fromJson(response, NWSPoints.Root::class.java)

            gridWFO = points.properties.cwa
            gridX = points.properties.gridX
            gridY = points.properties.gridY
        } catch (ex: Exception) {
        }
    }
}